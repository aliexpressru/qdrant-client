using System.Runtime.CompilerServices;
using Aer.QdrantClient.Http.Collections;
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Http.Infrastructure.Replication;

/// <summary>
/// Represents a component responsible for managing and
/// executing shard replication within a distributed collection.
/// Calculates which shards require additional replicas and coordinates
/// their replication to achieve the target replication factor across peers.
/// </summary>
/// <remarks>
/// Use this class to ensure that all shards in a collection
/// are replicated according to the configured or inferred replication factor.
/// The class provides methods to determine replication needs
/// and to perform asynchronous replication operations.
/// </remarks>
public class ShardReplicator
{
    private readonly QdrantHttpClient _qdrantClient;
    private readonly ILogger _logger;
    private readonly string _collectionName;
    private int _targetReplicationFactor;

    private Queue<ScheduledShardReplication> _shardReplicationsToExecute;

    /// <summary>
    /// Returns <c>true</c> if not sufficiently replicated shards detected.
    /// Use <see cref="ExecuteReplications(CancellationToken, ShardTransferMethod, TimeSpan?)"/> to perform the replication sequence.
    /// </summary>
    public bool ShardsNeedReplication => _shardReplicationsToExecute is { Count: > 0 };

    /// <summary>
    /// Returns the planned shard replications. If no replication required returns an empty collection.
    /// </summary>
    public IReadOnlyCollection<ScheduledShardReplication> ReplicationPlan => _shardReplicationsToExecute ?? [];

    internal ShardReplicator(QdrantHttpClient qdrantClient, ILogger logger, string collectionName)
    {
        _qdrantClient = qdrantClient;
        _logger = logger;
        _collectionName = collectionName;
    }

    internal void Calculate(
        GetClusterInfoResponse.ClusterInfo clusterInfo,
        GetCollectionInfoResponse.CollectionInfo collectionInfo,
        GetCollectionClusteringInfoResponse.CollectionClusteringInfo collectionClusteringInfo
    )
    {
        _targetReplicationFactor = GetCollectionReplicationFactor(collectionInfo, collectionClusteringInfo);

        // Check that each shard is replicated no fewer than targetCollectionReplicationFactor of times
        // If it is replicated fewer times - replicate to the peers that have the least number of replicas

        List<(uint ShardId, int NumberOfReplicasToAdd)> shardsToReplicate = new(collectionClusteringInfo.PeersByShards.Count);
        List<(uint ShardId, int NumberOfReplicasToDrop)> shardsToDrop = new(collectionClusteringInfo.PeersByShards.Count);

        // Collect shards with unbalanced replicas

        foreach (var (shardId, peerIds) in collectionClusteringInfo.PeersByShards)
        {
            switch (peerIds.Count.CompareTo(_targetReplicationFactor))
            {
                case 0:
                    // shard is replicated expected number of times
                    // No action needed
                    break;

                case > 0:
                    // shard is replicated more times than expected
                    // Do nothing - for now at least
                    shardsToDrop.Add((shardId, peerIds.Count - _targetReplicationFactor));
                    break;

                case < 0:
                    // shard is replicated fewer times than expected
                    shardsToReplicate.Add((shardId, _targetReplicationFactor - peerIds.Count));
                    break;
            }
        }

        if (_logger?.IsEnabled(LogLevel.Information) == true)
        {
            if (shardsToReplicate is { Count: > 0 })
            {
                // Not enough replicas

                _logger?.LogInformation(
                    "Collection {CollectionName} replication factor : {ReplicationFactor}. {ShardsToReplicateUpCount} shards to additionally replicate",
                    _collectionName,
                    _targetReplicationFactor,
                    shardsToReplicate.Count
                );
            }

            if (shardsToDrop is { Count: > 0 })
            {
                // More replicas than needed

                _logger?.LogInformation(
                    "Collection {CollectionName} replication factor : {ReplicationFactor}. {ShardsToReplicateDownCount} shards to drop replicas for",
                    _collectionName,
                    _targetReplicationFactor,
                    shardsToDrop.Count
                );
            }
        }

        // Plan shard replications

        PlanReplications(shardsToReplicate, shardsToDrop, clusterInfo, collectionClusteringInfo);
    }

