using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Http;

/// <summary>
/// Interface for Qdrant HTTP API client.
/// </summary>
public interface IQdrantHttpClient
{
    /// <summary>
    /// Replicates shards for specified or all collections from one peer to the other.
    /// </summary>
    /// <param name="sourcePeerId">The peer id for the peer to replicate shards from.</param>
    /// <param name="targetPeerId">The peer id for the peer to replicate shards to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="shardTransferMethod">
    /// Method for transferring the shard from one node to another.
    /// If not set, <see cref="ShardTransferMethod.Snapshot"/> will be used.
    /// </param>
    /// <param name="isMoveShards">If set to <c>true</c> moves shards to the target peer instead of copying them.</param>
    /// <param name="collectionNamesToReplicate">
    /// Collection names to replicate shards for.
    /// If <c>null</c> or empty - replicates all collection shards.
    /// </param>
    /// <param name="shardIdsToReplicate">
    /// The ids of the shards to replicate to the target peer.
    /// If null or empty - replicates all shards that are missing on the target peer.
    /// </param>
    Task<ReplicateShardsToPeerResponse> ReplicateShards(
        ulong sourcePeerId,
        ulong targetPeerId,
        CancellationToken cancellationToken,
        ILogger logger,
        bool isDryRun,
        ShardTransferMethod shardTransferMethod,
        bool isMoveShards,
        string[] collectionNamesToReplicate,
        uint[] shardIdsToReplicate);

    /// <summary>
    /// Replicates shards for specified or all collections from one peer to the other.
    /// </summary>
    /// <param name="sourcePeerUriSelectorString">The peer uri selector string for the peer to replicate shards from.</param>
    /// <param name="targetPeerUriSelectorString">The peer uri selector string for the peer to replicate shards to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="shardTransferMethod">
    /// Method for transferring the shard from one node to another.
    /// If not set, <see cref="ShardTransferMethod.Snapshot"/> will be used.
    /// </param>
    /// <param name="isMoveShards">If set to <c>true</c> moves shards to the target peer instead of copying them.</param>
    /// <param name="collectionNamesToReplicate">
    /// Collection names to replicate shards for.
    /// If <c>null</c> or empty - replicates all collection shards.
    /// </param>
    /// <param name="shardIdsToReplicate">
    /// The ids of the shards to replicate to the target peer.
    /// If null or empty - replicates all shards that are missing on the target peer.
    /// </param>
    Task<ReplicateShardsToPeerResponse> ReplicateShards(
        string sourcePeerUriSelectorString,
        string targetPeerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger,
        bool isDryRun,
        ShardTransferMethod shardTransferMethod,
        bool isMoveShards,
        string[] collectionNamesToReplicate,
        uint[] shardIdsToReplicate);

    /// <summary>
    /// Replicates shards for specified or all collections to specified peer.
    /// </summary>
    /// <param name="targetPeerId">The peer id for the peer to replicate shards to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="shardTransferMethod">
    /// Method for transferring the shard from one node to another.
    /// If not set, <see cref="ShardTransferMethod.Snapshot"/> will be used.
    /// </param>
    /// <param name="collectionNamesToReplicate">
    /// Collection names to replicate shards for.
    /// If <c>null</c> or empty - replicates all collection shards.
    /// </param>
    Task<ReplicateShardsToPeerResponse> ReplicateShardsToPeer(
        ulong targetPeerId,
        CancellationToken cancellationToken,
        ILogger logger,
        bool isDryRun,
        ShardTransferMethod shardTransferMethod,
        params string[] collectionNamesToReplicate);

    /// <summary>
    /// Replicates shards for specified or all collections to specified peer.
    /// </summary>
    /// <param name="targetPeerUriSelectorString">The peer uri selector string for the peer to replicate shards to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="shardTransferMethod">
    /// Method for transferring the shard from one node to another.
    /// If not set, <see cref="ShardTransferMethod.Snapshot"/> will be used.
    /// </param>
    /// <param name="collectionNamesToReplicate">
    /// Collection names to replicate shards for.
    /// If <c>null</c> or empty - replicates all collection shards.
    /// </param>
    Task<ReplicateShardsToPeerResponse> ReplicateShardsToPeer(
        string targetPeerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger,
        bool isDryRun,
        ShardTransferMethod shardTransferMethod,
        params string[] collectionNamesToReplicate);

