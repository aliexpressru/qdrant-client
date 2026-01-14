using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Abstractions;

public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Get information about the current state and composition of the cluster (shards).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<GetClusterInfoResponse> GetClusterInfo(
        CancellationToken cancellationToken,
        string clusterName = null);

    /// <summary>
    /// Tries to recover current peer Raft state.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<DefaultOperationResponse> RecoverPeerRaftState(
        CancellationToken cancellationToken,
        string clusterName = null);

    /// <summary>
    /// Removes the specified peer (shard) from the cluster.
    /// </summary>
    /// <param name="peerId">The identifier of the peer to drop.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isForceDropOperation">If <c>true</c> - removes peer even if it has shards/replicas on it.</param>
    /// <param name="timeout">The operation timeout. If not set the default value of 30 seconds used.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<DefaultOperationResponse> RemovePeer(
        ulong peerId,
        CancellationToken cancellationToken,
        bool isForceDropOperation = false,
        TimeSpan? timeout = null,
        string clusterName = null);

    /// <summary>
    /// Get clustering (sharding) information for a collection.
    /// </summary>
    /// <param name="collectionName">Collection name to get sharding info for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isTranslatePeerIdsToUris">If set to <c>true</c>, enriches collection cluster info response with peer URI values.</param>
    Task<GetCollectionClusteringInfoResponse> GetCollectionClusteringInfo(
        string collectionName,
        CancellationToken cancellationToken,
        bool isTranslatePeerIdsToUris = false);

    /// <summary>
    /// Update collection clustering (sharding) setup.
    /// </summary>
    /// <param name="collectionName">Collection name to update sharding info for.</param>
    /// <param name="updateOperation">The required collection clustering setup update operation model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">The operation timeout. If not set the default value of 30 seconds used.</param>
    Task<DefaultOperationResponse> UpdateCollectionClusteringSetup(
        string collectionName,
        UpdateCollectionClusteringSetupRequest updateOperation,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null);

    /// <summary>
    /// Creates collection shards with specified shard key.
    /// </summary>
    /// <param name="collectionName">Collection name to create shard key for.</param>
    /// <param name="shardKey">The shard key for shards to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="shardsNumber">How many shards to create for this key. If not specified, will use the default value from config.</param>
    /// <param name="replicationFactor">How many replicas to create for each shard. If not specified, will use the default value from config.</param>
    /// <param name="placement">
    /// Placement of shards for this key - array of peer ids, that can be used to place shards for this key.
    /// If not specified, will be randomly placed among all peers.
    /// </param>
    /// <param name="timeout">The operation timeout. If not set the default value of 30 seconds used.</param>
    /// <param name="initialState">
    /// Initial state of the shards for this key.
    /// If not specified, will be <see cref="ShardState.Initializing"/> first and then <see cref="ShardState.Active"/>.
    /// Warning: do not change this unless you know what you are doing
    /// </param>
    Task<DefaultOperationResponse> CreateShardKey(
        string collectionName,
        ShardKey shardKey,
        CancellationToken cancellationToken,
        uint? shardsNumber = null,
        uint? replicationFactor = null,
        ulong[] placement = null,
        TimeSpan? timeout = null,
        ShardState? initialState = null);

    /// <summary>
    /// Deletes collection shards with specified shard key.
    /// </summary>
    /// <param name="collectionName">Collection name to delete shard key for.</param>
    /// <param name="shardKey">The shard key for shards to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">The operation timeout. If not set the default value of 30 seconds used.</param>
    Task<DefaultOperationResponse> DeleteShardKey(
        string collectionName,
        ShardKey shardKey,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null);
}
