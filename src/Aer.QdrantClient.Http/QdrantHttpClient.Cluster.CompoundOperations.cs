using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Collections;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Helpers;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <summary>
    /// Replicates shards for specified or all collections to specified peer.
    /// </summary>
    /// <param name="targetPeerUriSelectorString">The peer uri selector string for the peer to replicate shards to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isIgnoreReplicationFactor">
    /// If set to <c>false</c> - does not replicate collection more than its configured replication factor.
    /// </param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="collectionNamesToReplicate">
    /// Collection names to replicate shards for.
    /// If <c>null</c> or empty - replicates all collection shards.
    /// </param>
    public async Task<ReplicateShardsToPeerResponse> ReplicateShardsToPeer(
        string targetPeerUriSelectorString,
        CancellationToken cancellationToken,
        bool isIgnoreReplicationFactor = true,
        ILogger logger = null,
        bool isDryRun = false,
        params string[] collectionNamesToReplicate)
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            var (targetPeerId, _, sourcePeerIds, peerUriPerPeerId) =
                (await GetPeerInfoByUriSubstring(targetPeerUriSelectorString, cancellationToken)).EnsureSuccess();

            var collectionInfos = (await ListCollectionInfo(
                isCountExactPointsNumber: false,
                cancellationToken)).EnsureSuccess();

            var sourcePeers = new CircularEnumerable<ulong>(sourcePeerIds);

            var collectionNames = collectionInfos.Keys.ToArray();

            if (collectionNamesToReplicate is {Length: > 0})
            {
                // check tant all collection names are stated correctly
                // this method gets called very rarely, so we don't bother with HashSet

                foreach (var collectionNameFromParameter in collectionNamesToReplicate)
                {
                    if (!collectionInfos.ContainsKey(collectionNameFromParameter))
                    {
                        sw.Stop();

                        return new ReplicateShardsToPeerResponse()
                        {
                            Result = false,
                            Status = QdrantStatus.Fail(
                                $"Collection '{collectionNameFromParameter}' does not exist, check parameters"),
                            Time = sw.Elapsed.TotalMinutes
                        };
                    }
                }

                collectionNames = collectionNamesToReplicate;
            }

            logger?.LogInformation(
                "Going to replicate shards for collections {CollectionNames} from peers {PeersToMoveShardsFrom} to peer {PeerToMoveShardsTo} ({PeerUri})",
                collectionInfos.Keys,
                sourcePeerIds,
                targetPeerId,
                peerUriPerPeerId[targetPeerId]
            );

            foreach (var collectionName in collectionNames)
            {
                var collectionShardingInfo =
                    (await GetCollectionClusteringInfo(collectionName, cancellationToken))
                    .EnsureSuccess();

                var collectionInfo = collectionInfos[collectionName];

                var collectionTargetReplicationFactor = collectionInfo.Config.Params.ReplicationFactor ?? 0U;

                if (collectionTargetReplicationFactor == 0U)
                {
                    logger?.LogError(
                        "Collection '{CollectionName}' replication factor is unknown. Skipping collection shards replication",
                        collectionName);

                    continue;
                }

                var (collectionShardsPerPeers, shardsWithReplicationFactors) =
                    GetShardIdsPerPeerIdsAndReplicationFactors(
                        collectionName,
                        collectionShardingInfo,
                        collectOnlyActiveShards: true,
                        logger);

                // collect source shards to replicate and peers to replicate shards from

                List<(uint sourceShardId, ulong sourcePeerId)> shardReplicationSources = new();

                foreach (var (sourceShardId, effectiveSourceShardReplicationFactor) in shardsWithReplicationFactors)
                {
                    var isSourceShardReplicationFactorReached = !isIgnoreReplicationFactor
                        &&
                        effectiveSourceShardReplicationFactor >= collectionTargetReplicationFactor;

                    // target peer either does not have any shards on it or does not have the source shard
                    var targetPeerDoesNotHaveSourceShard = !collectionShardsPerPeers.ContainsKey(targetPeerId)
                        || !collectionShardsPerPeers[targetPeerId].Contains(sourceShardId);

                    if (!isSourceShardReplicationFactorReached
                        && targetPeerDoesNotHaveSourceShard)
                    {
                        var candidateSourcePeer = sourcePeers.GetNext();

                        using var _ = sourcePeers.StartCircleDetection();

                        if (collectionShardsPerPeers.ContainsKey(candidateSourcePeer))
                        {
                            while (!collectionShardsPerPeers[candidateSourcePeer].Contains(sourceShardId))
                            {
                                // here we should not get an infinite cycle since
                                // at least one peer will contain a source shard replica
                                // but if we do - circe detection in CircularEnumerable should throw
                                candidateSourcePeer = sourcePeers.GetNext();
                            }
                        }

                        shardReplicationSources.Add((sourceShardId, candidateSourcePeer));
                    }
                    else
                    {
                        if (isSourceShardReplicationFactorReached)
                        {
                            // means collection replication factor reached
                            logger?.LogInformation(
                                "Collection '{CollectionName}' shard {ShardId} already replicated {EffectiveReplicationFactor} times, which is target collection replication factor, or target peer {TargetPeerId} ({PeerUri}) already contains this shard",
                                collectionName,
                                sourceShardId,
                                effectiveSourceShardReplicationFactor,
                                targetPeerId,
                                peerUriPerPeerId[targetPeerId]
                            );
                        }

                        if (!targetPeerDoesNotHaveSourceShard)
                        {
                            // shard already exists on target peer
                            logger?.LogInformation(
                                "Collection '{CollectionName}' shard {ShardId} already exists on a target peer {TargetPeerId} ({PeerUri})",
                                collectionName,
                                sourceShardId,
                                targetPeerId,
                                peerUriPerPeerId[targetPeerId]
                            );
                        }
                    }
                }

                foreach (var (sourceShardId, shardReplicaSourcePeerId) in shardReplicationSources)
                {
                    logger?.LogInformation(
                        "Going to replicate collection '{CollectionName}' shard {ShardId} from peer {SourcePeer} ({SourcePeerUri}) to peer {TargetPeer} ({TargetPeerUri})",
                        collectionName,
                        sourceShardId,
                        shardReplicaSourcePeerId,
                        peerUriPerPeerId[shardReplicaSourcePeerId],
                        targetPeerId,
                        peerUriPerPeerId[targetPeerId]
                    );

                    if (!isDryRun)
                    {
                        var isSuccessfullyStartOperationResponse = await UpdateCollectionClusteringSetup(
                            collectionName,
                            UpdateCollectionClusteringSetupRequest.CreateReplicateShardRequest(
                                shardId: sourceShardId,
                                fromPeerId: shardReplicaSourcePeerId,
                                toPeerId: targetPeerId),
                            cancellationToken);

                        if (!isSuccessfullyStartOperationResponse.Status.IsSuccess
                            || isSuccessfullyStartOperationResponse.Result is false)
                        {
                            logger?.LogError(
                                "Error replicating collection '{CollectionName}' shard {ShardId} from peer {SourcePeer} ({SourcePeerUri}) to peer {TargetPeer} ({TargetPeerUri}): {ErrorMessage}",
                                collectionName,
                                sourceShardId,
                                shardReplicaSourcePeerId,
                                peerUriPerPeerId[shardReplicaSourcePeerId],
                                targetPeerId,
                                peerUriPerPeerId[targetPeerId],
                                isSuccessfullyStartOperationResponse.Status.GetErrorMessage());

                            // means issuing replicate operation failed - abandon move

                            sw.Stop();

                            return new ReplicateShardsToPeerResponse()
                            {
                                Result = false,
                                Status = isSuccessfullyStartOperationResponse.Status,
                                Time = sw.Elapsed.TotalMinutes
                            };
                        }
                    }
                    else
                    {
                        logger?.LogInformation("Shard move simulation mode ON. No shards replicated");
                    }
                }
            }

            sw.Stop();

            return new ReplicateShardsToPeerResponse()
            {
                Result = true,
                Status = QdrantStatus.Success(),
                Time = sw.Elapsed.TotalMinutes
            };
        }
        catch (QdrantUnsuccessfulResponseStatusException qex)
        {
            sw.Stop();

            return new ReplicateShardsToPeerResponse()
            {
                Result = false,
                Status = QdrantStatus.Fail(qex.Message, qex),
                Time = sw.Elapsed.TotalMinutes
            };
        }
    }

    /// <summary>
    /// Removes all shards for all collections or specified collections from a peer by distributing them between another peers.
    /// </summary>
    /// <param name="peerToEmptyUriSelectorString">The peer uri selector string for the peer to move shards away from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="collectionNamesToMove">
    /// Collection names to move shards for.
    /// If <c>null</c> or empty - moves all collection shards.
    /// </param>
    public async Task<DrainPeerResponse> DrainPeer(
        string peerToEmptyUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        params string[] collectionNamesToMove
    )
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            var (sourcePeerId, _, targetPeerIds, peerUriPerPeerId) =
                (await GetPeerInfoByUriSubstring(peerToEmptyUriSelectorString, cancellationToken)).EnsureSuccess();

            var collectionNames = (await ListCollections(cancellationToken))
                .EnsureSuccess()
                .Collections
                .Select(c => c.Name)
                .ToArray();

            if (collectionNamesToMove is {Length: > 0})
            {
                // check tant all collection names are stated correctly
                // this method gets called very rarely, so we don't bother with HashSet

                foreach (var collectionNameFromParameter in collectionNamesToMove)
                {
                    if (!collectionNames.Contains(collectionNameFromParameter, StringComparer.InvariantCulture))
                    {
                        sw.Stop();

                        return new DrainPeerResponse()
                        {
                            Result = false,
                            Status = QdrantStatus.Fail(
                                $"Collection '{collectionNameFromParameter}' does not exist, check parameters"),
                            Time = sw.Elapsed.TotalMinutes
                        };
                    }
                }

                collectionNames = collectionNamesToMove;
            }

            var targetPeers = new CircularEnumerable<ulong>(targetPeerIds);

            logger?.LogInformation(
                "Going to move all active shards for collections '{CollectionNames}' from peer {PeerToEmpty} ({PeerUri}) to peers {PeersToMoveShardsTo}",
                collectionNames,
                sourcePeerId,
                peerUriPerPeerId[sourcePeerId],
                targetPeerIds);

            foreach (var collectionName in collectionNames)
            {
                var collectionShardingInfo =
                    (await GetCollectionClusteringInfo(collectionName, cancellationToken))
                    .EnsureSuccess();

                var (collectionShardsPerPeers, _) =
                    GetShardIdsPerPeerIdsAndReplicationFactors(
                        collectionName,
                        collectionShardingInfo,
                        collectOnlyActiveShards: true,
                        logger);

                if (!collectionShardsPerPeers.TryGetValue(sourcePeerId, out HashSet<uint> shardsIdsToMoveAwayFromPeer))
                {
                    logger?.LogInformation(
                        "Collection '{CollectionName}' has no shards on peer {SourcePeerId} ({SourceUri})",
                        collectionName,
                        sourcePeerId,
                        peerUriPerPeerId[sourcePeerId]);

                    continue;
                }

                Queue<uint> shardsIdsOnPeerToEmpty = new(shardsIdsToMoveAwayFromPeer);

                var maximalShardCountPerPeer = Math.Max(
                    1,
                    shardsIdsOnPeerToEmpty.Count / targetPeers.Count
                );

                while (shardsIdsOnPeerToEmpty.Count > 0)
                {
                    var targetPeerId = targetPeers.GetNext();

                    var shardsToMoveToPeer = shardsIdsOnPeerToEmpty.DequeueAtMost(maximalShardCountPerPeer);

                    foreach (var shardToMoveToPeer in shardsToMoveToPeer)
                    {
                        logger?.LogInformation(
                            "Going to move collection '{CollectionName}' shard {ShardId} from peer {SourcePeer} ({SourcePeerUri}) to peer {TargetPeer} ({TargetPeerUri})",
                            collectionName,
                            shardToMoveToPeer,
                            sourcePeerId,
                            peerUriPerPeerId[sourcePeerId],
                            targetPeerId,
                            peerUriPerPeerId[targetPeerId]
                        );

                        if (!isDryRun)
                        {
                            var isSuccessfullyStartOperationResponse = await UpdateCollectionClusteringSetup(
                                collectionName,
                                UpdateCollectionClusteringSetupRequest.CreateMoveShardRequest(
                                    shardId: shardToMoveToPeer,
                                    fromPeerId: sourcePeerId,
                                    toPeerId: targetPeerId,
                                    ShardTransferMethod.StreamRecords),
                                cancellationToken);

                            if (!isSuccessfullyStartOperationResponse.Status.IsSuccess
                                || isSuccessfullyStartOperationResponse.Result is false)
                            {
                                logger?.LogError(
                                    "Error moving collection '{CollectionName}' shard {ShardId} from peer {SourcePeer} ({SourcePeerUri}) to peer {TargetPeer} ({TargetPeerUri}): {ErrorMessage}",
                                    collectionName,
                                    shardToMoveToPeer,
                                    sourcePeerId,
                                    peerUriPerPeerId[sourcePeerId],
                                    targetPeerId,
                                    peerUriPerPeerId[targetPeerId],
                                    isSuccessfullyStartOperationResponse.Status.GetErrorMessage());

                                // means issuing move operation failed - abandon move

                                sw.Stop();

                                return new DrainPeerResponse()
                                {
                                    Result = false,
                                    Status = isSuccessfullyStartOperationResponse.Status,
                                    Time = sw.Elapsed.TotalMinutes
                                };
                            }
                        }
                        else
                        {
                            logger?.LogInformation("Shard move simulation mode ON. No shards moved");
                        }
                    }
                }

                targetPeers.Reset();
            }

            sw.Stop();

            return new DrainPeerResponse()
            {
                Result = true,
                Status = QdrantStatus.Success(),
                Time = sw.Elapsed.TotalMinutes
            };
        }
        catch (QdrantUnsuccessfulResponseStatusException qex)
        {
            sw.Stop();

            return new DrainPeerResponse()
            {
                Result = false,
                Status = QdrantStatus.Fail(qex.Message, qex),
                Time = sw.Elapsed.TotalMinutes
            };
        }
    }

    /// <summary>
    /// Checks whether the specified cluster node does not have any collection shards on it.
    /// </summary>
    /// <param name="clusterNodeUriSubstring">The cluster node URI substring.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<CheckIsPeerEmptyResponse> CheckIsPeerEmpty(
        string clusterNodeUriSubstring,
        CancellationToken cancellationToken)
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            var (peerIdToCheck, _, _, _) =
                (await GetPeerInfoByUriSubstring(clusterNodeUriSubstring, cancellationToken)).EnsureSuccess();

            var collectionNames = (await ListCollections(cancellationToken))
                .EnsureSuccess()
                .Collections
                .Select(c => c.Name);

            bool isPeerEmpty = true;

            // check that none of the collections have shards on the found peer
            foreach (var collectionName in collectionNames)
            {
                var collectionShardingInfo =
                    (await GetCollectionClusteringInfo(collectionName, cancellationToken))
                    .EnsureSuccess();

                bool isPeerEmptyForCollection;

                if (collectionShardingInfo.PeerId == peerIdToCheck)
                {
                    // means we are on the peer to check - check if no shards on this peer
                    isPeerEmptyForCollection = collectionShardingInfo.LocalShards.Length == 0;
                }
                else
                {
                    // means we are not on the peer to check - check if no remote shards on peer to check
                    isPeerEmptyForCollection = collectionShardingInfo.RemoteShards.All(si => si.PeerId != peerIdToCheck);
                }

                isPeerEmpty &= isPeerEmptyForCollection;
            }

            sw.Stop();

            return new CheckIsPeerEmptyResponse()
            {
                Result = isPeerEmpty,
                Status = QdrantStatus.Success(),
                Time = sw.Elapsed.TotalSeconds
            };
        }
        catch (QdrantUnsuccessfulResponseStatusException qex)
        {
            sw.Stop();

            return new CheckIsPeerEmptyResponse()
            {
                Result = false,
                Status = QdrantStatus.Fail(qex.Message, qex),
                Time = sw.Elapsed.TotalSeconds
            };
        }
    }

    /// <summary>
    /// Gets the peer information by the peer node uri substring. Returns the found peer and other peers.
    /// </summary>
    /// <param name="clusterNodeUriSubstring">Cluster node uri substring to get peer info for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="QdrantNoNodesFoundForUriSubstringException">Occurs when no nodes found for uri substring.</exception>
    /// <exception cref="QdrantMoreThanOneNodeFoundForUriSubstringException">Occurs when more than one node found for uri substring.</exception>
    public async Task<GetPeerResponse>
        GetPeerInfoByUriSubstring(
            string clusterNodeUriSubstring,
            CancellationToken cancellationToken)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var clusterInfo = await GetClusterInfoInternal(cancellationToken);

        var peerIdByNodeUrl = clusterInfo.ParsedPeers.ToDictionary(
            ci => ci.Value.Uri,
            ci => ci.Key);

        var candidatePeersIds = peerIdByNodeUrl.Where(
                kv => kv.Key.Contains(clusterNodeUriSubstring, StringComparison.InvariantCulture))
            .ToList();

        if (candidatePeersIds.Count == 0)
        {
            throw new QdrantNoNodesFoundForUriSubstringException(
                clusterNodeUriSubstring,
                peerIdByNodeUrl
            );
        }

        if (candidatePeersIds.Count > 1)
        {
            throw new QdrantMoreThanOneNodeFoundForUriSubstringException(
                clusterNodeUriSubstring,
                candidatePeersIds);
        }

        var nodePeerId = candidatePeersIds[0].Value;

        var otherPeersIds = peerIdByNodeUrl.Where(
                kv => kv.Value != nodePeerId)
            .Select(kv => kv.Value)
            .ToList();

        Dictionary<ulong, string> peerUriPerPeerId = new();

        foreach (var peer in peerIdByNodeUrl)
        {
            peerUriPerPeerId.Add(peer.Value, peer.Key);
        }

        var ret = new GetPeerResponse.PeerInfo()
        {
            PeerId = nodePeerId,
            PeerUri = candidatePeersIds[0].Key,
            OtherPeerIds = otherPeersIds,
            PeerUriPerPeerIds = peerUriPerPeerId
        };

        sw.Stop();

        return new GetPeerResponse()
        {
            Status = QdrantStatus.Success(),
            Result = ret,
            Time = sw.Elapsed.TotalSeconds
        };
    }

    private (Dictionary<ulong, HashSet<uint>> ShardsPerPeers, Dictionary<uint, uint> ShardReplicationFactors)
        GetShardIdsPerPeerIdsAndReplicationFactors(
            string collectionName,
            GetCollectionClusteringInfoResponse.CollectionClusteringInfo collectionShardingInfo,
            bool collectOnlyActiveShards,
            ILogger logger)
    {
        Dictionary<uint, uint> shardReplicationFactors = new();

        Dictionary<ulong, HashSet<uint>> collectionShardsPerPeers = new()
        {
            {collectionShardingInfo.PeerId, new()}
        };

        // collect local shards
        foreach (var localShard in collectionShardingInfo.LocalShards)
        {
            if (collectOnlyActiveShards && localShard.State != ShardState.Active)
            {
                logger?.LogInformation(
                    "Shard {ShardId} for collection '{CollectionName}' has non-active state '{ShardState}' on peer {PeerId}. Shard won't be processed",
                    localShard.ShardId,
                    collectionName,
                    localShard.State.ToString(),
                    collectionShardingInfo.PeerId
                );

                continue;
            }

            collectionShardsPerPeers[collectionShardingInfo.PeerId].Add(localShard.ShardId);

            if (!shardReplicationFactors.TryAdd(localShard.ShardId, 1))
            {
                shardReplicationFactors[localShard.ShardId]++;
            }
        }

        // collect remote shards
        foreach (var remoteShard in collectionShardingInfo.RemoteShards)
        {
            if (collectOnlyActiveShards && remoteShard.State != ShardState.Active)
            {
                logger?.LogInformation(
                    "Shard {ShardId} for collection '{CollectionName}' has non-active state '{ShardState}' on peer {PeerId}. Shard won't be processed",
                    remoteShard.ShardId,
                    collectionName,
                    remoteShard.State.ToString(),
                    remoteShard.PeerId
                );

                continue;
            }

            if (!collectionShardsPerPeers.ContainsKey(remoteShard.PeerId))
            {
                collectionShardsPerPeers.Add(remoteShard.PeerId, new());
            }

            if (!shardReplicationFactors.TryAdd(remoteShard.ShardId, 1))
            {
                shardReplicationFactors[remoteShard.ShardId]++;
            }

            collectionShardsPerPeers[remoteShard.PeerId].Add(remoteShard.ShardId);
        }

        return (collectionShardsPerPeers, shardReplicationFactors);
    }

    private async Task<GetClusterInfoResponse.ClusterInfo> GetClusterInfoInternal(CancellationToken cancellationToken)
    {
        var getClusterInfoResponse = await GetClusterInfo(cancellationToken);

        getClusterInfoResponse.EnsureSuccess();

        return getClusterInfoResponse.Result;
    }
}