    /// <summary>
    /// Equalizes shard replication between source and empty target peers for specified collections.
    /// The final result will be moving shards form source to empty target until equal number of shard replicas on both peers.
    /// </summary>
    /// <param name="collectionNamesToEqualize">Collection names to equalize shard replication for.</param>
    /// <param name="sourcePeerUriSelectorString">The peer uri selector string for the peer to replicate shards from.</param>
    /// <param name="emptyTargetPeerUriSelectorString">The peer uri selector string for the empty peer to replicate shards to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="shardTransferMethod">
    /// Method for transferring the shard from one node to another.
    /// If not set, <see cref="ShardTransferMethod.Snapshot"/> will be used.
    /// </param>
    Task<ReplicateShardsToPeerResponse> EqualizeShardReplication(
        string[] collectionNamesToEqualize,
        string sourcePeerUriSelectorString,
        string emptyTargetPeerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger,
        bool isDryRun,
        ShardTransferMethod shardTransferMethod);

    /// <summary>
    /// Equalizes shard replication between source and empty target peers for specified collections.
    /// The final result will be moving shards form source to empty target until equal number of shard replicas on both peers.
    /// </summary>
    /// <param name="collectionNamesToEqualize">Collection names to restore shard replication for.</param>
    /// <param name="sourcePeerId">The peer id for the peer to replicate shards from.</param>
    /// <param name="emptyTargetPeerId">The peer id for the peer to replicate shards to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="shardTransferMethod">
    /// Method for transferring the shard from one node to another.
    /// If not set, <see cref="ShardTransferMethod.Snapshot"/> will be used.
    /// </param>
    Task<ReplicateShardsToPeerResponse> EqualizeShardReplication(
        string[] collectionNamesToEqualize,
        ulong sourcePeerId,
        ulong emptyTargetPeerId,
        CancellationToken cancellationToken,
        ILogger logger,
        bool isDryRun,
        ShardTransferMethod shardTransferMethod);

    /// <summary>
    /// Removes all shards for all collections or specified collections from a peer by distributing them between another peers.
    /// </summary>
    /// <param name="peerId">The peer id for the peer to move shards away from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="shardTransferMethod">
    /// Method for transferring the shard from one node to another.
    /// If not set, <see cref="ShardTransferMethod.Snapshot"/> will be used.
    /// </param>
    /// <param name="collectionNamesToMove">
    /// Collection names to move shards for.
    /// If <c>null</c> or empty - moves all collection shards.
    /// </param>
    Task<DrainPeerResponse> DrainPeer(
        ulong peerId,
        CancellationToken cancellationToken,
        ILogger logger,
        bool isDryRun,
        ShardTransferMethod shardTransferMethod,
        params string[] collectionNamesToMove
    );

    /// <summary>
    /// Removes all shards for all collections or specified collections from a peer by distributing them between another peers.
    /// </summary>
    /// <param name="peerUriSelectorString">The peer uri selector string for the peer to move shards away from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="shardTransferMethod">
    /// Method for transferring the shard from one node to another.
    /// If not set, <see cref="ShardTransferMethod.Snapshot"/> will be used.
    /// </param>
    /// <param name="collectionNamesToMove">
    /// Collection names to move shards for.
    /// If <c>null</c> or empty - moves all collection shards.
    /// </param>
    Task<DrainPeerResponse> DrainPeer(
        string peerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger,
        bool isDryRun,
        ShardTransferMethod shardTransferMethod,
        params string[] collectionNamesToMove
    );

    /// <summary>
    /// Drops all shards for all collections or specified collections from a peer.
    /// </summary>
    /// <param name="peerId">The peer id for the peer to drop shards from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="collectionNamesToClear">
    /// Collection names to drop shards for.
    /// If <c>null</c> or empty - drops all collection shards.
    /// </param>
    Task<ClearPeerResponse> ClearPeer(
        ulong peerId,
        CancellationToken cancellationToken,
        ILogger logger,
        bool isDryRun,
        params string[] collectionNamesToClear
    );

    /// <summary>
    /// Drops all shards for all collections or specified collections from a peer.
    /// </summary>
    /// <param name="peerUriSelectorString">The peer uri selector string for the peer to drop shards from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="collectionNamesToClear">
    /// Collection names to drop shards for.
    /// If <c>null</c> or empty - drops all collection shards.
    /// </param>
    Task<ClearPeerResponse> ClearPeer(
        string peerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger,
        bool isDryRun,
        params string[] collectionNamesToClear
    );

