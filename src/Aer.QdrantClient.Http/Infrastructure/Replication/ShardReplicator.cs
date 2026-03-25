using System.Collections.Concurrent;
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
    private readonly string _clusterName;
    private int _targetReplicationFactor;

    // The shards that replicator is forbidden to perform any operations on
    private readonly HashSet<uint> _skippedShards = [];

    // Probably concurrent queue is an overkill here
    private ConcurrentQueue<ScheduledShardReplication> _replicationPlan;

    // Internal for testing purposes
    internal CollectionClusteringState _targetCollectionClusteringState;

    /// <summary>
    /// Returns <c>true</c> if not sufficiently replicated shards detected.
    /// Use <see cref="ExecuteReplications(CancellationToken, ShardTransferMethod, TimeSpan?)"/> to perform the replication sequence.
    /// </summary>
    public bool ShardsNeedReplication => _replicationPlan is { Count: > 0 };

    /// <summary>
    /// Returns the planned shard replications. If no replication required returns an empty collection.
    /// Since the ordering of this collection is not guaranteed, sort the resulting collection by
    /// <see cref="ScheduledShardReplication.StepNumber"/> to obtain the actual order of operations.
    /// </summary>
    public IReadOnlyCollection<ScheduledShardReplication> ReplicationPlan => _replicationPlan ?? [];

    internal ShardReplicator(QdrantHttpClient qdrantClient, ILogger logger, string collectionName, string clusterName)
    {
        _qdrantClient = qdrantClient;
        _logger = logger;
        _collectionName = collectionName;
        _clusterName = clusterName;
    }

    // We call calculate with initial collection and cluster state. We assume that it won't change by any means apart from shard replicator itself.
    // On each replication step we check that this invariant is held true.
    internal RestoreShardReplicationFactorResponse Calculate(
        GetClusterInfoResponse.ClusterInfo clusterInfo,
        GetCollectionInfoResponse.CollectionInfo collectionInfo,
        GetCollectionClusteringInfoResponse.CollectionClusteringInfo collectionClusteringInfo
    )
    {
        if (collectionClusteringInfo.ShardTransfers.Length > 0)
        {
            // We don't allow starting any replication-related operations while there are any ongoing shard transfers

            return new RestoreShardReplicationFactorResponse()
            {
                Result = null,
                Status = QdrantStatus.Fail(
                    $"Can't restore shard replication factor. Found {collectionClusteringInfo.ShardTransfers} ongoing shard transfers. To ensure that collection data is not corrupted, wait for ongoing shard transfers to finish and start restore shard replication factor process again."
                ),
            };
        }

        _targetReplicationFactor = GetCollectionReplicationFactor(collectionInfo, collectionClusteringInfo);

        // Check that each shard is replicated no fewer than targetCollectionReplicationFactor of times
        // If it is replicated fewer times - replicate to the peers that have the least number of replicas

        // Replace with lines below when collection expression parameters support lands in C#15
        List<(uint ShardId, int NumberOfReplicasToAdd)> shardsToReplicate = [];
        List<(uint ShardId, int NumberOfReplicasToDrop)> shardsToDrop = [];

        //List<(uint ShardId, int NumberOfReplicasToAdd)> shardsToReplicate = [with(collectionClusteringInfo.PeersByShards.Count)];
        //List<(uint ShardId, int NumberOfReplicasToDrop)> shardsToDrop = [with(collectionClusteringInfo.PeersByShards.Count)];

        // Check actual number of replicas for each shard against required replication factor

        List<(uint ShardId, ulong PeerId)> inactiveShardReplicasToDrop = [];

        foreach (var (shardId, peerIds) in collectionClusteringInfo.PeersByShards)
        {
            int inactiveReplicaCount = 0;

            foreach (var peerId in peerIds)
            {
                var peerState = collectionClusteringInfo.ShardStates[shardId][peerId];

                if (peerState is not (ShardState.Active or ShardState.ActiveRead))
                {
                    // We should add inactive replicas to the drop list only
                    // If any other active replica exists for them
                    inactiveShardReplicasToDrop.Add((shardId, peerId));
                    inactiveReplicaCount++;
                }
            }

            if (inactiveReplicaCount == peerIds.Count)
            {
                // Means all replicas for this shard are in inactive state. We don't have any means to copy active data.
                // Don't perform any operations on that shard
                _skippedShards.Add(shardId);

                continue;
            }

            // This is a total number of peers having specified shard replica with inactive replica count subtracted
            // (if any active replicas for that shard present at all)
            var activeReplicaCount = peerIds.Count - inactiveReplicaCount;

            switch (activeReplicaCount.CompareTo(_targetReplicationFactor))
            {
                case 0:
                    // shard is replicated expected number of times
                    // No action needed
                    break;

                case > 0:
                    // shard is replicated more times than expected - drop extra replicas
                    shardsToDrop.Add((shardId, activeReplicaCount - _targetReplicationFactor));
                    break;

                case < 0:
                    // shard is replicated fewer times than expected - add more replicas
                    shardsToReplicate.Add((shardId, _targetReplicationFactor - activeReplicaCount));
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

        var targetCollectionClusteringState = PlanReplications(
            inactiveShardReplicasToDrop,
            shardsToReplicate,
            shardsToDrop,
            clusterInfo,
            collectionClusteringInfo
        );

        _targetCollectionClusteringState = targetCollectionClusteringState;

        return new RestoreShardReplicationFactorResponse() { Result = this, Status = QdrantStatus.Success() };
    }

    private CollectionClusteringState PlanReplications(
        List<(uint ShardId, ulong PeerId)> inactiveShardReplicas,
        List<(uint ShardId, int NumberOfReplicasToAdd)> shardsToReplicate,
        List<(uint ShardId, int NumberOfReplicasToDrop)> shardsToDrop,
        GetClusterInfoResponse.ClusterInfo clusterInfo,
        GetCollectionClusteringInfoResponse.CollectionClusteringInfo collectionClusteringInfo
    )
    {
        // Here we should consider shards with more \ less replicas as well as placement of all the shards across the cluster.
        // On every step of the algorithm we perform sanity checks and throw InvalidOperationException if something does not look right

        _replicationPlan = new();

        // This is a snapshot of the collection clustering state before we start replication process.
        // We modify this snapshot on each step of the planning process to always keep
        // the current state of the collection clustering. We return it only for testing purposes
        var collectionClusteringState = new CollectionClusteringState(
            clusterInfo,
            collectionClusteringInfo,
            _targetReplicationFactor
        );

        // ------------------ Restore replication factor

        // 0. Drop inactive replicas. We consider replica inactive if it is not Active.

        PlanInactiveReplicaDrops(inactiveShardReplicas, collectionClusteringState);

        // 1. Drop extra replicas

        PlanExtraReplicaDrops(shardsToDrop, collectionClusteringState);

        // 2. Replicate shards that don't have enough replicas

        PlanAddingReplicas(shardsToReplicate, collectionClusteringState);

        // Here we need to check whether all the shards in current version of collectionClusteringState have expected number of replicas
        // If not - throw algorithm state exception - means we have forgotten to replicate \ drop something on previous steps
        CheckReplicationFactorRestored(collectionClusteringState);

        // ------------------ Restore shard balance

        // Calculate maximum number of shard moves we need to restore shard balance.
        // This number will be a limiting factor for loops in both overpopulation and underpopulation fix planning stages
        // so that we won't end up moving shards to and fro forever.

        var maxNumberOfShardMoves = CalculateMaximumNumberOfShardMovesForPopulationDepopulation(collectionClusteringState);

        // 3. Move shards until collection is balanced. I.e. there are no overpopulated or underpopulated peers.

        // 3.1 - check overpopulated peers and depopulate them

        PlanOverpopulationFix(collectionClusteringState, maxNumberOfShardMoves);

        // 3.2 - check underpopulated peers and populate them

        PlanUnderpopulationFix(collectionClusteringState, maxNumberOfShardMoves);

        return collectionClusteringState;
    }

    private void PlanInactiveReplicaDrops(
        List<(uint ShardId, ulong PeerId)> inactiveShardReplicasToDrop,
        CollectionClusteringState collectionClusteringState
    )
    {
        if (inactiveShardReplicasToDrop is { Count: > 0 })
        {
            foreach (var (shardIdToDrop, peerToDropShardFrom) in inactiveShardReplicasToDrop)
            {
                if (_skippedShards.Contains(shardIdToDrop))
                {
                    continue;
                }

                var expectedStateBeforeReplication = collectionClusteringState.Clone();

                if (!collectionClusteringState.DropShardReplica(shardIdToDrop, peerToDropShardFrom))
                {
                    throw new ShardReplicatorAlgorithmException(
                        $"Shard {shardIdToDrop} inactive replica drop from peer {peerToDropShardFrom} can't be performed"
                    );
                }

                // Here target peer uri and url are null since we are dropping the replica
                _replicationPlan.Enqueue(
                    new(
                        shardIdToDrop,
                        SourcePeerId: peerToDropShardFrom,
                        SourcePeerUri: collectionClusteringState.KnownPeers[peerToDropShardFrom].Uri,
                        TargetPeerId: null,
                        TargetPeerUri: null,
                        ScheduledShardReplication.ReplicatorAction.DropReplica,
                        collectionClusteringState.Version
                    )
                    {
                        ExpectedInitialState = expectedStateBeforeReplication,
                    }
                );
            }
        }
    }

    private void PlanExtraReplicaDrops(
        List<(uint ShardId, int NumberOfReplicasToDrop)> shardsToDrop,
        CollectionClusteringState collectionClusteringState
    )
    {
        if (shardsToDrop is { Count: > 0 })
        {
            foreach (var (shardIdToDrop, replicasToDrop) in shardsToDrop)
            {
                if (_skippedShards.Contains(shardIdToDrop))
                {
                    continue;
                }

                int replicasLeftToDrop = replicasToDrop;

                while (replicasLeftToDrop > 0)
                {
                    var allShardPeers = collectionClusteringState.PeersByShards[shardIdToDrop];

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

                    if (selectedPeerId == 0)
                    {
                        // Means no peer was selected since peer ids are all >0
                        throw new ShardReplicatorAlgorithmException(
                            $"A peer for the shard {shardIdToDrop} to drop was not found"
                        );
                    }

                    var expectedStateBeforeReplication = collectionClusteringState.Clone();

                    if (!collectionClusteringState.DropShardReplica(shardIdToDrop, selectedPeerId))
                    {
                        throw new ShardReplicatorAlgorithmException(
                            $"Shard {shardIdToDrop} drop from peer {selectedPeerId} can't be performed"
                        );
                    }

                    // Here target peer uri and url are null since we are dropping the replica
                    _replicationPlan.Enqueue(
                        new(
                            shardIdToDrop,
                            SourcePeerId: selectedPeerId,
                            SourcePeerUri: collectionClusteringState.KnownPeers[selectedPeerId].Uri,
                            TargetPeerId: null,
                            TargetPeerUri: null,
                            ScheduledShardReplication.ReplicatorAction.DropReplica,
                            collectionClusteringState.Version
                        )
                        {
                            ExpectedInitialState = expectedStateBeforeReplication,
                        }
                    );

                    replicasLeftToDrop--;
                }
            }
        }
    }

    private void PlanAddingReplicas(
        List<(uint ShardId, int NumberOfReplicasToAdd)> shardsToReplicate,
        CollectionClusteringState collectionClusteringState
    )
    {
        if (shardsToReplicate is { Count: > 0 })
        {
            foreach (var (shardIdToReplicate, replicasToAdd) in shardsToReplicate)
            {
                if (_skippedShards.Contains(shardIdToReplicate))
                {
                    continue;
                }

                // Select target peers which do not have specified shard replica
                var targetPeerIds = collectionClusteringState
                    .ShardsByPeers.Where(kv => !kv.Value.Contains(shardIdToReplicate))
                    // Order by the number of replicas already on the shard.
                    // We need to fill up the peers with least replicas first
                    .OrderBy(kv => kv.Value.Count)
                    .Select(kv => kv.Key)
                    .ToArray();

                HashSet<ulong> sourcePeerIds = collectionClusteringState.PeersByShards[shardIdToReplicate];

                // Round-robin both source and target peers
                CircularEnumerable<ulong> sourcePeers = new(sourcePeerIds);
                CircularEnumerable<ulong> targetPeers = new(targetPeerIds);

                int replicasLeftToAdd = replicasToAdd;

                while (replicasLeftToAdd > 0)
                {
                    var sourcePeerId = sourcePeers.GetNext();
                    var sourcePeerUri = collectionClusteringState.KnownPeers[sourcePeerId].Uri;

                    var targetPeerId = targetPeers.GetNext();
                    var targetPeerUri = collectionClusteringState.KnownPeers[targetPeerId].Uri;

                    var expectedStateBeforeReplication = collectionClusteringState.Clone();

                    if (!collectionClusteringState.AddShardReplica(shardIdToReplicate, targetPeerId))
                    {
                        throw new ShardReplicatorAlgorithmException(
                            $"Shard {shardIdToReplicate} replication from peer {sourcePeerId} to {targetPeerId} can't be performed"
                        );
                    }

                    _replicationPlan.Enqueue(
                        new(
                            shardIdToReplicate,
                            sourcePeerId,
                            sourcePeerUri,
                            targetPeerId,
                            targetPeerUri,
                            ScheduledShardReplication.ReplicatorAction.AddReplica,
                            collectionClusteringState.Version
                        )
                        {
                            ExpectedInitialState = expectedStateBeforeReplication,
                        }
                    );

                    replicasLeftToAdd--;
                }
            }
        }
    }

    private void CheckReplicationFactorRestored(CollectionClusteringState collectionClusteringState)
    {
        foreach (var (shardIdToCheck, peerIds) in collectionClusteringState.PeersByShards)
        {
            if (peerIds.Count != _targetReplicationFactor)
            {
                throw new ShardReplicatorAlgorithmException(
                    $"Shard {shardIdToCheck} replication factor was not restored after restoration planning. Expected {_targetReplicationFactor}, found {peerIds.Count}. It might indicate a bug in shard replication planning steps prior to this point"
                );
            }
        }
    }

    private static int CalculateMaximumNumberOfShardMovesForPopulationDepopulation(CollectionClusteringState collectionClusteringState)
    {
        int shardsToMoveCount = 0;

        // Check overpopulated peers and collect number of shards to move

        HashSet<ulong> alreadyCheckedPeers = [];

        foreach (var (peerId, peerShards) in collectionClusteringState.ShardsByPeers)
        {
            if (peerShards.Count > collectionClusteringState.MaxNumberOfReplicasPerPeer)
            {
                alreadyCheckedPeers.Add(peerId);
                var extraReplicas = peerShards.Count - collectionClusteringState.MaxNumberOfReplicasPerPeer;

                shardsToMoveCount += extraReplicas;
            }
        }

        int maxReplicaMoves = shardsToMoveCount;

        // Check underpopulated peers and populate them with available replicas to move until MaxNumberOfReplicasPerPeer

        foreach (var (peerId, peerShards) in collectionClusteringState.ShardsByPeers)
        {
            if (alreadyCheckedPeers.Contains(peerId))
            {
                // Skip already checked overpopulated peers
                continue;
            }

            if (peerShards.Count < collectionClusteringState.MaxNumberOfReplicasPerPeer)
            {
                var requiredReplicas = collectionClusteringState.MaxNumberOfReplicasPerPeer - peerShards.Count;

                shardsToMoveCount -= requiredReplicas;
            }
        }

        // We might end up requiring more nodes than we already had initially shaven off the overpopulated nodes
        maxReplicaMoves += Math.Abs(shardsToMoveCount);

        // This number is theoretical upper bound count of moves required for *both* overpopulation *and* underpopulation fix.
        // We use this upper bound for both operations thus doubling allowed number of moves.
        // Since it's a sanity check calculation this doubling is allowable.

        return maxReplicaMoves;
    }

    private void PlanOverpopulationFix(CollectionClusteringState collectionClusteringState, int maxShardMovesNumber)
    {
        var movesLeft = maxShardMovesNumber;

        var overpopulatedPeer = collectionClusteringState.GetMostOverpopulatedPeer();

        while (overpopulatedPeer.HasValue)
        {
            if (movesLeft == 0)
            {
                throw new ShardReplicatorAlgorithmException(
                    $"The overpopulated peer depopulation failed : calculated maximal number of shard moves {maxShardMovesNumber} exceeded. This might indicate an infinite loop or an unexpected edge case"
                );
            }

            var (minReplicasPeerId, minReplicasPeerShardIds) = collectionClusteringState.GetMinReplicasPeer();

            bool foundShardToMove = false;

            foreach (var shardIdToMove in overpopulatedPeer.Value.ShardIds)
            {
                if (!minReplicasPeerShardIds.Contains(shardIdToMove))
                {
                    var sourcePeerId = overpopulatedPeer.Value.PeerId;
                    var targetPeerId = minReplicasPeerId;

                    var expectedStateBeforeReplication = collectionClusteringState.Clone();

                    if (!collectionClusteringState.MoveShardReplica(shardIdToMove, sourcePeerId, targetPeerId))
                    {
                        throw new ShardReplicatorAlgorithmException(
                            $"Shard {shardIdToMove} replica move from peer {sourcePeerId} to {targetPeerId} can't be performed"
                        );
                    }

                    _replicationPlan.Enqueue(
                        new(
                            shardIdToMove,
                            sourcePeerId,
                            collectionClusteringState.KnownPeers[sourcePeerId].Uri,
                            targetPeerId,
                            collectionClusteringState.KnownPeers[targetPeerId].Uri,
                            ScheduledShardReplication.ReplicatorAction.MoveReplica,
                            collectionClusteringState.Version
                        )
                        {
                            ExpectedInitialState = expectedStateBeforeReplication,
                        }
                    );

                    foundShardToMove = true;

                    movesLeft--;

                    // Since this is not a performance-critical section, we move one shard at a time to not overcomplicate things
                    break;
                }
            }

            if (!foundShardToMove)
            {
                throw new ShardReplicatorAlgorithmException(
                    "The overpopulated peer depopulation failed : no shard found to move from the overpopulated peer"
                );
            }

            overpopulatedPeer = collectionClusteringState.GetMostOverpopulatedPeer();
        }
    }

    private void PlanUnderpopulationFix(CollectionClusteringState collectionClusteringState, int maxShardMovesNumber)
    {
        var movesLeft = maxShardMovesNumber;

        var underpopulatedPeer = collectionClusteringState.GetMostUnderpopulatedPeer();

        while (underpopulatedPeer.HasValue)
        {
            if (movesLeft == 0)
            {
                throw new ShardReplicatorAlgorithmException(
                    $"The underpopulated peer population failed : calculated maximal number of shard moves {maxShardMovesNumber} exceeded. This might indicate an infinite loop or an unexpected edge case"
                );
            }

            var underpopulatedPeerShards = underpopulatedPeer.Value.ShardIds;

            var (maxReplicasPeerId, candidateShardIdsToMove) = collectionClusteringState.GetMaxReplicasPeer();

            bool foundShardToMove = false;

            foreach (var shardIdToMove in candidateShardIdsToMove)
            {
                if (!underpopulatedPeerShards.Contains(shardIdToMove))
                {
                    var sourcePeerId = maxReplicasPeerId;
                    var targetPeerId = underpopulatedPeer.Value.PeerId;

                    var expectedStateBeforeReplication = collectionClusteringState.Clone();

                    if (!collectionClusteringState.MoveShardReplica(shardIdToMove, sourcePeerId, targetPeerId))
                    {
                        throw new ShardReplicatorAlgorithmException(
                            $"Shard {shardIdToMove} replica move from peer {sourcePeerId} to {targetPeerId} can't be performed"
                        );
                    }

                    _replicationPlan.Enqueue(
                        new(
                            shardIdToMove,
                            sourcePeerId,
                            collectionClusteringState.KnownPeers[sourcePeerId].Uri,
                            targetPeerId,
                            collectionClusteringState.KnownPeers[targetPeerId].Uri,
                            ScheduledShardReplication.ReplicatorAction.MoveReplica,
                            collectionClusteringState.Version
                        )
                        {
                            ExpectedInitialState = expectedStateBeforeReplication,
                        }
                    );

                    foundShardToMove = true;

                    movesLeft--;

                    // Since this is not a performance-critical section, we move one shard at a time to not overcomplicate things
                    break;
                }
            }

            if (!foundShardToMove)
            {
                throw new ShardReplicatorAlgorithmException(
                    "The underpopulated peer population failed : no shard found to move to the underpopulated peer"
                );
            }

            underpopulatedPeer = collectionClusteringState.GetMostUnderpopulatedPeer();
        }
    }

    /// <summary>
    /// Asynchronously executed the next replication from a <see cref="ReplicationPlan"/> and returns its result.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous replication operation.
    /// </param>
    /// <param name="shardTransferMethod">
    /// The method used to transfer the shard to the target peer.
    /// Defaults to <see cref="ShardTransferMethod.Snapshot"/> if not specified.
    /// </param>
    /// <param name="timeout">
    /// An optional timeout that specifies the maximum duration
    /// to wait for a replication operation.
    /// If not provided, the default timeout of 30 seconds is used.</param>
    /// <remarks>
    /// It is recommended to check on the returned replication status
    /// before continuing with the next replication step.
    /// </remarks>
    public Task<ReplicateShardsToPeerResponse> ExecuteNextReplication(
        CancellationToken cancellationToken,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        TimeSpan? timeout = null
    )
    {
        if (_replicationPlan is null or { IsEmpty: true })
        {
            return Task.FromResult(ReplicateShardsToPeerResponse.Fail(QdrantStatus.Fail("No replications to execute"), time: 0));
        }

        _replicationPlan.TryDequeue(out var nextReplicationStep);

        return ExecuteNextReplicationInternal(nextReplicationStep, shardTransferMethod, timeout, cancellationToken);
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
        if (_replicationPlan is null or { IsEmpty: true })
        {
            yield break;
        }

        while (!_replicationPlan.IsEmpty)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _replicationPlan.TryDequeue(out var nextReplicationStep);

            var shardReplicationResult = await ExecuteNextReplicationInternal(
                nextReplicationStep,
                shardTransferMethod,
                timeout,
                cancellationToken
            );

            yield return shardReplicationResult;
        }
    }

    private async Task<ReplicateShardsToPeerResponse> ExecuteNextReplicationInternal(
        ScheduledShardReplication nextReplicationStep,
        ShardTransferMethod shardTransferMethod,
        TimeSpan? timeout,
        CancellationToken cancellationToken
    )
    {
        var (shardId, sourcePeerId, _, targetPeerId, _, replicatorAction, _) = nextReplicationStep;

        var (canCommenceReplication, errorResponse) = await CheckCanCommenceReplication(nextReplicationStep, cancellationToken);

        if (!canCommenceReplication)
        {
            return errorResponse;
        }

        switch (replicatorAction)
        {
            case ScheduledShardReplication.ReplicatorAction.AddReplica:
            {
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

                return replicateShardResponse;
            }

            case ScheduledShardReplication.ReplicatorAction.DropReplica:
            {
                var dropShardReplicaStartResponse = await _qdrantClient.UpdateCollectionClusteringSetup(
                    _collectionName,
                    UpdateCollectionClusteringSetupRequest.CreateDropShardReplicaRequest(shardId, sourcePeerId),
                    cancellationToken,
                    timeout
                );

                ReplicateShardsToPeerResponse replicateShardResponse;

                if (dropShardReplicaStartResponse.Status.IsSuccess)
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
                                    TargetPeerId: null,
                                    _collectionName
                                ),
                            ],
                            AlreadyReplicatedShards: []
                        ),
                        Status = QdrantStatus.Success(),
                        Time = dropShardReplicaStartResponse.Time,
                    };
                }
                else
                {
                    replicateShardResponse = new ReplicateShardsToPeerResponse(dropShardReplicaStartResponse)
                    {
                        Result = new(
                            ReplicatedShards:
                            [
                                new ReplicateShardsToPeerResponse.ReplicateShardToPeerResult(
                                    IsSuccess: false,
                                    ShardId: shardId,
                                    SourcePeerId: sourcePeerId,
                                    TargetPeerId: null,
                                    _collectionName
                                ),
                            ],
                            AlreadyReplicatedShards: []
                        ),
                    };
                }

                return replicateShardResponse;
            }

            case ScheduledShardReplication.ReplicatorAction.MoveReplica:
            {
                var moveShardStartResponse = await _qdrantClient.UpdateCollectionClusteringSetup(
                    _collectionName,
                    UpdateCollectionClusteringSetupRequest.CreateMoveShardRequest(
                        shardId,
                        sourcePeerId,
                        targetPeerId.Value,
                        shardTransferMethod
                    ),
                    cancellationToken,
                    timeout
                );

                ReplicateShardsToPeerResponse replicateShardResponse;

                if (moveShardStartResponse.Status.IsSuccess)
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
                        Time = moveShardStartResponse.Time,
                    };
                }
                else
                {
                    replicateShardResponse = new ReplicateShardsToPeerResponse(moveShardStartResponse)
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

                return replicateShardResponse;
            }

            default:
                throw new InvalidOperationException($"Unknown replicator action {replicatorAction}");
        }
    }

    private async Task<(bool CanCommenceReplication, ReplicateShardsToPeerResponse ErrorResponse)> CheckCanCommenceReplication(
        ScheduledShardReplication nextReplicationStep,
        CancellationToken cancellationToken
    )
    {
        try
        {
            // Get current cluster state and compare that the expected cluster state in the replication step
            // is the same as the obtained current state

            var currentClusterInfo = (await _qdrantClient.GetClusterInfo(cancellationToken, _clusterName)).EnsureSuccess();

            var currentCollectionClusteringInfo = (
                await _qdrantClient.GetCollectionClusteringInfo(_collectionName, cancellationToken, clusterName: _clusterName)
            ).EnsureSuccess();

            var currentCollectionClusteringState = new CollectionClusteringState(
                currentClusterInfo,
                currentCollectionClusteringInfo,
                _targetReplicationFactor
            );

            var ongoingShardTransfersCount = currentCollectionClusteringInfo.ShardTransfers.Length;

            if (ongoingShardTransfersCount != 0)
            {
                // We don't allow performing any replication-related operations while there are any ongoing shard transfers

                // Clear the plan to force user to retry operation from the beginning
                _replicationPlan.Clear();

                return (
                    false,
                    new ReplicateShardsToPeerResponse()
                    {
                        Result = null,
                        Status = QdrantStatus.Fail(
                            $"Can't restore shard replication factor. Found {ongoingShardTransfersCount} ongoing shard transfers. To ensure that collection data is not corrupted, wait for ongoing shard transfers to finish and start restore shard replication factor process again."
                        ),
                    }
                );
            }

            if (!currentCollectionClusteringState.Equals(nextReplicationStep.ExpectedInitialState))
            {
                // We don't allow performing any replication-related when cluster is in unexpected state

                // Clear the plan to force user to retry operation from the beginning
                _replicationPlan.Clear();

                return (
                    false,
                    new ReplicateShardsToPeerResponse()
                    {
                        Result = null,
                        Status = QdrantStatus.Fail(
                            $"Can't restore shard replication factor. Looks like collection clustering was changed parallel to the replication process by ShardReplicator. To ensure that collection data is not corrupted, restart restore shard replication factor process."
                        ),
                    }
                );
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            // Clear the plan to force user to retry operation from the beginning
            _replicationPlan.Clear();

            return (
                false,
                new ReplicateShardsToPeerResponse()
                {
                    Result = null,
                    Status = QdrantStatus.Fail($"Can't restore shard replication factor. An exception happened : {ex}."),
                }
            );
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