    private void PlanReplications(
        List<(uint ShardId, int NumberOfReplicasToAdd)> shardsToReplicate,
        List<(uint ShardId, int NumberOfReplicasToDrop)> shardsToDrop,
        GetClusterInfoResponse.ClusterInfo clusterInfo,
        GetCollectionClusteringInfoResponse.CollectionClusteringInfo collectionClusteringInfo
    )
    {
        // Here we should consider shards with more \ less replicas as well as placement of all the shards across the cluster

        _shardReplicationsToExecute = new(
            capacity: shardsToReplicate.Sum(s => s.NumberOfReplicasToAdd) + shardsToDrop.Sum(s => s.NumberOfReplicasToDrop)
        );

        var collectionClusteringState = new CollectionClusteringState(clusterInfo, collectionClusteringInfo);

        // 0. Drop extra replicas

        if (shardsToDrop is { Count: > 0 })
        {
            foreach (var (shardId, replicasToDrop) in shardsToDrop)
            {
                // Find all shard replicas

                int replicasLeftToDrop = replicasToDrop;

                while (replicasLeftToDrop > 0)
                {
                    var allShardPeers = collectionClusteringState.PeersByShards[shardId];

                    int peerReplicas = 0;
                    ulong selectedPeerId = 0;

                    foreach (var peerId in allShardPeers)
                    {
                        // Select the peer with most replicas of any shards on it
                        var peerReplicaCount = collectionClusteringState.ShardsByPeers[peerId].Count;
                        if (peerReplicaCount > peerReplicas)
                        {
                            peerReplicas = peerReplicaCount;
                            selectedPeerId = peerId;
                        }
                    }

                    // Here target peer uri and url are null since we are dropping the replica

                    _shardReplicationsToExecute.Enqueue(
                        new(
                            shardId,
                            SourcePeerId: selectedPeerId,
                            SourcePeerUri: collectionClusteringState.KnownPeers[selectedPeerId].Uri,
                            TargetPeerId: null,
                            TargetPeerUri: null,
                            ScheduledShardReplication.ReplicatorAction.DropReplica
                        )
                    );

                    collectionClusteringState.DropShardReplica(shardId, selectedPeerId);
                }
            }
        }

        // 1. Replicate shards that don't have enough replicas

        if (shardsToReplicate is { Count: > 0 })
        {
            foreach (var (shardId, replicasToAdd) in shardsToReplicate)
            {
                // select target peers which do not have specified shard replica
                var targetPeerIds = collectionClusteringState
                    .ShardsByPeers.Where(kv => !kv.Value.Contains(shardId))
                    // Order by the number of replicas already on the shard.
                    // We need to fill up the peers with least replicas first
                    .OrderBy(kv => kv.Value.Count)
                    .Select(kv => kv.Key)
                    .ToArray();

                HashSet<ulong> sourcePeerIds = collectionClusteringState.PeersByShards[shardId];

                CircularEnumerable<ulong> sourcePeers = new(sourcePeerIds);
                CircularEnumerable<ulong> targetPeers = new(targetPeerIds);

                int replicasLeftToAdd = replicasToAdd;

                while (replicasLeftToAdd > 0)
                {
                    var sourcePeerId = sourcePeers.GetNext();
                    var sourcePeerUri = collectionClusteringState.KnownPeers[sourcePeerId].Uri;

                    var targetPeerId = targetPeers.GetNext();
                    var targetPeerUri = collectionClusteringState.KnownPeers[targetPeerId].Uri;

                    _shardReplicationsToExecute.Enqueue(
                        new(
                            shardId,
                            sourcePeerId,
                            sourcePeerUri,
                            targetPeerId,
                            targetPeerUri,
                            ScheduledShardReplication.ReplicatorAction.AddReplica
                        )
                    );

                    collectionClusteringState.AddShardReplica(shardId, targetPeerId);

                    replicasLeftToAdd--;
                }
            }
        }

        // 2. Move shards around until collection is balanced

        // Calculate how many shard replicas should a peer normally hold

        var expectedNumberOfReplicasPerPeer =
            (double)_targetReplicationFactor * collectionClusteringState.ShardCount / collectionClusteringState.KnownPeers.Count;

        var maxNumberOfReplicasPerPeer = Math.Ceiling(expectedNumberOfReplicasPerPeer);
        var minNumberOfReplicasPerPeer = Math.Floor(expectedNumberOfReplicasPerPeer);

        var unbalancedPeers = collectionClusteringState
            .ShardsByPeers.Where(peerWithShards =>
                peerWithShards.Value.Count > maxNumberOfReplicasPerPeer || peerWithShards.Value.Count < minNumberOfReplicasPerPeer
            )
            .ToArray();

        if (unbalancedPeers.Length == 0)
        {
            // No unbalanced peers - nothing to move
            return;
        }

        // Find peers with maximum an minimum replicas
        var maxReplicasPeer = collectionClusteringState.ShardsByPeers.MaxBy(p => p.Value.Count);
        var minReplicasPeer = collectionClusteringState.ShardsByPeers.MinBy(p => p.Value.Count);

        // Find all peers with enough

        // Then we select two peers - one with more replicas than needed, and one with less replicas than needed
    }

