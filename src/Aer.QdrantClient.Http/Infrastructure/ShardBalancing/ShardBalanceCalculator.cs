using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
using Aer.QdrantClient.Http.Models.Responses;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Http.Infrastructure.ShardBalancing;

internal class ShardBalanceCalculator(
    string collectionName,
    GetCollectionInfoResponse.CollectionInfo collectionInfo,
    GetCollectionClusteringInfoResponse.CollectionClusteringInfo collectionClusteringInfo)
{
    private int _targetReplicationFactor;

    public bool HasUnbalancedReplicas { get; private set; }

    public void Calculate(ILogger logger)
    {
        _targetReplicationFactor = GetCollectionReplicationFactor();

        // Check that each shard is replicated targetCollectionReplicationFactor of times
        // If it is replicated more than that - drop extra replicas
        // If it is replicated fewer times - replicate to the peers that have the least number of replicas

        List<(uint ShardId, int NumberOfReplicasToAdd)> shardsToScaleUp =
            new(collectionClusteringInfo.PeersByShards.Count);

        List<(uint ShardId, int NumberOfReplicasToDrop)> shardsToScaleDown =
            new(collectionClusteringInfo.PeersByShards.Count);

        Dictionary<uint, List<ulong>> peersWithExtraReplicasForShard = new(16);

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
                    shardsToScaleDown.Add((shardId, peerIds.Count - _targetReplicationFactor));
                    break;

                case < 0:
                    // shard is replicated fewer times than expected
                    shardsToScaleUp.Add((shardId, _targetReplicationFactor - peerIds.Count));
                    break;
            }
        }

        HasUnbalancedReplicas =
            shardsToScaleUp.Count > 0
            || shardsToScaleDown.Count > 0;

        if (logger?.IsEnabled(LogLevel.Information) == true)
        {
            if (HasUnbalancedReplicas)
            {
                // Unbalanced replicas

                logger?.LogInformation(
                    "Collection {CollectionName} replication factor : {ReplicationFactor}. Found unbalanced replicas. {ShardsToScaleUpCount} to additionally replicate, {ShardsToScaleDownCount} with extra replicas",
                    collectionName,
                    _targetReplicationFactor,
                    shardsToScaleUp.Count,
                    shardsToScaleDown.Count
                );
            }
            else
            {
                // Balanced replicas

                logger?.LogInformation(
                    "Collection {CollectionName} replication factor : {ReplicationFactor}. No unbalanced replicas found",
                    collectionName,
                    _targetReplicationFactor
                );
            }
        }
    }

    public async Task ExecuteRebalance(QdrantHttpClient qdrantClient)
    {

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
