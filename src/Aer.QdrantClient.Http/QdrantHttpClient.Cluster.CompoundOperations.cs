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
    /// Replicates shards for specified or all collections to specified cluster node if shard is not already replicated to specified node.
    /// </summary>
    /// <param name="targetClusterNodeSelectorString">The cluster node selector string for the node to replicate shards to.</param>
    /// <param name="collectionNamesToReplicate">
    /// Filter for replicating only shards for specified collections.
    /// If <c>null</c> or empty - replicates all collection shards.
    /// </param>
    /// <param name="isIgnoreReplicationFactor">If set to <c>true</c> - allows replicating collection more than its replication factor.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    public async Task<ReplicateShardsToPeerResponse> ReplicateShardsToClusterNode(
        string targetClusterNodeSelectorString,
        string[] collectionNamesToReplicate,
        bool isIgnoreReplicationFactor,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false)
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            var (peerIdToPopulate, _, peerIdsToReplicateShardsFrom, peerUriPerPeerId) =
                (await GetPeerInfoByNodeUriSubstring(targetClusterNodeSelectorString, cancellationToken)).EnsureSuccess();

            var collectionInfos = await ListCollectionInfo(
                isCountExactPointsNumber: false,
                cancellationToken);

            var peersToReplicateShardsFrom = new CircularEnumerable<ulong>(peerIdsToReplicateShardsFrom);

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
                peerIdsToReplicateShardsFrom,
                peerIdToPopulate,
                peerUriPerPeerId[peerIdToPopulate]
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

                var (collectionShardsPerPeers, shardReplicationFactors) =
                    GetShardIdsPerPeerIdsAndReplicationFactors(
                        collectionName,
                        collectionShardingInfo,
                        collectOnlyActiveShards: true,
                        logger);

                // collect shards to replicate and peers to replicate shards from

                List<(uint shardId, ulong sourcePeerId)> shardReplicationSources = new();

                foreach (var (shardId, effectiveShardReplicationFactor) in shardReplicationFactors)
                {
                    bool shouldReplicateShard =
                        isIgnoreReplicationFactor
                        || effectiveShardReplicationFactor < collectionTargetReplicationFactor;

                    if (shouldReplicateShard
                        &&
                        // target peer either does not have any shards on it or does not have this particular shard
                        (!collectionShardsPerPeers.ContainsKey(peerIdToPopulate)
                            || !collectionShardsPerPeers[peerIdToPopulate].Contains(shardId)))
                    {
                        var candidateSourcePeer = peersToReplicateShardsFrom.GetNext();

                        if (collectionShardsPerPeers.ContainsKey(candidateSourcePeer))
                        {
                            while (!collectionShardsPerPeers[candidateSourcePeer].Contains(shardId))
                            {
                                // here we should not get an infinite cycle since at least one peer will contain shard replica
                                candidateSourcePeer = peersToReplicateShardsFrom.GetNext();
                            }
                        }

                        shardReplicationSources.Add((shardId, candidateSourcePeer));
                    }
                    else
                    {
                        if (!shouldReplicateShard)
                        {
                            // means collection replication factor reached
                            logger?.LogInformation(
                                "Collection '{CollectionName}' shard {ShardId} already replicated {EffectiveReplicationFactor} times, which is target collection replication factor, or target peer {TargetPeerId} ({PeerUri}) already contains this shard",
                                collectionName,
                                shardId,
                                effectiveShardReplicationFactor,
                                peerIdToPopulate,
                                peerUriPerPeerId[peerIdToPopulate]
                            );
                        }
                        else
                        {
                            // means shard already exists on target peer
                            logger?.LogInformation(
                                "Collection '{CollectionName}' shard {ShardId} already exists on a target peer {TargetPeerId} ({PeerUri})",
                                collectionName,
                                shardId,
                                peerIdToPopulate,
                                peerUriPerPeerId[peerIdToPopulate]
                            );
                        }
                    }
                }

                foreach (var (shardId, shardReplicaSourcePeerId) in shardReplicationSources)
                {
                    logger?.LogInformation(
                        "Going to replicate collection '{CollectionName}' shard {ShardId} from peer {SourcePeer} ({SourcePeerUri}) to peer {TargetPeer} ({TargetPeerUri})",
                        collectionName,
                        shardId,
                        shardReplicaSourcePeerId,
                        peerUriPerPeerId[shardReplicaSourcePeerId],
                        peerIdToPopulate,
                        peerUriPerPeerId[peerIdToPopulate]
                    );

                    if (!isDryRun)
                    {
                        var isSuccessfullyStartOperationResponse = await UpdateCollectionClusteringSetup(
                            collectionName,
                            UpdateCollectionClusteringSetupRequest.CreateReplicateShardRequest(
                                shardId: shardId,
                                fromPeerId: shardReplicaSourcePeerId,
                                toPeerId: peerIdToPopulate),
                            cancellationToken);

                        if (!isSuccessfullyStartOperationResponse.Status.IsSuccess
                            || isSuccessfullyStartOperationResponse.Result is false)
                        {
                            logger?.LogError(
                                "Error replicating collection '{CollectionName}' shard {ShardId} from peer {SourcePeer} ({SourcePeerUri}) to peer {TargetPeer} ({TargetPeerUri}): {ErrorMessage}",
                                collectionName,
                                shardId,
                                shardReplicaSourcePeerId,
                                peerUriPerPeerId[shardReplicaSourcePeerId],
                                peerIdToPopulate,
                                peerUriPerPeerId[peerIdToPopulate],
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
    /// Removes all shards from a cluster node by either moving them physically or dropping their replicas.
    /// </summary>
    /// <param name="clusterNodeToEmptySelectorString">The cluster node slector string for the node to move shards away from.</param>
    /// <param name="collectionNamesToMove">
    /// Filter for moving only shards for specified collections.
    /// If <c>null</c> or empty - moves all collection shards.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    public async Task<DrainPeerResponse> DrainPeer(
        string clusterNodeToEmptySelectorString,
        string[] collectionNamesToMove,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false)
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            var (peerIdToEmpty, _, peerIdsToMoveShardsTo, peerUriPerPeerId) =
                (await GetPeerInfoByNodeUriSubstring(clusterNodeToEmptySelectorString, cancellationToken)).EnsureSuccess();

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

            var peersToMoveShardsTo = new CircularEnumerable<ulong>(peerIdsToMoveShardsTo);

            logger?.LogInformation(
                "Going to move all active shards for collections '{CollectionNames}' from peer {PeerToEmpty} ({PeerUri}) to peers {PeersToMoveShardsTo}",
                collectionNames,
                peerIdToEmpty,
                peerUriPerPeerId[peerIdToEmpty],
                peerIdsToMoveShardsTo);

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

                if (!collectionShardsPerPeers.TryGetValue(peerIdToEmpty, out HashSet<uint> shardsIdsToMoveAwayFromPeer))
                {
                    logger?.LogInformation(
                        "Collection '{CollectionName}' has no shards on peer {SourcePeerId} ({SourceUri})",
                        collectionName,
                        peerIdToEmpty,
                        peerUriPerPeerId[peerIdToEmpty]);

                    continue;
                }

                Queue<uint> shardsIdsOnPeerToEmpty = new(shardsIdsToMoveAwayFromPeer);

                var maximalShardCountPerPeer = Math.Max(
                    1,
                    shardsIdsOnPeerToEmpty.Count / peersToMoveShardsTo.Count
                );

                while (shardsIdsOnPeerToEmpty.Count > 0)
                {
                    var peerToMoveShardsTo = peersToMoveShardsTo.GetNext();

                    var shardsToMoveToPeer = shardsIdsOnPeerToEmpty.DequeueAtMost(maximalShardCountPerPeer);

                    foreach (var shardToMoveToPeer in shardsToMoveToPeer)
                    {
                        if (collectionShardsPerPeers.ContainsKey(peerToMoveShardsTo)
                            && collectionShardsPerPeers[peerToMoveShardsTo].Contains(shardToMoveToPeer))
                        {
                            // means target peer already contains replica of this shard
                            // just drop shard replica on peer to empty

                            logger?.LogInformation(
                                "Collection '{CollectionName}' shard {ShardId} already exists on peer {TargetPeer} ({TargetPeerUri}). Going to just drop replica from peer {SourcePeer} ({SourcePeerUri})",
                                collectionName,
                                shardToMoveToPeer,
                                peerToMoveShardsTo,
                                peerUriPerPeerId[peerToMoveShardsTo],
                                peerIdToEmpty,
                                peerUriPerPeerId[peerIdToEmpty]);

                            if (!isDryRun)
                            {
                                var isSuccessfullyStartOperationResponse = await UpdateCollectionClusteringSetup(
                                    collectionName,
                                    UpdateCollectionClusteringSetupRequest.CreateDropShardReplicaRequest(
                                        shardId: shardToMoveToPeer,
                                        peerId: peerIdToEmpty),
                                    cancellationToken);

                                if (!isSuccessfullyStartOperationResponse.Status.IsSuccess
                                    || isSuccessfullyStartOperationResponse.Result is false)
                                {
                                    logger?.LogError(
                                        "Error dropping collection '{CollectionName}' shard {ShardId} from peer {SourcePeer} ({SourcePeerUri}): {ErrorMessage}",
                                        collectionName,
                                        shardToMoveToPeer,
                                        peerIdToEmpty,
                                        peerUriPerPeerId[peerIdToEmpty],
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
                                logger?.LogInformation("Shard move simulation mode ON. No shards dropped");
                            }
                        }
                        else
                        {
                            logger?.LogInformation(
                                "Going to move collection '{CollectionName}' shard {ShardId} from peer {SourcePeer} ({SourcePeerUri}) to peer {TargetPeer} ({TargetPeerUri})",
                                collectionName,
                                shardToMoveToPeer,
                                peerIdToEmpty,
                                peerUriPerPeerId[peerIdToEmpty],
                                peerToMoveShardsTo,
                                peerUriPerPeerId[peerToMoveShardsTo]
                            );

                            if (!isDryRun)
                            {
                                var isSuccessfullyStartOperationResponse = await UpdateCollectionClusteringSetup(
                                    collectionName,
                                    UpdateCollectionClusteringSetupRequest.CreateMoveShardRequest(
                                        shardId: shardToMoveToPeer,
                                        fromPeerId: peerIdToEmpty,
                                        toPeerId: peerToMoveShardsTo,
                                        ShardTransferMethod.StreamRecords),
                                    cancellationToken);

                                if (!isSuccessfullyStartOperationResponse.Status.IsSuccess
                                    || isSuccessfullyStartOperationResponse.Result is false)
                                {
                                    logger?.LogError(
                                        "Error moving collection '{CollectionName}' shard {ShardId} from peer {SourcePeer} ({SourcePeerUri}) to peer {TargetPeer} ({TargetPeerUri}): {ErrorMessage}",
                                        collectionName,
                                        shardToMoveToPeer,
                                        peerIdToEmpty,
                                        peerUriPerPeerId[peerIdToEmpty],
                                        peerToMoveShardsTo,
                                        peerUriPerPeerId[peerToMoveShardsTo],
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
                }

                peersToMoveShardsTo.Reset();
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
    public async Task<CheckIsPeerEmptyResponse> CheckIsClusterNodeEmpty(
        string clusterNodeUriSubstring,
        CancellationToken cancellationToken)
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            var (peerIdToCheck, _, _, _) =
                (await GetPeerInfoByNodeUriSubstring(clusterNodeUriSubstring, cancellationToken)).EnsureSuccess();

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
    /// <exception cref="QdrantNoNodesFoundForUriSubstringException">Occures when no nodes found for uri substring.</exception>
    /// <exception cref="QdrantMoreThanOneNodeFoundForUriSubstringException">Occures when more than one node found for uri substring.</exception>
    public async Task<GetPeerResponse>
        GetPeerInfoByNodeUriSubstring(
            string clusterNodeUriSubstring,
            CancellationToken cancellationToken)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var clusterInfo = await GetClusterInfoInternal(cancellationToken);

        var peerIdByNodeUrl = clusterInfo.Peers.ToDictionary(
            ci => ci.Value.Uri,
            ci => ulong.Parse(ci.Key));

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
