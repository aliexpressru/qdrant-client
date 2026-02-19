using Aer.QdrantClient.Http.Collections;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Helpers;
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
using Aer.QdrantClient.Http.Infrastructure.Replication;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;
using MoreLinq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <inheritdoc/>
    public async Task<ReplicateShardsToPeerResponse> ReplicateShards(
        ulong sourcePeerId,
        ulong targetPeerId,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        bool isMoveShards = false,
        string[] collectionNamesToReplicate = null,
        uint[] shardIdsToReplicate = null,
        string clusterName = null)
    {
        var sourcePeerInfo = await GetPeerInfo(
            sourcePeerId,
            cancellationToken,
            clusterName: clusterName);

        var targetPeerInfo = await GetPeerInfo(
            targetPeerId,
            cancellationToken,
            clusterName: clusterName);

        return await ReplicateShardsInternal(
            sourcePeerInfo,
            targetPeerInfo,
            shardTransferMethod,
            cancellationToken,
            logger,
            isDryRun,
            collectionNamesToReplicate,
            shardIdsToReplicate ?? [],
            isMoveShards,
            clusterName: clusterName);
    }

    /// <inheritdoc/>
    public async Task<ReplicateShardsToPeerResponse> ReplicateShards(
        string sourcePeerUriSelectorString,
        string targetPeerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        bool isMoveShards = false,
        string[] collectionNamesToReplicate = null,
        uint[] shardIdsToReplicate = null,
        string clusterName = null)
    {
        var sourcePeerInfo = await GetPeerInfo(
            sourcePeerUriSelectorString,
            cancellationToken,
            clusterName: clusterName);

        var targetPeerInfo = await GetPeerInfo(
            targetPeerUriSelectorString,
            cancellationToken,
            clusterName: clusterName);

        return await ReplicateShardsInternal(
            sourcePeerInfo,
            targetPeerInfo,
            shardTransferMethod,
            cancellationToken,
            logger,
            isDryRun,
            collectionNamesToReplicate,
            shardIdsToReplicate ?? [],
            isMoveShards,
            clusterName: clusterName);
    }

    private async Task<ReplicateShardsToPeerResponse> ReplicateShardsInternal(
        GetPeerResponse sourcePeerInfoResponse,
        GetPeerResponse targetPeerInfoResponse,
        ShardTransferMethod shardTransferMethod,
        CancellationToken cancellationToken,
        ILogger logger,
        bool isDryRun,
        string[] collectionNamesToReplicate,
        uint[] shardIdsToReplicate,
        bool isMoveShards,
        string clusterName = null)
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            var (sourcePeerId, _, _, peerUriPerPeerId) = sourcePeerInfoResponse.EnsureSuccess();
            var (targetPeerId, _, _, _) = targetPeerInfoResponse.EnsureSuccess();

            var collectionInfos = (await ListCollectionInfo(
                isCountExactPointsNumber: false,
                cancellationToken,
                clusterName: clusterName)).EnsureSuccess();

            string[] collectionNames;

            if (collectionNamesToReplicate is { Length: > 0 })
            {
                // check that all collection names are stated correctly
                foreach (var collectionNameFromParameter in collectionNamesToReplicate)
                {
                    if (!collectionInfos.ContainsKey(collectionNameFromParameter))
                    {
                        sw.Stop();

                        return ReplicateShardsToPeerResponse.Fail(
                            QdrantStatus.Fail(
                                $"Collection '{collectionNameFromParameter}' does not exist, check parameters"),
                            sw.Elapsed.TotalSeconds
                        );
                    }
                }

                collectionNames = collectionNamesToReplicate;
            }
            else
            {
                collectionNames = [.. collectionInfos.Keys];
            }

            if (logger?.IsEnabled(LogLevel.Information) == true)
            {
                logger?.LogInformation(
                    "Going to replicate shards for collections {CollectionNames} from peer {PeerToMoveShardsFrom}({PeerToMoveShardsFromUri}) to peer {PeerToMoveShardsTo}({PeerToMoveShardsToUri})",
                    collectionInfos.Keys,
                    sourcePeerId,
                    peerUriPerPeerId[sourcePeerId],
                    targetPeerId,
                    peerUriPerPeerId[targetPeerId]
                );
            }

            List<ReplicateShardsToPeerResponse.ReplicateShardToPeerResult> replicateShardResults = [];

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

                IEnumerable<uint> sourceShardIds;

                if (shardIdsToReplicate is { Length: > 0 })
                {
                    // Check that all provided shard ids exist on the source peer
                    foreach (var shardIdToReplicate in shardIdsToReplicate)
                    {
                        if (!collectionShardsPerPeers[sourcePeerId].Contains(shardIdToReplicate))
                        {
                            sw.Stop();

                            return ReplicateShardsToPeerResponse.Fail(
                                QdrantStatus.Fail(
                                    $"Collection '{collectionName}' does not have shard {shardIdToReplicate} on source peer {sourcePeerId}({peerUriPerPeerId[sourcePeerId]})"),
                                sw.Elapsed.TotalSeconds
                            );
                        }
                    }

                    sourceShardIds = shardIdsToReplicate;
                }
                else
                {
                    sourceShardIds = collectionShardsPerPeers[sourcePeerId];
                }

                foreach (var sourceShardId in sourceShardIds)
                {
                    // target peer either does not have any shards on it or does not have the source shard
                    var targetPeerDoesNotHaveSourceShard =
                        !collectionShardsPerPeers.ContainsKey(targetPeerId)
                        || !collectionShardsPerPeers[targetPeerId].Contains(sourceShardId);

                    if (targetPeerDoesNotHaveSourceShard)
                    {
                        if (logger?.IsEnabled(LogLevel.Information) == true)
                        {
                            logger?.LogInformation(
                                "Going to replicate collection '{CollectionName}' shard {ShardId} from peer {SourcePeer}({SourcePeerUri}) to peer {TargetPeer}({TargetPeerUri})",
                                collectionName,
                                sourceShardId,
                                sourcePeerId,
                                peerUriPerPeerId[sourcePeerId],
                                targetPeerId,
                                peerUriPerPeerId[targetPeerId]
                            );
                        }

                        if (!isDryRun)
                        {
                            var replicateShardResponse = await UpdateCollectionClusteringSetup(
                                collectionName,
                                isMoveShards
                                    ? UpdateCollectionClusteringSetupRequest.CreateMoveShardRequest(
                                        shardId: sourceShardId,
                                        fromPeerId: sourcePeerId,
                                        toPeerId: targetPeerId,
                                        shardTransferMethod: shardTransferMethod)
                                    : UpdateCollectionClusteringSetupRequest.CreateReplicateShardRequest(
                                        shardId: sourceShardId,
                                        fromPeerId: sourcePeerId,
                                        toPeerId: targetPeerId,
                                        shardTransferMethod: shardTransferMethod),
                                cancellationToken);

                            if (!replicateShardResponse.Status.IsSuccess
                                || replicateShardResponse.Result is false)
                            {
                                if (logger?.IsEnabled(LogLevel.Error) == true)
                                {
                                    logger?.LogError(
                                        "Error replicating collection '{CollectionName}' shard {ShardId} from peer {SourcePeer}({SourcePeerUri}) to peer {TargetPeer}({TargetPeerUri}): {ErrorMessage}",
                                        collectionName,
                                        sourceShardId,
                                        sourcePeerId,
                                        peerUriPerPeerId[sourcePeerId],
                                        targetPeerId,
                                        peerUriPerPeerId[targetPeerId],
                                        replicateShardResponse.Status.GetErrorMessage()
                                    );
                                }

                                // means issuing replicate operation failed - abandon move

                                sw.Stop();

                                return new ReplicateShardsToPeerResponse(replicateShardResponse)
                                {
                                    Result = [
                                        ReplicateShardsToPeerResponse.ReplicateShardToPeerResult.Fail(
                                            sourceShardId, sourcePeerId, targetPeerId, collectionName)
                                    ],
                                    Time = sw.Elapsed.TotalSeconds
                                };
                            }

                            replicateShardResults.Add(new(
                                IsSuccess: true,
                                ShardId: sourceShardId,
                                SourcePeerId: sourcePeerId,
                                TargetPeerId: targetPeerId,
                                collectionName)
                            );
                        }
                        else
                        {
                            logger?.LogInformation("Shard move simulation mode ON. No shards replicated");
                        }
                    }
                    else
                    {
                        if (logger?.IsEnabled(LogLevel.Information) == true)
                        {
                            // shard already exists on target peer
                            logger?.LogInformation(
                                "Collection '{CollectionName}' shard {ShardId} already exists on a target peer {TargetPeerId}({PeerUri}). Shard won't be replicated",
                                collectionName,
                                sourceShardId,
                                targetPeerId,
                                peerUriPerPeerId[targetPeerId]
                            );
                        }
                    }
                }
            }

            sw.Stop();

            return new ReplicateShardsToPeerResponse()
            {
                Result = replicateShardResults,
                Status = QdrantStatus.Success(),
                Time = sw.Elapsed.TotalSeconds
            };
        }
        catch (QdrantUnsuccessfulResponseStatusException qex)
        {
            sw.Stop();

            return ReplicateShardsToPeerResponse.Fail(QdrantStatus.Fail(qex.Message, qex), sw.Elapsed.TotalSeconds);
        }
    }

    /// <inheritdoc/>
    public async Task<ReplicateShardsToPeerResponse> ReplicateShardsToPeer(
        ulong targetPeerId,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        string clusterName = null,
        params string[] collectionNamesToReplicate)
    {
        var targetPeerInfo = await GetPeerInfo(
            targetPeerId,
            cancellationToken,
            clusterName: clusterName);

        return await ReplicateShardsToPeerInternal(
            targetPeerInfo,
            shardTransferMethod,
            cancellationToken,
            logger,
            isDryRun,
            clusterName,
            collectionNamesToReplicate);
    }

    /// <inheritdoc/>
    public async Task<ReplicateShardsToPeerResponse> ReplicateShardsToPeer(
        string targetPeerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        string clusterName = null,
        params string[] collectionNamesToReplicate)
    {
        var targetPeerInfo = await GetPeerInfo(
            targetPeerUriSelectorString,
            cancellationToken,
            clusterName: clusterName);

        return await ReplicateShardsToPeerInternal(
            targetPeerInfo,
            shardTransferMethod,
            cancellationToken,
            logger,
            isDryRun,
            clusterName,
            collectionNamesToReplicate);
    }

    private async Task<ReplicateShardsToPeerResponse> ReplicateShardsToPeerInternal(
        GetPeerResponse targetPeerInfoResponse,
        ShardTransferMethod shardTransferMethod,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        string clusterName = null,
        params string[] collectionNamesToReplicate)
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            var (targetPeerId, _, sourcePeerIds, peerUriPerPeerId) = targetPeerInfoResponse.EnsureSuccess();

            var collectionInfos = (await ListCollectionInfo(
                isCountExactPointsNumber: false,
                cancellationToken,
                clusterName: clusterName)).EnsureSuccess();

            var sourcePeers = new CircularEnumerable<ulong>(sourcePeerIds);

            string[] collectionNames;

            if (collectionNamesToReplicate is { Length: > 0 })
            {
                // check that all collection names are stated correctly
                foreach (var collectionNameFromParameter in collectionNamesToReplicate)
                {
                    if (!collectionInfos.ContainsKey(collectionNameFromParameter))
                    {
                        sw.Stop();

                        return ReplicateShardsToPeerResponse.Fail(
                            QdrantStatus.Fail(
                            $"Collection '{collectionNameFromParameter}' does not exist, check parameters"),
                            sw.Elapsed.TotalSeconds
                        );
                    }
                }

                collectionNames = collectionNamesToReplicate;
            }
            else
            {
                collectionNames = [.. collectionInfos.Keys];
            }

            if (logger?.IsEnabled(LogLevel.Information) == true)
            {
                logger?.LogInformation(
                    "Going to replicate shards for collections {CollectionNames} from peers {PeersToMoveShardsFrom} to peer {PeerToMoveShardsTo}({PeerUri})",
                    collectionInfos.Keys,
                    sourcePeerIds,
                    targetPeerId,
                    peerUriPerPeerId[targetPeerId]
                );
            }

            List<ReplicateShardsToPeerResponse.ReplicateShardToPeerResult> replicateShardResults = [];

            foreach (var collectionName in collectionNames)
            {
                var collectionShardingInfo =
                    (await GetCollectionClusteringInfo(collectionName, cancellationToken))
                    .EnsureSuccess();

                var collectionInfo = collectionInfos[collectionName];

                var collectionTargetReplicationFactor = collectionInfo.Config.Params.ReplicationFactor ?? 0U;

                if (collectionTargetReplicationFactor == 0U)
                {
                    if (logger?.IsEnabled(LogLevel.Error) == true)
                    {
                        logger?.LogError(
                            "Collection '{CollectionName}' replication factor is unknown. Skipping collection shards replication",
                            collectionName);
                    }
                    continue;
                }

                var (collectionShardsPerPeers, shardsWithReplicationFactors) =
                    GetShardIdsPerPeerIdsAndReplicationFactors(
                        collectionName,
                        collectionShardingInfo,
                        collectOnlyActiveShards: true,
                        logger);

                // collect source shards to replicate and peers to replicate shards from

                List<(uint sourceShardId, ulong sourcePeerId)> shardReplicationSources = [];

                foreach (var (sourceShardId, _) in shardsWithReplicationFactors)
                {
                    // target peer either does not have any shards on it or does not have the source shard
                    var targetPeerDoesNotHaveSourceShard =
                        !collectionShardsPerPeers.ContainsKey(targetPeerId)
                        || !collectionShardsPerPeers[targetPeerId].Contains(sourceShardId);

                    if (targetPeerDoesNotHaveSourceShard)
                    {
                        var candidateSourcePeer = sourcePeers.GetNext();

                        using var _ = sourcePeers.StartCircleDetection();

                        if (collectionShardsPerPeers.TryGetValue(candidateSourcePeer, out HashSet<uint> candidateCollectionShards))
                        {
                            while (!candidateCollectionShards.Contains(sourceShardId))
                            {
                                // here we should not get an infinite cycle since
                                // at least one peer will contain a source shard replica
                                // but if we do - circle detection in CircularEnumerable should throw
                                candidateSourcePeer = sourcePeers.GetNext();
                            }
                        }

                        shardReplicationSources.Add((sourceShardId, candidateSourcePeer));
                    }
                    else
                    {
                        if (logger?.IsEnabled(LogLevel.Information) == true)
                        {
                            // shard already exists on target peer
                            logger?.LogInformation(
                                "Collection '{CollectionName}' shard {ShardId} already exists on a target peer {TargetPeerId}({PeerUri}). Shard won't be replicated",
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
                    if (logger?.IsEnabled(LogLevel.Information) == true)
                    {
                        logger?.LogInformation(
                            "Going to replicate collection '{CollectionName}' shard {ShardId} from peer {SourcePeer}({SourcePeerUri}) to peer {TargetPeer}({TargetPeerUri})",
                            collectionName,
                            sourceShardId,
                            shardReplicaSourcePeerId,
                            peerUriPerPeerId[shardReplicaSourcePeerId],
                            targetPeerId,
                            peerUriPerPeerId[targetPeerId]
                        );
                    }

                    if (!isDryRun)
                    {
                        var replicateShardResponse = await UpdateCollectionClusteringSetup(
                            collectionName,
                            UpdateCollectionClusteringSetupRequest.CreateReplicateShardRequest(
                                shardId: sourceShardId,
                                fromPeerId: shardReplicaSourcePeerId,
                                toPeerId: targetPeerId,
                                shardTransferMethod: shardTransferMethod),
                            cancellationToken);

                        if (!replicateShardResponse.Status.IsSuccess
                            || replicateShardResponse.Result is false)
                        {
                            if (logger?.IsEnabled(LogLevel.Error) == true)
                            {
                                logger?.LogError(
                                    "Error replicating collection '{CollectionName}' shard {ShardId} from peer {SourcePeer}({SourcePeerUri}) to peer {TargetPeer}({TargetPeerUri}): {ErrorMessage}",
                                    collectionName,
                                    sourceShardId,
                                    shardReplicaSourcePeerId,
                                    peerUriPerPeerId[shardReplicaSourcePeerId],
                                    targetPeerId,
                                    peerUriPerPeerId[targetPeerId],
                                    replicateShardResponse.Status.GetErrorMessage()
                                );
                            }

                            // means issuing replicate operation failed - abandon move

                            sw.Stop();

                            return new ReplicateShardsToPeerResponse(replicateShardResponse)
                            {
                                Result = [
                                    ReplicateShardsToPeerResponse.ReplicateShardToPeerResult.Fail(
                                        sourceShardId,
                                        shardReplicaSourcePeerId,
                                        targetPeerId,
                                        collectionName)
                                ],
                                Time = sw.Elapsed.TotalSeconds
                            };
                        }

                        replicateShardResults.Add(new(
                                IsSuccess: true,
                                ShardId: sourceShardId,
                                SourcePeerId: shardReplicaSourcePeerId,
                                TargetPeerId: targetPeerId,
                                collectionName)
                        );
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
                Result = replicateShardResults,
                Status = QdrantStatus.Success(),
                Time = sw.Elapsed.TotalSeconds
            };
        }
        catch (QdrantUnsuccessfulResponseStatusException qex)
        {
            sw.Stop();

            return ReplicateShardsToPeerResponse.Fail(QdrantStatus.Fail(qex.Message, qex), sw.Elapsed.TotalSeconds);
        }
    }

    /// <inheritdoc/>
    public async Task<RestoreShardReplicationFactorResponse> RestoreShardReplicationFactor(
        string collectionName,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        TimeSpan? timeout = null,
        string clusterName = null)
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            var collectionInfo = (
                await GetCollectionInfo(collectionName, cancellationToken)
            ).EnsureSuccess();

            var collectionClusteringInfo = (
                await GetCollectionClusteringInfo(
                    collectionName,
                    cancellationToken,
                    clusterName: clusterName
                )
            ).EnsureSuccess();

            var shardReplicator = new ShardReplicator(
                this,
                logger,
                collectionName,
                collectionInfo,
                collectionClusteringInfo);

            shardReplicator.Calculate();

            sw.Stop();

            return new RestoreShardReplicationFactorResponse()
            {
                Result = shardReplicator,
                Status = QdrantStatus.Success(),
                Time = sw.Elapsed.TotalSeconds
            };

        }
        catch (QdrantUnsuccessfulResponseStatusException qex)
        {
            sw.Stop();

            return new RestoreShardReplicationFactorResponse()
            {
                Result = null,
                Status = QdrantStatus.Fail(qex.Message, qex),
                Time = sw.Elapsed.TotalSeconds
            };
        }
    }

    /// <inheritdoc/>
    public async Task<ReplicateShardsToPeerResponse> EqualizeShardReplication(
        string[] collectionNamesToEqualize,
        string sourcePeerUriSelectorString,
        string emptyTargetPeerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        string clusterName = null)
    {
        GetPeerResponse sourcePeerInfo = await GetPeerInfo(
            sourcePeerUriSelectorString,
            cancellationToken,
            clusterName: clusterName);

        GetPeerResponse targetPeerInfo = await GetPeerInfo(
            emptyTargetPeerUriSelectorString,
            cancellationToken,
            clusterName: clusterName);

        return await EqualizeShardReplicationInternal(
            sourcePeerInfo,
            targetPeerInfo,
            collectionNamesToEqualize,
            clusterName,
            shardTransferMethod,
            cancellationToken,
            logger,
            isDryRun);
    }

    /// <inheritdoc/>
    public async Task<ReplicateShardsToPeerResponse> EqualizeShardReplication(
        string[] collectionNamesToEqualize,
        ulong sourcePeerId,
        ulong emptyTargetPeerId,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        string clusterName = null)
    {
        GetPeerResponse sourcePeerInfo = await GetPeerInfo(
            sourcePeerId,
            cancellationToken,
            clusterName: clusterName);

        GetPeerResponse targetPeerInfo = await GetPeerInfo(
            emptyTargetPeerId,
            cancellationToken,
            clusterName: clusterName);

        return await EqualizeShardReplicationInternal(
            sourcePeerInfo,
            targetPeerInfo,
            collectionNamesToEqualize,
            clusterName,
            shardTransferMethod,
            cancellationToken,
            logger,
            isDryRun
        );
    }

    private async Task<ReplicateShardsToPeerResponse> EqualizeShardReplicationInternal(
        GetPeerResponse sourcePeerInfoResponse,
        GetPeerResponse emptyTargetPeerInfoResponse,
        string[] collectionNamesToEqualize,
        string clusterName,
        ShardTransferMethod shardTransferMethod,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false)
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            var (sourcePeerId, _, _, _) = sourcePeerInfoResponse.EnsureSuccess();
            var (targetPeerId, _, _, peerUriPerPeerId) = emptyTargetPeerInfoResponse.EnsureSuccess();

            var collectionInfos = (await ListCollectionInfo(
                isCountExactPointsNumber: false,
                cancellationToken,
                clusterName: clusterName)).EnsureSuccess();

            string[] collectionNames;

            if (collectionNamesToEqualize is { Length: > 0 })
            {
                // check that all collection names are stated correctly
                foreach (var collectionNameFromParameter in collectionNamesToEqualize)
                {
                    if (!collectionInfos.ContainsKey(collectionNameFromParameter))
                    {
                        sw.Stop();

                        return ReplicateShardsToPeerResponse.Fail(
                            QdrantStatus.Fail(
                                $"Collection '{collectionNameFromParameter}' does not exist, check parameters"),
                            sw.Elapsed.TotalSeconds
                        );
                    }
                }

                collectionNames = collectionNamesToEqualize;
            }
            else
            {
                collectionNames = [.. collectionInfos.Keys];
            }
            if (logger?.IsEnabled(LogLevel.Information) == true)
            {
                logger?.LogInformation(
                    "Going to equalize shards for collections {CollectionNames} between source peer {PeerToMoveShardsFrom}({PeerToMoveShardsFromUri}) and target peer {PeerToMoveShardsTo}({PeerToMoveShardsToUri})",
                    collectionInfos.Keys,
                    sourcePeerId,
                    peerUriPerPeerId[sourcePeerId],
                    targetPeerId,
                    peerUriPerPeerId[targetPeerId]
                );
            }

            List<ReplicateShardsToPeerResponse.ReplicateShardToPeerResult> replicateShardResults = [];

            foreach (var collectionName in collectionNames)
            {
                var collectionShardingInfo =
                    (await GetCollectionClusteringInfo(collectionName, cancellationToken))
                    .EnsureSuccess();

                // we don't care about replication factor here - we just want to equalize shards between two peers
                var (collectionShardsPerPeers, _) =
                    GetShardIdsPerPeerIdsAndReplicationFactors(
                        collectionName,
                        collectionShardingInfo,
                        collectOnlyActiveShards: true,
                        logger);

                var sourceShardIds = collectionShardsPerPeers[sourcePeerId];

                if (sourceShardIds.Count <= 1)
                {
                    return ReplicateShardsToPeerResponse.Fail(
                        QdrantStatus.Fail(
                            $"Collection '{collectionName}' has {sourceShardIds.Count} shards on source peer {sourcePeerId}({peerUriPerPeerId[sourcePeerId]}). The source peer should have more than 1 shards for equalization"),
                        sw.Elapsed.TotalSeconds
                    );
                }

                var targetShardIds = collectionShardsPerPeers.TryGetValue(
                    targetPeerId,
                    out HashSet<uint> shardsOnTargetPeer)
                    ? shardsOnTargetPeer
                    : [];

                if (targetShardIds.Count != 0)
                {
                    // target peer is not empty - log and skip

                    sw.Stop();

                    return ReplicateShardsToPeerResponse.Fail(
                        QdrantStatus.Fail(
                            $"Collection '{collectionName}' has {targetShardIds.Count} shards on target peer {targetPeerId}({collectionShardsPerPeers[targetPeerId]}). The target peer should be empty for equalization"),
                        sw.Elapsed.TotalSeconds);
                }

                var shardsToMoveCount = sourceShardIds.Count / 2;

                var shardIdsToMove = sourceShardIds.RandomSubset(shardsToMoveCount);

                foreach (var shardIdToMove in shardIdsToMove)
                {
                    if (logger?.IsEnabled(LogLevel.Information) == true)
                    {
                        logger?.LogInformation(
                            "Going to move collection '{CollectionName}' shard {ShardId} from peer {SourcePeer}({SourcePeerUri}) to peer {TargetPeer}({TargetPeerUri})",
                            collectionName,
                            shardIdToMove,
                            sourcePeerId,
                            peerUriPerPeerId[sourcePeerId],
                            targetPeerId,
                            peerUriPerPeerId[targetPeerId]
                        );
                    }

                    if (!isDryRun)
                    {
                        var moveShardResponse = await UpdateCollectionClusteringSetup(
                            collectionName,
                            UpdateCollectionClusteringSetupRequest.CreateMoveShardRequest(
                                shardId: shardIdToMove,
                                fromPeerId: sourcePeerId,
                                toPeerId: targetPeerId,
                                shardTransferMethod: shardTransferMethod),
                            cancellationToken);

                        if (!moveShardResponse.Status.IsSuccess
                            || moveShardResponse.Result is false)
                        {
                            if (logger?.IsEnabled(LogLevel.Error) == true)
                            {
                                logger?.LogError(
                                    "Error moving collection '{CollectionName}' shard {ShardId} from peer {SourcePeer}({SourcePeerUri}) to peer {TargetPeer}({TargetPeerUri}): {ErrorMessage}",
                                    collectionName,
                                    shardIdToMove,
                                    sourcePeerId,
                                    peerUriPerPeerId[sourcePeerId],
                                    targetPeerId,
                                    peerUriPerPeerId[targetPeerId],
                                    moveShardResponse.Status.GetErrorMessage()
                                );
                            }

                            // means issuing move shard operation failed - abandon move

                            sw.Stop();

                            return new ReplicateShardsToPeerResponse(moveShardResponse)
                            {
                                Result = [
                                    ReplicateShardsToPeerResponse.ReplicateShardToPeerResult.Fail(
                                        shardIdToMove,
                                        sourcePeerId,
                                        targetPeerId,
                                        collectionName)
                                ],
                                Time = sw.Elapsed.TotalSeconds
                            };
                        }

                        replicateShardResults.Add(new(
                            IsSuccess: true,
                            ShardId: shardIdToMove,
                            SourcePeerId: sourcePeerId,
                            TargetPeerId: targetPeerId,
                            collectionName)
                        );
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
                Result = replicateShardResults,
                Status = QdrantStatus.Success(),
                Time = sw.Elapsed.TotalSeconds
            };
        }
        catch (QdrantUnsuccessfulResponseStatusException qex)
        {
            sw.Stop();

            return ReplicateShardsToPeerResponse.Fail(
                QdrantStatus.Fail(qex.Message, qex),
                sw.Elapsed.TotalSeconds
            );
        }
    }

    /// <inheritdoc/>
    public async Task<DrainPeerResponse> DrainPeer(
        ulong peerId,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        string clusterName = null,
        params string[] collectionNamesToMove
    )
    {
        var peerToDrainInfo = await GetPeerInfo(
            peerId,
            cancellationToken,
            clusterName: clusterName);

        return await DrainPeerInternal(
            peerToDrainInfo,
            shardTransferMethod,
            clusterName,
            cancellationToken,
            logger,
            isDryRun,
            collectionNamesToMove);
    }

    /// <inheritdoc/>
    public async Task<DrainPeerResponse> DrainPeer(
        string peerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        string clusterName = null,
        params string[] collectionNamesToMove
    )
    {
        var peerToDrainInfo = await GetPeerInfo(
            peerUriSelectorString,
            cancellationToken,
            clusterName: clusterName);

        return await DrainPeerInternal(
            peerToDrainInfo,
            shardTransferMethod,
            clusterName,
            cancellationToken,
            logger,
            isDryRun,
            collectionNamesToMove);
    }

    private async Task<DrainPeerResponse> DrainPeerInternal(
        GetPeerResponse peerToDrainInfoResponse,
        ShardTransferMethod shardTransferMethod,
        string clusterName,
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
                peerToDrainInfoResponse.EnsureSuccess();

            var collectionNames = (await ListCollections(cancellationToken, clusterName))
                .EnsureSuccess()
                .Collections
                .Select(c => c.Name)
                .ToArray();

            if (collectionNamesToMove is { Length: > 0 })
            {
                // check that all collection names are stated correctly
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
                            Time = sw.Elapsed.TotalSeconds
                        };
                    }
                }

                collectionNames = collectionNamesToMove;
            }

            var targetPeers = new CircularEnumerable<ulong>(targetPeerIds);

            if (logger?.IsEnabled(LogLevel.Information) == true)
            {
                logger?.LogInformation(
                    "Going to move all active shards for collections '{CollectionNames}' from peer {PeerToEmpty}({PeerUri}) to peers {PeersToMoveShardsTo}",
                    collectionNames,
                    sourcePeerId,
                    peerUriPerPeerId[sourcePeerId],
                    targetPeerIds
                );
            }

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
                    if (logger?.IsEnabled(LogLevel.Information) == true)
                    {
                        logger?.LogInformation(
                            "Collection '{CollectionName}' has no shards on peer {SourcePeerId}({SourceUri})",
                            collectionName,
                            sourcePeerId,
                            peerUriPerPeerId[sourcePeerId]
                        );
                    }

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
                        if (logger?.IsEnabled(LogLevel.Information) == true)
                        {
                            logger?.LogInformation(
                                "Going to move collection '{CollectionName}' shard {ShardId} from peer {SourcePeer}({SourcePeerUri}) to peer {TargetPeer}({TargetPeerUri})",
                                collectionName,
                                shardToMoveToPeer,
                                sourcePeerId,
                                peerUriPerPeerId[sourcePeerId],
                                targetPeerId,
                                peerUriPerPeerId[targetPeerId]
                            );
                        }

                        if (!isDryRun)
                        {
                            var moveShardResponse = await UpdateCollectionClusteringSetup(
                                collectionName,
                                UpdateCollectionClusteringSetupRequest.CreateMoveShardRequest(
                                    shardId: shardToMoveToPeer,
                                    fromPeerId: sourcePeerId,
                                    toPeerId: targetPeerId,
                                    shardTransferMethod: shardTransferMethod),
                                cancellationToken);

                            if (!moveShardResponse.Status.IsSuccess
                                || moveShardResponse.Result is false)
                            {
                                if (logger?.IsEnabled(LogLevel.Error) == true)
                                {
                                    logger?.LogError(
                                        "Error moving collection '{CollectionName}' shard {ShardId} from peer {SourcePeer}({SourcePeerUri}) to peer {TargetPeer}({TargetPeerUri}): {ErrorMessage}",
                                        collectionName,
                                        shardToMoveToPeer,
                                        sourcePeerId,
                                        peerUriPerPeerId[sourcePeerId],
                                        targetPeerId,
                                        peerUriPerPeerId[targetPeerId],
                                        moveShardResponse.Status.GetErrorMessage()
                                    );
                                }

                                // means issuing move operation failed - abandon drain

                                sw.Stop();

                                return new DrainPeerResponse(moveShardResponse)
                                {
                                    Result = false,
                                    Time = sw.Elapsed.TotalSeconds
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
                Time = sw.Elapsed.TotalSeconds
            };
        }
        catch (QdrantUnsuccessfulResponseStatusException qex)
        {
            sw.Stop();

            return new DrainPeerResponse()
            {
                Result = false,
                Status = QdrantStatus.Fail(qex.Message, qex),
                Time = sw.Elapsed.TotalSeconds
            };
        }
    }

    /// <inheritdoc/>
    public async Task<ClearPeerResponse> ClearPeer(
        ulong peerId,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        string clusterName = null,
        params string[] collectionNamesToClear
    )
    {
        var peerToDrainInfo = await GetPeerInfo(
            peerId,
            cancellationToken,
            clusterName: clusterName);

        return await ClearPeerInternal(
            peerToDrainInfo,
            clusterName,
            cancellationToken,
            logger,
            isDryRun,
            collectionNamesToClear);
    }

    /// <inheritdoc/>
    public async Task<ClearPeerResponse> ClearPeer(
        string peerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        string clusterName = null,
        params string[] collectionNamesToClear
    )
    {
        var peerToDrainInfo = await GetPeerInfo(
            peerUriSelectorString,
            cancellationToken,
            clusterName: clusterName);

        return await ClearPeerInternal(
            peerToDrainInfo,
            clusterName,
            cancellationToken,
            logger,
            isDryRun,
            collectionNamesToClear);
    }

    private async Task<ClearPeerResponse> ClearPeerInternal(
        GetPeerResponse peerToClearInfoResponse,
        string clusterName,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        params string[] collectionNamesToMove
    )
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            var (sourcePeerId, _, _, peerUriPerPeerId) =
                peerToClearInfoResponse.EnsureSuccess();

            var collectionNames = (await ListCollections(cancellationToken, clusterName))
                .EnsureSuccess()
                .Collections
                .Select(c => c.Name)
                .ToArray();

            if (collectionNamesToMove is { Length: > 0 })
            {
                // check that all collection names are stated correctly
                // this method gets called very rarely, so we don't bother with HashSet

                foreach (var collectionNameFromParameter in collectionNamesToMove)
                {
                    if (!collectionNames.Contains(collectionNameFromParameter, StringComparer.InvariantCulture))
                    {
                        sw.Stop();

                        return new ClearPeerResponse()
                        {
                            Result = false,
                            Status = QdrantStatus.Fail(
                                $"Collection '{collectionNameFromParameter}' does not exist, check parameters"),
                            Time = sw.Elapsed.TotalSeconds
                        };
                    }
                }

                collectionNames = collectionNamesToMove;
            }

            if (logger?.IsEnabled(LogLevel.Information) == true)
            {
                logger?.LogInformation(
                    "Going to drop all shards for collections '{CollectionNames}' from peer {PeerToEmpty}({PeerUri})",
                    collectionNames,
                    sourcePeerId,
                    peerUriPerPeerId[sourcePeerId]
                );
            }

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

                if (!collectionShardsPerPeers.TryGetValue(sourcePeerId, out HashSet<uint> shardsIdsToDropFromPeer))
                {
                    if (logger?.IsEnabled(LogLevel.Information) == true)
                    {
                        logger?.LogInformation(
                            "Collection '{CollectionName}' has no shards on peer {SourcePeerId} ({SourceUri})",
                            collectionName,
                            sourcePeerId,
                            peerUriPerPeerId[sourcePeerId]
                        );
                    }

                    continue;
                }

                foreach (var shardIdToDrop in shardsIdsToDropFromPeer)
                {
                    if (!isDryRun)
                    {
                        var dropShardReplicaResponse = await UpdateCollectionClusteringSetup(
                            collectionName,
                            UpdateCollectionClusteringSetupRequest.CreateDropShardReplicaRequest(
                                shardId: shardIdToDrop,
                                peerId: sourcePeerId),
                            cancellationToken);

                        if (!dropShardReplicaResponse.Status.IsSuccess
                            || dropShardReplicaResponse.Result is false)
                        {
                            if (logger?.IsEnabled(LogLevel.Error) == true)
                            {
                                logger?.LogError(
                                    "Error dropping collection '{CollectionName}' shard {ShardId} from peer {SourcePeer}({SourcePeerUri}) : {ErrorMessage}",
                                    collectionName,
                                    shardIdToDrop,
                                    sourcePeerId,
                                    peerUriPerPeerId[sourcePeerId],
                                    dropShardReplicaResponse.Status.GetErrorMessage()
                                );
                            }

                            // means issuing remove operation failed - abandon clear

                            sw.Stop();

                            return new ClearPeerResponse(dropShardReplicaResponse)
                            {
                                Result = false,
                                Time = sw.Elapsed.TotalSeconds
                            };
                        }
                    }
                    else
                    {
                        logger?.LogInformation("Shard move simulation mode ON. No shards moved");
                    }
                }
            }

            sw.Stop();

            return new ClearPeerResponse()
            {
                Result = true,
                Status = QdrantStatus.Success(),
                Time = sw.Elapsed.TotalSeconds
            };
        }
        catch (QdrantUnsuccessfulResponseStatusException qex)
        {
            sw.Stop();

            return new ClearPeerResponse()
            {
                Result = false,
                Status = QdrantStatus.Fail(qex.Message, qex),
                Time = sw.Elapsed.TotalSeconds
            };
        }
    }

    /// <inheritdoc/>
    public async Task<DropCollectionReplicaFromPeerResponse> DropCollectionShardsFromPeer(
        string collectionName,
        ulong peerId,
        uint[] shardIds,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        string clusterName = null
    )
    {
        var peerToDropShardFromInfo = await GetPeerInfo(
            peerId,
            cancellationToken,
            clusterName: clusterName);

        return await DropCollectionShardsFromPeerInternal(
            collectionName,
            peerToDropShardFromInfo,
            shardIds,
            cancellationToken,
            logger,
            isDryRun,
            clusterName
        );
    }

    /// <inheritdoc/>
    public async Task<DropCollectionReplicaFromPeerResponse> DropCollectionShardsFromPeer(
        string collectionName,
        string peerUriSelectorString,
        uint[] shardIds,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        string clusterName = null
    )
    {
        var peerToDropShardFromInfo = await GetPeerInfo(
            peerUriSelectorString,
            cancellationToken,
            clusterName: clusterName);

        return await DropCollectionShardsFromPeerInternal(
            collectionName,
            peerToDropShardFromInfo,
            shardIds,
            cancellationToken,
            logger,
            isDryRun,
            clusterName
        );
    }

    private async Task<DropCollectionReplicaFromPeerResponse> DropCollectionShardsFromPeerInternal(
        string collectionName,
        GetPeerResponse peerToDropShardFromInfo,
        uint[] shardIds,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        string clusterName = null
    )
    {
        Stopwatch sw = Stopwatch.StartNew();

        List<uint> shardsWithSuccessfulOperationStarts = new(shardIds.Length);

        try
        {
            var (sourcePeerId, _, _, peerUriPerPeerId) =
                peerToDropShardFromInfo.EnsureSuccess();

            foreach (var shardIdToDrop in shardIds)
            {
                if (!isDryRun)
                {
                    var dropShardReplicaResponse = await UpdateCollectionClusteringSetup(
                        collectionName,
                        UpdateCollectionClusteringSetupRequest.CreateDropShardReplicaRequest(
                            shardId: shardIdToDrop,
                            peerId: sourcePeerId),
                        cancellationToken);

                    if (!dropShardReplicaResponse.Status.IsSuccess
                        || dropShardReplicaResponse.Result is false)
                    {
                        if (logger?.IsEnabled(LogLevel.Error) == true)
                        {
                            logger?.LogError(
                                "Error dropping collection '{CollectionName}' shard {ShardId} from peer {SourcePeer}({SourcePeerUri}) : {ErrorMessage}",
                                collectionName,
                                shardIdToDrop,
                                sourcePeerId,
                                peerUriPerPeerId[sourcePeerId],
                                dropShardReplicaResponse.Status.GetErrorMessage()
                            );
                        }

                        // means issuing drop operation failed - abandon clear

                        var ret = new DropCollectionReplicaFromPeerResponse(dropShardReplicaResponse)
                        {
                            Result = new(false, [.. shardsWithSuccessfulOperationStarts]),
                            Time = sw.Elapsed.TotalSeconds
                        };

                        return ret;
                    }
                }
                else
                {
                    logger?.LogInformation("Shard drop simulation mode ON. No shard replicas dropped");
                }

                shardsWithSuccessfulOperationStarts.Add(shardIdToDrop);
            }

            sw.Stop();

            return new DropCollectionReplicaFromPeerResponse()
            {
                Result = new(true, [.. shardsWithSuccessfulOperationStarts]),
                Status = QdrantStatus.Success(),
                Time = sw.Elapsed.TotalSeconds
            };
        }
        catch (QdrantUnsuccessfulResponseStatusException qex)
        {
            sw.Stop();

            return new DropCollectionReplicaFromPeerResponse()
            {
                Result = new(false, [.. shardsWithSuccessfulOperationStarts]),
                Status = QdrantStatus.Fail(qex.Message, qex),
                Time = sw.Elapsed.TotalSeconds
            };
        }
    }

    /// <inheritdoc/>
    public async Task<CheckIsPeerEmptyResponse> CheckIsPeerEmpty(
        ulong peerId,
        CancellationToken cancellationToken,
        string clusterName = null)
    {
        var peerToCheckInfo = await GetPeerInfo(
            peerId,
            cancellationToken,
            clusterName: clusterName);

        return await CheckIsPeerEmptyInternal(
            peerToCheckInfo,
            clusterName,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<CheckIsPeerEmptyResponse> CheckIsPeerEmpty(
        string peerUriSelectorString,
        CancellationToken cancellationToken,
        string clusterName = null)
    {
        var peerToCheckInfo = await GetPeerInfo(
            peerUriSelectorString,
            cancellationToken);

        return await CheckIsPeerEmptyInternal(
            peerToCheckInfo,
            clusterName,
            cancellationToken);
    }

    private async Task<CheckIsPeerEmptyResponse> CheckIsPeerEmptyInternal(
        GetPeerResponse peerToCheckInfoResponse,
        string clusterName,
        CancellationToken cancellationToken)
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            var (peerIdToCheck, _, _, _) = peerToCheckInfoResponse.EnsureSuccess();

            var collectionNames = (await ListCollections(cancellationToken, clusterName))
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
                    isPeerEmptyForCollection =
                        collectionShardingInfo.RemoteShards.All(si => si.PeerId != peerIdToCheck);
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

    /// <inheritdoc/>
    public Task<GetPeerResponse> GetPeerInfo(
        string peerUriSelectorString,
        ulong? peerId,
        CancellationToken cancellationToken,
        string clusterName = null)
    {
        if (string.IsNullOrWhiteSpace(peerUriSelectorString)
            && !peerId.HasValue)
        {
            throw new ArgumentException(
                $"Either {nameof(peerUriSelectorString)} or {nameof(peerId)} must be provided.");
        }

        if (!string.IsNullOrWhiteSpace(peerUriSelectorString))
        {
            return GetPeerInfo(peerUriSelectorString, cancellationToken, clusterName: clusterName);
        }

        return GetPeerInfo(peerId!.Value, cancellationToken, clusterName: clusterName);
    }

    /// <inheritdoc/>
    public async Task<GetPeerResponse> GetPeerInfo(
        string peerUriSelectorString,
        CancellationToken cancellationToken,
        string clusterName = null)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var clusterInfo = await GetClusterInfoInternal(clusterName, cancellationToken);

        // Here we need to check whether the cluster don't have duplicated peers by same URI.

        HashSet<string> seenPeerUrls = [];
        List<KeyValuePair<string, ulong>> peerIdsByNodeUrls = [];
        List<KeyValuePair<string, ulong>> duplicatePeers = [];

        foreach (var peer in clusterInfo.ParsedPeers)
        {
            if (seenPeerUrls.Add(peer.Value.Uri))
            {
                peerIdsByNodeUrls.Add(new KeyValuePair<string, ulong>(peer.Value.Uri, peer.Key));
            }
            else
            {
                // Means we have already seen this peer URL, so we have a duplicate.
                // This may indicate invalid cluster state.

                duplicatePeers.Add(new KeyValuePair<string, ulong>(peer.Value.Uri, peer.Key));
            }
        }

        if (duplicatePeers.Count > 0)
        {
            throw new QdrantInvalidClusterStateException(
                $"Cluster contains peers with duplicated URIs [{string.Join(", ", duplicatePeers.Select(p => $"{p.Key} - {p.Value}"))}]. "
                + $"All peers: [{string.Join(", ", clusterInfo.ParsedPeers.Select(p => $"{p.Value.Uri} - {p.Key}"))}]");
        }

        var candidatePeerIds = peerIdsByNodeUrls
            .Where(kv => kv.Key.Contains(peerUriSelectorString, StringComparison.InvariantCulture))
            .ToList();

        if (candidatePeerIds.Count == 0)
        {
            throw new QdrantNoPeersFoundForUriSubstringException(
                peerUriSelectorString,
                peerIdsByNodeUrls
            );
        }

        if (candidatePeerIds.Count > 1)
        {
            throw new QdrantMoreThanOnePeerFoundForUriSubstringException(
                peerUriSelectorString,
                candidatePeerIds);
        }

        var nodePeerId = candidatePeerIds[0].Value;

        var otherPeerIds = peerIdsByNodeUrls.Where(kv => kv.Value != nodePeerId)
            .Select(kv => kv.Value)
            .ToList();

        Dictionary<ulong, string> peerUriPerPeerId = [];

        foreach (var peer in peerIdsByNodeUrls)
        {
            var nodeUri = peer.Key;
            var peerId = peer.Value;

            peerUriPerPeerId.Add(peerId, nodeUri);
        }

        var ret = new GetPeerResponse.PeerInfo()
        {
            PeerId = nodePeerId,
            PeerUri = candidatePeerIds[0].Key,
            OtherPeerIds = otherPeerIds,
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

    /// <inheritdoc/>
    public async Task<GetPeerResponse> GetPeerInfo(
        ulong peerId,
        CancellationToken cancellationToken,
        string clusterName = null)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var clusterInfo = await GetClusterInfoInternal(clusterName, cancellationToken);

        if (!clusterInfo.ParsedPeers.TryGetValue(peerId, out GetClusterInfoResponse.PeerInfoUint peerInfo))
        {
            throw new QdrantNoPeersFoundException(
                peerId,
                clusterInfo.ParsedPeers.Keys
            );
        }

        var otherPeerIds = clusterInfo.ParsedPeers
            .Where(p => p.Key != peerId)
            .Select(p => p.Key)
            .ToList();

        Dictionary<ulong, string> peerUriPerPeerId = [];

        foreach (var peer in clusterInfo.ParsedPeers)
        {
            var uri = peer.Value.Uri;
            var id = peer.Key;

            peerUriPerPeerId.Add(id, uri);
        }

        var ret = new GetPeerResponse.PeerInfo()
        {
            PeerId = peerId,
            PeerUri = peerInfo.Uri,
            OtherPeerIds = otherPeerIds,
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

    /// <inheritdoc/>
    [Obsolete($"Use one of the {nameof(GetPeerInfo)} methods.")]
    [SuppressMessage(
        "ReSharper",
        "UnusedMember.Global",
        Justification = "Obsolete method kept for backward compatibility.")]
    public Task<GetPeerResponse>
        GetPeerInfoByUriSubstring(
            string clusterNodeUriSubstring,
            CancellationToken cancellationToken) => GetPeerInfo(clusterNodeUriSubstring, cancellationToken);

    private static (Dictionary<ulong, HashSet<uint>> ShardsPerPeers, Dictionary<uint, uint> ShardReplicationFactors)
        GetShardIdsPerPeerIdsAndReplicationFactors(
            string collectionName,
            GetCollectionClusteringInfoResponse.CollectionClusteringInfo collectionShardingInfo,
            bool collectOnlyActiveShards,
            ILogger logger)
    {
        Dictionary<uint, uint> shardReplicationFactors = [];

        Dictionary<ulong, HashSet<uint>> collectionShardsPerPeers = new()
        {
            {collectionShardingInfo.PeerId, new()}
        };

        // collect local shards
        foreach (var localShard in collectionShardingInfo.LocalShards)
        {
            if (collectOnlyActiveShards && localShard.State != ShardState.Active)
            {
                if (logger?.IsEnabled(LogLevel.Information) == true)
                {
                    logger?.LogInformation(
                        "Shard {ShardId} for collection '{CollectionName}' has non-active state '{ShardState}' on peer {PeerId}. Shard won't be processed",
                        localShard.ShardId,
                        collectionName,
                        localShard.State.ToString(),
                        collectionShardingInfo.PeerId
                    );
                }

                continue;
            }

            collectionShardsPerPeers[collectionShardingInfo.PeerId].Add(localShard.ShardId);

#if NETSTANDARD2_0
            if (!shardReplicationFactors.TryAdd(localShard.ShardId, (uint)1))
#else
            if (!shardReplicationFactors.TryAdd(localShard.ShardId, 1))
#endif
            {
                shardReplicationFactors[localShard.ShardId]++;
            }
        }

        // collect remote shards
        foreach (var remoteShard in collectionShardingInfo.RemoteShards)
        {
            if (collectOnlyActiveShards && remoteShard.State != ShardState.Active)
            {
                if (logger?.IsEnabled(LogLevel.Information) == true)
                {
                    logger?.LogInformation(
                        "Shard {ShardId} for collection '{CollectionName}' has non-active state '{ShardState}' on peer {PeerId}. Shard won't be processed",
                        remoteShard.ShardId,
                        collectionName,
                        remoteShard.State.ToString(),
                        remoteShard.PeerId
                    );
                }

                continue;
            }

            collectionShardsPerPeers.TryAdd(remoteShard.PeerId, []);

#if NETSTANDARD2_0
            if (!shardReplicationFactors.TryAdd(remoteShard.ShardId, (uint)1))
#else
            if (!shardReplicationFactors.TryAdd(remoteShard.ShardId, 1))
#endif
            {
                shardReplicationFactors[remoteShard.ShardId]++;
            }

            collectionShardsPerPeers[remoteShard.PeerId].Add(remoteShard.ShardId);
        }

        return (collectionShardsPerPeers, shardReplicationFactors);
    }

    private async Task<GetClusterInfoResponse.ClusterInfo> GetClusterInfoInternal(string collectionOrClusterName, CancellationToken cancellationToken)
    {
        var getClusterInfoResponse = await GetClusterInfo(cancellationToken, collectionOrClusterName);

        getClusterInfoResponse.EnsureSuccess();

        return getClusterInfoResponse.Result;
    }
}
