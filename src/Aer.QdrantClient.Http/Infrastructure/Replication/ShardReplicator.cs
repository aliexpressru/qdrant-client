using Aer.QdrantClient.Http.Collections;
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Aer.QdrantClient.Http.Infrastructure.Replication;

/// <summary>
/// Represents a component responsible for managing and
/// executing shard replication within a distributed collection.
/// Calculates which shards require additional replicas and coordinates
/// their replication to achieve the target replication factor across peers.
/// </summary>
/// <remarks>Use this class to ensure that all shards in a collection
/// are replicated according to the configured or inferred replication factor.
/// The class provides methods to determine replication needs
/// and to perform asynchronous replication operations.
/// </remarks>
internal class ShardReplicator
{
    private readonly QdrantHttpClient _qdrantClient;
    private readonly ILogger _logger;
    private readonly string _collectionName;
    private readonly GetCollectionInfoResponse.CollectionInfo _collectionInfo;
    private readonly GetCollectionClusteringInfoResponse.CollectionClusteringInfo _collectionClusteringInfo;
    private int _targetReplicationFactor;

    private List<(uint ShardId, int NumberOfReplicasToAdd)> _shardsToReplicate;

    /// <summary>
    /// Returns <c>true</c> if not sufficiently replicated shards detected.
    /// Use <see cref="ExecuteReplications(CancellationToken, ShardTransferMethod, TimeSpan?)"/> to perform the replication sequence.
    /// </summary>
    public bool ShardsNeedReplication => _shardsToReplicate is { Count: > 0 };

    internal ShardReplicator(
        QdrantHttpClient qdrantClient,
        ILogger logger,
        string collectionName,
        GetCollectionInfoResponse.CollectionInfo collectionInfo,
        GetCollectionClusteringInfoResponse.CollectionClusteringInfo collectionClusteringInfo)
    {
        _qdrantClient = qdrantClient;
        _logger = logger;
        _collectionName = collectionName;
        _collectionInfo = collectionInfo;
        _collectionClusteringInfo = collectionClusteringInfo;
    }

    internal void Calculate()
    {
        _targetReplicationFactor = GetCollectionReplicationFactor();

        // Check that each shard is replicated no fewer than targetCollectionReplicationFactor of times
        // If it is replicated fewer times - replicate to the peers that have the least number of replicas

        _shardsToReplicate = new(_collectionClusteringInfo.PeersByShards.Count);

        // Collect shards with unbalanced replicas

        foreach (var (shardId, peerIds) in _collectionClusteringInfo.PeersByShards)
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
                    break;

                case < 0:
                    // shard is replicated fewer times than expected
                    _shardsToReplicate.Add((shardId, _targetReplicationFactor - peerIds.Count));
                    break;
            }
        }

        if (_logger?.IsEnabled(LogLevel.Information) == true)
        {
            if (ShardsNeedReplication)
            {
                // Not enough replicas

                _logger?.LogInformation(
                    "Collection {CollectionName} replication factor : {ReplicationFactor}. {ShardsToReplicateUpCount} to additionally replicate",
                    _collectionName,
                    _targetReplicationFactor,
                    _shardsToReplicate.Count
                );
            }
            else
            {
                // Enough replicas

                _logger?.LogInformation(
                    "Collection {CollectionName} replication factor : {ReplicationFactor}. All shards have no fewer than configured replicas",
                    _collectionName,
                    _targetReplicationFactor
                );
            }
        }
    }

    /// <summary>
    /// Asynchronously replicates the specified shards to target peers if needed.
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
    /// An asynchronous stream of tuples, each containing the shard ID,
    /// source peer ID, and target peer ID for each replication performed.
    /// </returns>
    /// <remarks>
    /// It is advised to check on each returned replication status before continuing with the next one.
    /// </remarks>
    public async IAsyncEnumerable<(uint ShardId, ulong SourcePeerId, ulong TargetPeerId, ReplicateShardsToPeerResponse replicateShardResponse)> ExecuteReplications(
        [EnumeratorCancellation] CancellationToken cancellationToken,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        TimeSpan? timeout = null)
    {
        foreach (var (shardId, replicasToAdd) in _shardsToReplicate)
        {
            // select target peers which do not have specified shard replica
            var targetPeerIds = _collectionClusteringInfo
                .ShardsByPeers.Where(kv => !kv.Value.Contains(shardId))
                .Select(kv => kv.Key)
                .ToArray();

            HashSet<ulong> sourcePeerIds = _collectionClusteringInfo.PeersByShards[shardId];

            CircularEnumerable<ulong> sourcePeers = new(sourcePeerIds);
            CircularEnumerable<ulong> targetPeers = new(targetPeerIds);

            int replicasLeftToAdd = replicasToAdd;

            while (replicasLeftToAdd > 0)
            {
                var sourcePeerId = sourcePeers.GetNext();
                var targetPeerId = targetPeers.GetNext();

                var replicateShardStartResponse = await _qdrantClient.UpdateCollectionClusteringSetup(
                    _collectionName,
                    UpdateCollectionClusteringSetupRequest.CreateReplicateShardRequest(
                        shardId,
                        sourcePeerId,
                        targetPeerId,
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
                        Result = true,
                        Status = QdrantStatus.Success(),
                        Time = replicateShardStartResponse.Time
                    };
                }
                else
                {
                    replicateShardResponse = new ReplicateShardsToPeerResponse(replicateShardStartResponse)
                    {
                        Result = false
                    };
                }

                replicasLeftToAdd--;

                yield return (
                    shardId,
                    sourcePeerId,
                    targetPeerId,
                    replicateShardResponse);
            }
        }
    }

    private int GetCollectionReplicationFactor()
    {
        if (_collectionInfo.Config.Params.ReplicationFactor.HasValue)
        {
            return (int)_collectionInfo.Config.Params.ReplicationFactor.Value;
        }

        // If no replication factor returned from collectionInfo - assume the largest number of replicas across all collection shards is target replication factor

        int selectedReplicationFactor = 0;

        foreach (var (shardId, peerIds) in _collectionClusteringInfo.PeersByShards)
        {
            if (peerIds.Count > selectedReplicationFactor)
            {
                selectedReplicationFactor = peerIds.Count;
            }
        }

        return selectedReplicationFactor;
    }
}