    /// <summary>
    /// Checks whether the specified cluster node does not have any collection shards on it.
    /// </summary>
    /// <param name="peerId">The cluster node peer id for the peer to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<CheckIsPeerEmptyResponse> CheckIsPeerEmpty(
        ulong peerId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether the specified cluster node does not have any collection shards on it.
    /// </summary>
    /// <param name="peerUriSelectorString">The cluster node uri selector for the peer to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<CheckIsPeerEmptyResponse> CheckIsPeerEmpty(
        string peerUriSelectorString,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the peer information by the peer node uri substring or by peer id. Returns the found peer and other peers.
    /// </summary>
    /// <param name="peerUriSelectorString">Peer uri substring to get peer info for or <c>null</c> if using <paramref name="peerId"/>.</param>
    /// <param name="peerId">Cluster node peer id to get peer info for or <c>null</c> if using <paramref name="peerUriSelectorString"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="QdrantNoPeersFoundForUriSubstringException">Occurs when no nodes found for uri substring.</exception>
    /// <exception cref="QdrantMoreThanOnePeerFoundForUriSubstringException">Occurs when more than one node found for uri substring.</exception>
    Task<GetPeerResponse> GetPeerInfo(
        string peerUriSelectorString,
        ulong? peerId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the peer information by the peer node uri substring. Returns the found peer and other peers.
    /// </summary>
    /// <param name="peerUriSelectorString">Peer uri substring to get peer info for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="QdrantNoPeersFoundForUriSubstringException">Occurs when no nodes found for uri substring.</exception>
    /// <exception cref="QdrantMoreThanOnePeerFoundForUriSubstringException">Occurs when more than one node found for uri substring.</exception>
    Task<GetPeerResponse> GetPeerInfo(string peerUriSelectorString, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the peer information by the peer node uri substring. Returns the found peer and other peers.
    /// </summary>
    /// <param name="peerId">Cluster node peer is to get peer info for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="QdrantNoPeersFoundForUriSubstringException">Occurs when no nodes found for uri substring.</exception>
    /// <exception cref="QdrantMoreThanOnePeerFoundForUriSubstringException">Occurs when more than one node found for uri substring.</exception>
    Task<GetPeerResponse> GetPeerInfo(ulong peerId, CancellationToken cancellationToken);

    Task<GetPeerResponse>
        GetPeerInfoByUriSubstring(
            string clusterNodeUriSubstring,
            CancellationToken cancellationToken);

    /// <summary>
    /// Get information about the current state and composition of the cluster (shards).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<GetClusterInfoResponse> GetClusterInfo(
        CancellationToken cancellationToken);

    /// <summary>
    /// Tries to recover current peer Raft state.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<DefaultOperationResponse> RecoverPeerRaftState(
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes the specified peer (shard) from the cluster.
    /// </summary>
    /// <param name="peerId">The identifier of the peer to drop.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isForceDropOperation">If <c>true</c> - removes peer even if it has shards/replicas on it.</param>
    /// <param name="timeout">The operation timeout. If not set the default value of 30 seconds used.</param>
    Task<DefaultOperationResponse> RemovePeer(
        ulong peerId,
        CancellationToken cancellationToken,
        bool isForceDropOperation,
        TimeSpan? timeout);

    /// <summary>
    /// Get clustering (sharding) information for a collection.
    /// </summary>
    /// <param name="collectionName">Collection name to get sharding info for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isTranslatePeerIdsToUris">If set to <c>true</c>, enriches collection cluster info response with peer URI values.</param>
    Task<GetCollectionClusteringInfoResponse> GetCollectionClusteringInfo(
        string collectionName,
        CancellationToken cancellationToken,
        bool isTranslatePeerIdsToUris);

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
        TimeSpan? timeout);

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
    Task<DefaultOperationResponse> CreateShardKey(
        string collectionName,
        ShardKey shardKey,
        CancellationToken cancellationToken,
        uint? shardsNumber,
        uint? replicationFactor,
        ulong[] placement,
        TimeSpan? timeout);

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
        TimeSpan? timeout);

    /// <summary>
    /// Get the detailed information about specified existing collection.
    /// </summary>
    /// <param name="collectionName">Collection name to get info for.</param>
    /// <param name="isCountExactPointsNumber">If set to <c>true</c> counts the exact number of points in collection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<GetCollectionInfoResponse> GetCollectionInfo(
        string collectionName,
        bool isCountExactPointsNumber,
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Get the detailed information about all existing collections.
    /// </summary>
    /// <param name="isCountExactPointsNumber">If set to <c>true</c> counts collection points for all collections.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<ListCollectionInfoResponse> ListCollectionInfo(
        bool isCountExactPointsNumber,
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Create collection.
    /// </summary>
    /// <param name="collectionName">Collection name. Must be maximum 255 characters long.</param>
    /// <param name="request">The collection creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<DefaultOperationResponse> CreateCollection(
        string collectionName,
        CreateCollectionRequest request,
        CancellationToken cancellationToken,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Update parameters of the existing collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to update.</param>
    /// <param name="request">Collection parameters to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout in seconds. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<DefaultOperationResponse> UpdateCollectionParameters(
        string collectionName,
        UpdateCollectionParametersRequest request,
        CancellationToken cancellationToken,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Trigger optimizers on existing collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout in seconds. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    /// <remarks>Issues the empty update collection parameters request to start optimizers for grey collections. https://qdrant.tech/documentation/concepts/collections/#grey-collection-status</remarks>
    Task<DefaultOperationResponse> TriggerOptimizers(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Get the detailed information about specified existing collection.
    /// </summary>
    /// <param name="collectionName">Collection name to get info for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<GetCollectionInfoResponse> GetCollectionInfo(
        string collectionName,
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Get the names of all the existing collections.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<ListCollectionsResponse> ListCollections(CancellationToken cancellationToken);

    /// <summary>
    /// Delete collection by name.
    /// </summary>
    /// <param name="collectionName">The name of the collection to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<DefaultOperationResponse> DeleteCollection(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Get list of all existing collections aliases.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<ListCollectionAliasesResponse> ListAllAliases(
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Get list of all aliases for a specified collection.
    /// </summary>
    /// <param name="collectionName">The name of the collection to list aliases for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<ListCollectionAliasesResponse> ListCollectionAliases(
        string collectionName,
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Execute multiple collection aliases update operations in one batch.
    /// </summary>
    /// <param name="updateCollectionAliasesRequest">The request with update aliases operations batch.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<DefaultOperationResponse> UpdateCollectionsAliases(
        UpdateCollectionAliasesRequest updateCollectionAliasesRequest,
        CancellationToken cancellationToken,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Checks whether collection with specified name exists.
    /// </summary>
    /// <param name="collectionName">The name of the collection to check existence for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<CheckCollectionExistsResponse> CheckCollectionExists(
        string collectionName,
        CancellationToken cancellationToken);

    Task<PayloadIndexOperationResponse> CreatePayloadIndex(
        string collectionName,
        string payloadFieldName,
        PayloadIndexedFieldType payloadFieldType,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        bool onDisk,

        bool? isTenant,
        bool? isPrincipal,

        bool? isLookupEnabled,
        bool? isRangeEnabled,

        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Creates the full text index on specified payload text field.
    /// </summary>
    /// <remarks>
    /// For indexing, it is recommended to choose the field that limits the search result the most.
    /// As a rule, the more different values a payload value has, the more efficient the index will be used.
    /// You should not create an index for Boolean fields and fields with only a few possible values.
    /// </remarks>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="payloadTextFieldName">Name of the indexed payload text field.</param>
    /// <param name="payloadTextFieldTokenizerType">Type of the payload text field tokenizer.</param>
    /// <param name="minimalTokenLength">The minimal word token length.</param>
    /// <param name="maximalTokenLength">The maximal word token length.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isLowercasePayloadTokens">If <c>true</c>, lowercase all tokens. Default: <c>true</c>.</param>
    /// <param name="onDisk">
    /// If set to <c>true</c> the payload will be stored on-disk instead of in-memory.
    /// On-disk payload index might affect cold requests latency, as it requires additional disk I/O operations.
    /// </param>
    /// <param name="enablePhraseMatching">Enable phrase matching on this text field.</param>
    /// <param name="stemmer">Algorithm for stemming. If <c>null</c> stemming is disabled.</param>
    /// <param name="stopwords">Ignore this set of tokens. Can select from predefined languages and/or provide a custom set.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<PayloadIndexOperationResponse> CreateFullTextPayloadIndex(
        string collectionName,
        string payloadTextFieldName,
        FullTextIndexTokenizerType payloadTextFieldTokenizerType,
        CancellationToken cancellationToken,

        uint? minimalTokenLength,
        uint? maximalTokenLength,

        bool isLowercasePayloadTokens,
        bool onDisk,
        bool enablePhraseMatching,

        FullTextIndexStemmingAlgorithm stemmer,
        FullTextIndexStopwords stopwords,

        bool isWaitForResult,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Deletes the index for a payload field.
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="fieldName">Name of the field to delete index for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<PayloadIndexOperationResponse> DeletePayloadIndex(
        string collectionName,
        string fieldName,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Delete points by specified ids.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete points from.</param>
    /// <param name="pointIds">The point ids to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="shardSelector">The shard selector. If set, performs operation only on specified shard(s).</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The delete operation ordering settings.</param>
    Task<PointsOperationResponse> DeletePoints(
        string collectionName,
        IEnumerable<PointId> pointIds,
        CancellationToken cancellationToken,
        ShardSelector shardSelector,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Delete points by specified filters.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete points from.</param>
    /// <param name="filter">The filter to find points to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="shardSelector">The shard selector. If set, performs operation only on specified shard(s).</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The delete operation ordering settings.</param>
    Task<PointsOperationResponse> DeletePoints(
        string collectionName,
        QdrantFilter filter,
        CancellationToken cancellationToken,
        ShardSelector shardSelector,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Perform insert + updates on points. If point with given id already exists - it will be overwritten.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete points from.</param>
    /// <param name="upsertPoints">The point data to upsert.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The upsert operation ordering settings.</param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    Task<PointsOperationResponse> UpsertPoints<TPayload>(
        string collectionName,
        UpsertPointsRequest<TPayload> upsertPoints,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering)
        where TPayload : class;

    /// <summary>
    /// Set payload keys values for points.
    /// Sets only the specified keys leaving all other intact.
    /// </summary>
    /// <param name="collectionName">Name of the collection to set payload for.</param>
    /// <param name="setPointsPayload">Set points payload request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    Task<PointsOperationResponse> SetPointsPayload<TPayload>(
        string collectionName,
        SetPointsPayloadRequest<TPayload> setPointsPayload,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering)
        where TPayload : class;

    /// <summary>
    /// Replace full payload of points with new one.
    /// </summary>
    /// <param name="collectionName">Name of the collection to set payload for.</param>
    /// <param name="overwritePointsPayload">Overwrite points payload request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    Task<PointsOperationResponse> OverwritePointsPayload<TPayload>(
        string collectionName,
        OverwritePointsPayloadRequest<TPayload> overwritePointsPayload,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering)
        where TPayload : class;

    /// <summary>
    /// Delete specified payload keys for points.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete payload for.</param>
    /// <param name="deletePointsPayloadKeys">Delete points payload keys request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    Task<PointsOperationResponse> DeletePointsPayloadKeys(
        string collectionName,
        DeletePointsPayloadKeysRequest deletePointsPayloadKeys,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Delete specified payload keys for points.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete payload for.</param>
    /// <param name="clearPointsPayload">Clear points payload request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    Task<PointsOperationResponse> ClearPointsPayload(
        string collectionName,
        ClearPointsPayloadRequest clearPointsPayload,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Update specified named vectors on points, keep unspecified vectors intact.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete payload for.</param>
    /// <param name="updatePointsVectors">Update points vectors request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    Task<PointsOperationResponse> UpdatePointsVectors(
        string collectionName,
        UpdatePointsVectorsRequest updatePointsVectors,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Delete named vectors from the given points.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete payload for.</param>
    /// <param name="deletePointsVectors">Delete points vectors request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    Task<PointsOperationResponse> DeletePointsVectors(
        string collectionName,
        DeletePointsVectorsRequest deletePointsVectors,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Apply a series of update operations for points, vectors and payloads.
    /// Operations are executed sequentially in order of appearance in <see cref="BatchUpdatePointsRequest"/>.
    /// </summary>
    /// <param name="collectionName">Name of the collection to apply operations to.</param>
    /// <param name="batchUpdatePointsRequest">The request with operation sequence definition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    Task<BatchPointsOperationResponse> BatchUpdate(
        string collectionName,
        BatchUpdatePointsRequest batchUpdatePointsRequest,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Retrieve full information of single point by id.
    /// </summary>
    /// <param name="collectionName">Name of the collection to retrieve point from.</param>
    /// <param name="pointId">The identifier of the point to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<GetPointResponse> GetPoint(
        string collectionName,
        PointId pointId,
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieve multiple points by specified ids.
    /// </summary>
    /// <param name="collectionName">Name of the collection to retrieve from.</param>
    /// <param name="pointIds">The point ids to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<GetPointsResponse> GetPoints(
        string collectionName,
        IEnumerable<PointId> pointIds,
        PayloadPropertiesSelector withPayload,
        CancellationToken cancellationToken,
        VectorSelector withVector,
        ReadPointsConsistency consistency,
        ShardSelector shardSelector,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Scroll request - paginate over all points which matches given filtering condition.
    /// </summary>
    /// <param name="collectionName">Name of the collection to retrieve from.</param>
    /// <param name="filter">Look only for points which satisfies this conditions. If not provided - all points.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="limit">Page size. Default: 10.</param>
    /// <param name="offsetPoint">Start ID to read points from.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    /// <param name="orderBySelector">
    /// The ordering field and direction selector.
    /// You can pass a string payload field name value which would be interpreted as order by the specified field in ascending order.
    /// </param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<ScrollPointsResponse> ScrollPoints(
        string collectionName,
        QdrantFilter filter,
        PayloadPropertiesSelector withPayload,
        CancellationToken cancellationToken,
        ulong limit,
        PointId offsetPoint,
        VectorSelector withVector,
        ReadPointsConsistency consistency,
        ShardSelector shardSelector,
        OrderBySelector orderBySelector,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Count points which matches given filtering condition.
    /// </summary>
    /// <param name="collectionName">Name of the collection to count points in.</param>
    /// <param name="countPointsRequest">The count points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<CountPointsResponse> CountPoints(
        string collectionName,
        CountPointsRequest countPointsRequest,
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieves facets for the specified payload field.
    /// </summary>
    /// <param name="collectionName">Name of the collection to facet count points in.</param>
    /// <param name="facetCountPointsRequest">The facet count points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<FacetCountPointsResponse> FacetCountPoints(
        string collectionName,
        FacetCountPointsRequest facetCountPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieve the closest points based on vector similarity and given filtering conditions.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="searchPointsRequest">The search points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsResponse> SearchPoints(
        string collectionName,
        SearchPointsRequest searchPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieve the closest points based on vector similarity and given filtering conditions.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="searchPointsBatchedRequest">The search points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsBatchedResponse> SearchPointsBatched(
        string collectionName,
        SearchPointsBatchedRequest searchPointsBatchedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieve the closest points based on vector similarity and given filtering conditions, grouped by a given payload field.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="searchPointsGroupedRequest">The search points grouped request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsGroupedResponse> SearchPointsGrouped(
        string collectionName,
        SearchPointsGroupedRequest searchPointsGroupedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieves sparse matrix of pairwise distances between points sampled from the collection. Output is a list of pairs of points and their distances.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="searchPointsDistanceMatrixRequest">The search points distance matrix request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsDistanceMatrixPairsResponse> SearchPointsDistanceMatrixPairs(
        string collectionName,
        SearchPointsDistanceMatrixRequest searchPointsDistanceMatrixRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieves sparse matrix of pairwise distances between points sampled from the collection. Output is a form of row and column offsets and list of distances.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="searchPointsDistanceMatrixRequest">The search points distance matrix request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsDistanceMatrixOffsetsResponse> SearchPointsDistanceMatrixOffsets(
        string collectionName,
        SearchPointsDistanceMatrixRequest searchPointsDistanceMatrixRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Look for the points which are closer to stored positive examples and at the same time further to negative examples.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="recommendPointsRequest">The recommend points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsResponse> RecommendPoints(
        string collectionName,
        RecommendPointsRequest recommendPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Look for the points which are closer to stored positive examples and at the same time further to negative examples.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="recommendPointsBatchedRequest">The recommend points batched request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsBatchedResponse> RecommendPointsBatched(
        string collectionName,
        RecommendPointsBatchedRequest recommendPointsBatchedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Look for the points which are closer to stored positive examples
    /// and at the same time further to negative examples, grouped by a given payload field.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="recommendPointsGroupedRequest">The recommend points grouped request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsGroupedResponse> RecommendPointsGrouped(
        string collectionName,
        RecommendPointsGroupedRequest recommendPointsGroupedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Use context and a target to find the most similar points to the target, constrained by the context.
    /// When using only the context (without a target), a special search - called context search - is performed
    /// where pairs of points are used to generate a loss that guides the search towards the zone where
    /// most positive examples overlap. This means that the score minimizes the scenario of finding a point
    /// closer to a negative than to a positive part of a pair. Since the score of a context relates to loss,
    /// the maximum score a point can get is <c>0.0</c>, and it becomes normal that many points can have a score of <c>0.0</c>.
    /// <br/>
    /// When using target (with or without context), the score behaves a little different:
    /// The integer part of the score represents the rank with respect to the context, while the decimal part
    /// of the score relates to the distance to the target. The context part of the score for each pair
    /// is calculated <c>+1</c> if the point is closer to a positive than to a negative part of a pair,
    /// and <c>-1</c> otherwise.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="discoverPointsRequest">The discover points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsResponse> DiscoverPoints(
        string collectionName,
        DiscoverPointsRequest discoverPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Look for points based on target and/or positive and negative example pairs, in batch.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="discoverPointsBatchedRequest">The discover points batched request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsBatchedResponse> DiscoverPointsBatched(
        string collectionName,
        DiscoverPointsBatchedRequest discoverPointsBatchedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Universally query points. This endpoint covers all capabilities of search, recommend, discover, filters.
    /// But also enables hybrid and multi-stage queries.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="queryPointsRequest">The universal query API request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<QueryPointsResponse> QueryPoints(
        string collectionName,
        QueryPointsRequest queryPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Universally query points in batch. This endpoint covers all capabilities of search, recommend, discover, filters.
    /// But also enables hybrid and multi-stage queries.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="queryPointsRequest">The universal query API request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<QueryPointsBatchedResponse> QueryPointsBatched(
        string collectionName,
        QueryPointsBatchedRequest queryPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Universally query points and group results by a specified payload field.
    /// This endpoint covers all capabilities of search, recommend, discover, filters.
    /// But also enables hybrid and multi-stage queries.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="queryPointsRequest">The universal query API request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsGroupedResponse> QueryPointsGrouped(
        string collectionName,
        QueryPointsGroupedRequest queryPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Gets the Qdrant instance details.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<GetInstanceDetailsResponse> GetInstanceDetails(CancellationToken cancellationToken);

    /// <summary>
    /// Get the Qdrant telemetry information.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="detailsLevel">Defines how detailed the telemetry data is.</param>
    /// <param name="isAnonymizeTelemetryData">If set tot <c>true</c>, anonymize the collected telemetry result.</param>
    Task<GetTelemetryResponse> GetTelemetry(
        CancellationToken cancellationToken,
        uint detailsLevel,
        bool isAnonymizeTelemetryData);

    /// <summary>
    /// Collect metrics data including app info, collections info, cluster info and statistics in Prometheus format.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isAnonymizeMetricsData">If set tot <c>true</c>, anonymize the collected metrics result.</param>
    Task<string> GetPrometheusMetrics(
        CancellationToken cancellationToken,
        bool isAnonymizeMetricsData);

    Task<SetLockOptionsResponse> SetLockOptions(
        bool areWritesDisabled,
        string reasonMessage,
        CancellationToken cancellationToken);

    Task<SetLockOptionsResponse> GetLockOptions(CancellationToken cancellationToken);

    Task<ReportIssuesResponse> ReportIssues(CancellationToken cancellationToken);

    Task<ClearReportedIssuesResponse> ClearIssues(CancellationToken cancellationToken);

    /// <summary>
    /// Get list of snapshots for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to get a snapshot list.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<ListSnapshotsResponse> ListCollectionSnapshots(
        string collectionName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Create new snapshot for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to create a snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<CreateSnapshotResponse> CreateCollectionSnapshot(
        string collectionName,
        CancellationToken cancellationToken,
        bool isWaitForResult);

    /// <summary>
    /// Recover local collection data from a local snapshot by its name. This will overwrite any data, stored on
    /// this node, for the collection. If collection does not exist - it will be created.
    /// The snapshot path should be <c>/qdrant/snapshots/{collectionName}/{snapshotName}</c> on the Qdrant node.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="snapshotName">The name of the local snapshot file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverCollectionFromSnapshot(
        string collectionName,
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        SnapshotPriority? snapshotPriority,
        string snapshotChecksum);

    /// <summary>
    /// Recover local collection data from a possibly remote snapshot. This will overwrite any data, stored on
    /// this node, for the collection. If collection does not exist - it will be created.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="snapshotLocationUri">The snapshot location in URI format. Can be either a URL or a <c>file:///</c> path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverCollectionFromSnapshot(
        string collectionName,
        Uri snapshotLocationUri,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        SnapshotPriority? snapshotPriority,
        string snapshotChecksum);

    /// <summary>
    /// Recover local collection data from an uploaded snapshot. This will overwrite any data,
    /// stored on this node, for the collection. If collection does not exist - it will be created.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="snapshotContent">The snapshot content stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverCollectionFromUploadedSnapshot(
        string collectionName,
        Stream snapshotContent,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        SnapshotPriority? snapshotPriority,
        string snapshotChecksum);

    /// <summary>
    /// Download specified snapshot from a collection as a file stream.
    /// </summary>
    /// <param name="collectionName">Name of the collection to download snapshot for.</param>
    /// <param name="snapshotName">Name of the snapshot to download.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<DownloadSnapshotResponse> DownloadCollectionSnapshot(
        string collectionName,
        string snapshotName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Delete snapshot for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete snapshot for.</param>
    /// <param name="snapshotName">Name of the snapshot to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<DefaultOperationResponse> DeleteCollectionSnapshot(
        string collectionName,
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult);

    /// <summary>
    /// A compound operation that lists all snapshots for all collections and shards in the storage.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<ListSnapshotsResponse> ListAllSnapshots(CancellationToken cancellationToken);

    /// <summary>
    /// A compound operation that deletes all existing storage snapshots.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<DefaultOperationResponse> DeleteAllStorageSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult);

    /// <summary>
    /// A compound operation that deletes all existing collection snapshots.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<DefaultOperationResponse> DeleteAllCollectionSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult);

    /// <summary>
    /// A compound operation that deletes all existing collection shard snapshots.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<DefaultOperationResponse> DeleteAllCollectionShardSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult);

    /// <summary>
    /// Returns a list of all snapshots for a shard from a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to get a snapshot list.</param>
    /// <param name="shardId">Id of the shard for which to list snapshots.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<ListSnapshotsResponse> ListShardSnapshots(
        string collectionName,
        uint shardId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Create new snapshot of a shard for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to create a snapshot.</param>
    /// <param name="shardId">Id of the shard for which to create a snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<CreateSnapshotResponse> CreateShardSnapshot(
        string collectionName,
        uint shardId,
        CancellationToken cancellationToken,
        bool isWaitForResult);

    /// <summary>
    /// Recover shard of a local collection data from a snapshot.
    /// This will overwrite any data, stored in this shard, for the collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="shardId">Id of the shard which to recover from a snapshot.</param>
    /// <param name="snapshotLocationUri">The snapshot location in URI format. File scheme uris are not supported by qdrant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverShardFromSnapshot(
        string collectionName,
        uint shardId,
        Uri snapshotLocationUri,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        SnapshotPriority? snapshotPriority,
        string snapshotChecksum);

    /// <summary>
    /// Recover shard of a local collection from an uploaded snapshot.
    /// This will overwrite any data, stored on this node, for the collection shard.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="shardId">Id of the shard which to recover from a snapshot.</param>
    /// <param name="snapshotContent">The snapshot content stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverShardFromUploadedSnapshot(
        string collectionName,
        uint shardId,
        Stream snapshotContent,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        SnapshotPriority? snapshotPriority,
        string snapshotChecksum);

    /// <summary>
    /// Downloads the specified snapshot of a shard from a collection as a file stream.
    /// </summary>
    /// <param name="collectionName">Name of the collection to download snapshot for.</param>
    /// <param name="shardId">Id of the shard for which to download snapshot.</param>
    /// <param name="snapshotName">Name of the snapshot to download.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<DownloadSnapshotResponse> DownloadShardSnapshot(
        string collectionName,
        uint shardId,
        string snapshotName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified snapshot of a shard from a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete shard snapshot for.</param>
    /// <param name="shardId">Id of the shard for which to delete snapshot.</param>
    /// <param name="snapshotName">Name of the snapshot to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<DefaultOperationResponse> DeleteShardSnapshot(
        string collectionName,
        uint shardId,
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult);

    /// <summary>
    /// Returns a list of all snapshots for the entire storage.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<ListSnapshotsResponse> ListStorageSnapshots(
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new snapshot of the entire storage.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<CreateSnapshotResponse> CreateStorageSnapshot(
        CancellationToken cancellationToken,
        bool isWaitForResult);

    /// <summary>
    /// Download specified snapshot of the whole storage as a file stream.
    /// </summary>
    /// <param name="snapshotName">Name of the snapshot to download.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    /// Full storage snapshot is a .tar file with each collection having its own snapshot inside.
    /// Alongside it the config.json maps snapshots to collections.
    /// </remarks>
    Task<DownloadSnapshotResponse> DownloadStorageSnapshot(
        string snapshotName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Delete snapshot of the whole storage.
    /// </summary>
    /// <param name="snapshotName">Name of the snapshot to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<DefaultOperationResponse> DeleteStorageSnapshot(
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult);

    /// <summary>
    /// Recover the whole storage data from snapshot by its name. This will overwrite any data, stored on
    /// this node, for the collection. If collection does not exist - it will be created.
    /// The snapshot path should be <c>/qdrant/snapshots/{snapshotName}</c> on the Qdrant node.
    /// </summary>
    /// <param name="snapshotName">The name of the local snapshot file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverStorageFromSnapshot(
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        SnapshotPriority? snapshotPriority,
        string snapshotChecksum);

    /// <summary>
    /// Recover the whole storage data from a possibly remote snapshot.
    /// </summary>
    /// <param name="snapshotLocationUri">The snapshot location in URI format. Can be either a URL or a <c>file:///</c> path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverStorageFromSnapshot(
        Uri snapshotLocationUri,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        SnapshotPriority? snapshotPriority,
        string snapshotChecksum);

    /// <summary>
    /// Recover the whole storage from an uploaded snapshot.
    /// </summary>
    /// <param name="snapshotContent">The snapshot content stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverStorageFromUploadedSnapshot(
        Stream snapshotContent,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        SnapshotPriority? snapshotPriority,
        string snapshotChecksum);

    /// <summary>
    /// Asynchronously wait until the collection status becomes <see cref="QdrantCollectionStatus.Green"/>
    /// and collection optimizer status becomes <see cref="QdrantOptimizerStatus.Ok"/>.
    /// </summary>
    /// <param name="collectionName">The name of the collection to check status for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="pollingInterval">The collection status polling interval. Is not set the default polling interval is 1 second.</param>
    /// <param name="timeout">The timeout after which the collection considered not green and exception is thrown. The default timeout is 30 seconds.</param>
    /// <param name="requiredNumberOfGreenCollectionResponses">The number of green status responses to be received
    /// for collection status to be considered green. To increase the probability that every node has
    /// the same green status - set this parameter to a value greater than the number of nodes.</param>
    /// <param name="isCheckShardTransfersCompleted">
    /// If set to <c>true</c> check that all collection shard transfers are completed.
    /// The collection is not considered ready until all shard transfers are completed.
    /// </param>
    Task EnsureCollectionReady(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? pollingInterval,
        TimeSpan? timeout,
        uint requiredNumberOfGreenCollectionResponses,
        bool isCheckShardTransfersCompleted);

}
