using Aer.QdrantClient.Http.Collections;
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Aer.QdrantClient.Http.Infrastructure.Replication;

internal class ShardReplicator(
    string collectionName,
    GetCollectionInfoResponse.CollectionInfo collectionInfo,
    GetCollectionClusteringInfoResponse.CollectionClusteringInfo collectionClusteringInfo
)
{
    private int _targetReplicationFactor;

    private List<(uint ShardId, int NumberOfReplicasToAdd)> _shardsToReplicate;

    public bool ShardsNeedReplication => _shardsToReplicate is { Count: > 0 };

    public void Calculate(ILogger logger)
    {
        _targetReplicationFactor = GetCollectionReplicationFactor();

        // Check that each shard is replicated no fewer than targetCollectionReplicationFactor of times
        // If it is replicated fewer times - replicate to the peers that have the least number of replicas

        _shardsToReplicate = new(collectionClusteringInfo.PeersByShards.Count);

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
                    break;

                case < 0:
                    // shard is replicated fewer times than expected
                    _shardsToReplicate.Add((shardId, _targetReplicationFactor - peerIds.Count));
                    break;
            }
        }

        if (logger?.IsEnabled(LogLevel.Information) == true)
        {
            if (ShardsNeedReplication)
            {
                // Not enough replicas

                logger?.LogInformation(
                    "Collection {CollectionName} replication factor : {ReplicationFactor}. {ShardsToReplicateUpCount} to additionally replicate",
                    collectionName,
                    _targetReplicationFactor,
                    _shardsToReplicate.Count
                );
            }
            else
            {
                // Enough replicas

                logger?.LogInformation(
                    "Collection {CollectionName} replication factor : {ReplicationFactor}. All shards have no fewer than configured replicas",
                    collectionName,
                    _targetReplicationFactor
                );
            }
        }
    }

    public async IAsyncEnumerable<(uint ShardId, ulong SourcePeerId, ulong TargetPeerId)> ExecuteReplications(
        QdrantHttpClient qdrantClient,
        [EnumeratorCancellation] CancellationToken cancellationToken,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        TimeSpan? timeout = null
    )
    {
        foreach (var (shardId, replicasToAdd) in _shardsToReplicate)
        {
            // select target peers which do not have specified shard replica
            var targetPeerIds = collectionClusteringInfo
                .ShardsByPeers.Where(kv => !kv.Value.Contains(shardId))
                .Select(kv => kv.Key)
                .ToArray();

            HashSet<ulong> sourcePeerIds = collectionClusteringInfo.PeersByShards[shardId];

            CircularEnumerable<ulong> sourcePeers = new(sourcePeerIds);
            CircularEnumerable<ulong> targetPeers = new(targetPeerIds);

            int replicasLeftToAdd = replicasToAdd;

            while (replicasLeftToAdd > 0)
            {
                var sourcePeerId = sourcePeers.GetNext();
                var targetPeerId = targetPeers.GetNext();

                var replicateShardStartResponse = await qdrantClient.UpdateCollectionClusteringSetup(
                    collectionName,
                    UpdateCollectionClusteringSetupRequest.CreateReplicateShardRequest(
                        shardId,
                        sourcePeerId,
                        targetPeerId,
                        shardTransferMethod
                    ),
                    cancellationToken,
                    timeout
                );

                replicateShardStartResponse.EnsureSuccess();

                replicasLeftToAdd--;

                yield return (shardId, sourcePeerId, targetPeerId);
            }
        }
    }

    private int GetCollectionReplicationFactor()
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