    /// <summary>
    /// Asynchronously replicates the specified shards to target peers.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel
    /// the asynchronous replication operation.
    /// </param>
    /// <param name="shardTransferMethod">
    /// The method used to transfer the shard to the target peer.
    /// Defaults to <see cref="ShardTransferMethod.Snapshot"/> if not specified.
    /// </param>
    /// <param name="timeout">
    /// An optional timeout that specifies the maximum duration
    /// to wait for each replication operation.
    /// If not provided, the default timeout of 30 seconds is used.</param>
    /// <returns>
    /// An asynchronous stream of shard replication results.
    /// </returns>
    /// <remarks>
    /// It is recommended to check on each returned replication status
    /// before continuing with the next replication step.
    /// </remarks>
    public async IAsyncEnumerable<ReplicateShardsToPeerResponse> ExecuteReplications(
        [EnumeratorCancellation] CancellationToken cancellationToken,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        TimeSpan? timeout = null
    )
    {
        if (_shardReplicationsToExecute is null or { Count: 0 })
        {
            yield break;
        }

        while (_shardReplicationsToExecute.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (shardId, sourcePeerId, _, targetPeerId, _, replicatorAction) = _shardReplicationsToExecute.Dequeue();

            switch (replicatorAction)
            {
                case ScheduledShardReplication.ReplicatorAction.AddReplica:
                    var replicateShardStartResponse = await _qdrantClient.UpdateCollectionClusteringSetup(
                        _collectionName,
                        UpdateCollectionClusteringSetupRequest.CreateReplicateShardRequest(
                            shardId,
                            sourcePeerId,
                            targetPeerId.Value,
                            shardTransferMethod
                        ),
                        cancellationToken,
                        timeout
                    );

                    ReplicateShardsToPeerResponse replicateShardResponse;

                    if (replicateShardStartResponse.Status.IsSuccess)
                    {
                        replicateShardResponse = new ReplicateShardsToPeerResponse()
                        {
                            Result = new(
                                ReplicatedShards:
                                [
                                    new ReplicateShardsToPeerResponse.ReplicateShardToPeerResult(
                                        IsSuccess: true,
                                        ShardId: shardId,
                                        SourcePeerId: sourcePeerId,
                                        TargetPeerId: targetPeerId,
                                        _collectionName
                                    ),
                                ],
                                AlreadyReplicatedShards: []
                            ),
                            Status = QdrantStatus.Success(),
                            Time = replicateShardStartResponse.Time,
                        };
                    }
                    else
                    {
                        replicateShardResponse = new ReplicateShardsToPeerResponse(replicateShardStartResponse)
                        {
                            Result = new(
                                ReplicatedShards:
                                [
                                    new ReplicateShardsToPeerResponse.ReplicateShardToPeerResult(
                                        IsSuccess: false,
                                        ShardId: shardId,
                                        SourcePeerId: sourcePeerId,
                                        TargetPeerId: targetPeerId,
                                        _collectionName
                                    ),
                                ],
                                AlreadyReplicatedShards: []
                            ),
                        };
                    }

                    yield return replicateShardResponse;

                    break;
                case ScheduledShardReplication.ReplicatorAction.DropReplica:
                    throw new NotImplementedException();
                    break;
                case ScheduledShardReplication.ReplicatorAction.MoveReplica:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown replicator action {replicatorAction}");
            }
        }
    }

    private static int GetCollectionReplicationFactor(
        GetCollectionInfoResponse.CollectionInfo collectionInfo,
        GetCollectionClusteringInfoResponse.CollectionClusteringInfo collectionClusteringInfo
    )
    {
        if (collectionInfo.Config.Params.ReplicationFactor.HasValue)
        {
            return (int)collectionInfo.Config.Params.ReplicationFactor.Value;
        }

        // If no replication factor returned from collectionInfo - assume the largest number of replicas across all collection shards is target replication factor

        int selectedReplicationFactor = 0;

        foreach (var (shardId, peerIds) in collectionClusteringInfo.PeersByShards)
        {
            if (peerIds.Count > selectedReplicationFactor)
            {
                selectedReplicationFactor = peerIds.Count;
            }
        }

        return selectedReplicationFactor;
    }
}
