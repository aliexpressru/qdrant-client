using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Http.Abstractions;

public partial interface IQdrantHttpClient
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
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<ReplicateShardsToPeerResponse> ReplicateShards(
        ulong sourcePeerId,
        ulong targetPeerId,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        bool isMoveShards = false,
        string[] collectionNamesToReplicate = null,
        uint[] shardIdsToReplicate = null,
        string clusterName = null);

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
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<ReplicateShardsToPeerResponse> ReplicateShards(
        string sourcePeerUriSelectorString,
        string targetPeerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        bool isMoveShards = false,
        string[] collectionNamesToReplicate = null,
        uint[] shardIdsToReplicate = null,
        string clusterName = null);

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
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    /// <param name="collectionNamesToReplicate">
    /// Collection names to replicate shards for.
    /// If <c>null</c> or empty - replicates all collection shards.
    /// </param>
    Task<ReplicateShardsToPeerResponse> ReplicateShardsToPeer(
        ulong targetPeerId,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        string clusterName = null,
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
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    /// <param name="collectionNamesToReplicate">
    /// Collection names to replicate shards for.
    /// If <c>null</c> or empty - replicates all collection shards.
    /// </param>
    Task<ReplicateShardsToPeerResponse> ReplicateShardsToPeer(
        string targetPeerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        string clusterName = null,
        params string[] collectionNamesToReplicate);

    /// <summary>
    /// Restores shard replica count to be no fewer than configured replication factor for a specified collection.
    /// The final result will be replicating shards until replication factor for each shard is restored.
    /// If shard has number of replicas greater than configured replication factor for collection - it will not be altered.
    /// </summary>
    /// <param name="collectionName">Collection name to balance shard replication for.</param>
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
    /// <param name="timeout">The timeout to wait for resharding operation initiation.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<ReplicateShardsToPeerResponse> RestoreShardReplicationFactor(
        string collectionName,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        TimeSpan? timeout = null,
        string clusterName = null);

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
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<ReplicateShardsToPeerResponse> EqualizeShardReplication(
        string[] collectionNamesToEqualize,
        string sourcePeerUriSelectorString,
        string emptyTargetPeerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        string clusterName = null);

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
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<ReplicateShardsToPeerResponse> EqualizeShardReplication(
        string[] collectionNamesToEqualize,
        ulong sourcePeerId,
        ulong emptyTargetPeerId,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        string clusterName = null);

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
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    /// <param name="collectionNamesToMove">
    /// Collection names to move shards for.
    /// If <c>null</c> or empty - moves all collection shards.
    /// </param>
    Task<DrainPeerResponse> DrainPeer(
        ulong peerId,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        string clusterName = null,
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
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    /// <param name="collectionNamesToMove">
    /// Collection names to move shards for.
    /// If <c>null</c> or empty - moves all collection shards.
    /// </param>
    Task<DrainPeerResponse> DrainPeer(
        string peerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        ShardTransferMethod shardTransferMethod = ShardTransferMethod.Snapshot,
        string clusterName = null,
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
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    /// <param name="collectionNamesToClear">
    /// Collection names to drop shards for.
    /// If <c>null</c> or empty - drops all collection shards.
    /// </param>
    Task<ClearPeerResponse> ClearPeer(
        ulong peerId,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        string clusterName = null,
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
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    /// <param name="collectionNamesToClear">
    /// Collection names to drop shards for.
    /// If <c>null</c> or empty - drops all collection shards.
    /// </param>
    Task<ClearPeerResponse> ClearPeer(
        string peerUriSelectorString,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        string clusterName = null,
        params string[] collectionNamesToClear
    );

    /// <summary>
    /// Drops the specified collection shards from a peer node in the cluster.
    /// </summary>
    /// <param name="collectionName">The name of the collection to drop shards for.</param>
    /// <param name="peerId">The peer id for the peer to drop collection shards from.</param>
    /// <param name="shardIds">An array of shard IDs to be dropped from the peer. Each ID must correspond to an existing shard in the
    /// collection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<DropCollectionReplicaFromPeerResponse> DropCollectionShardsFromPeer(
        string collectionName,
        ulong peerId,
        uint[] shardIds,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        string clusterName = null
    );

    /// <summary>
    /// Drops the specified collection shards from a peer node in the cluster.
    /// </summary>
    /// <param name="collectionName">The name of the collection to drop shards for.</param>
    /// <param name="peerUriSelectorString">The peer uri selector string for the peer to drop shards from.</param>
    /// <param name="shardIds">An array of shard IDs to be dropped from the peer. Each ID must correspond to an existing shard in the
    /// collection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="isDryRun">
    /// If set to <c>true</c>, this operation calculates and logs
    /// all shard movements without actually executing them.
    /// </param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<DropCollectionReplicaFromPeerResponse> DropCollectionShardsFromPeer(
        string collectionName,
        string peerUriSelectorString,
        uint[] shardIds,
        CancellationToken cancellationToken,
        ILogger logger = null,
        bool isDryRun = false,
        string clusterName = null
    );

    /// <summary>
    /// Checks whether the specified cluster node does not have any collection shards on it.
    /// </summary>
    /// <param name="peerId">The cluster node peer id for the peer to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<CheckIsPeerEmptyResponse> CheckIsPeerEmpty(
        ulong peerId,
        CancellationToken cancellationToken,
        string clusterName = null);

    /// <summary>
    /// Checks whether the specified cluster node does not have any collection shards on it.
    /// </summary>
    /// <param name="peerUriSelectorString">The cluster node uri selector for the peer to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<CheckIsPeerEmptyResponse> CheckIsPeerEmpty(
        string peerUriSelectorString,
        CancellationToken cancellationToken,
        string clusterName = null);

    /// <summary>
    /// Gets the peer information by the peer node uri substring or by peer id. Returns the found peer and other peers.
    /// </summary>
    /// <param name="peerUriSelectorString">Peer uri substring to get peer info for or <c>null</c> if using <paramref name="peerId"/>.</param>
    /// <param name="peerId">Cluster node peer id to get peer info for or <c>null</c> if using <paramref name="peerUriSelectorString"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    /// <exception cref="QdrantNoPeersFoundForUriSubstringException">Occurs when no nodes found for uri substring.</exception>
    /// <exception cref="QdrantMoreThanOnePeerFoundForUriSubstringException">Occurs when more than one node found for uri substring.</exception>
    Task<GetPeerResponse> GetPeerInfo(
        string peerUriSelectorString,
        ulong? peerId,
        CancellationToken cancellationToken,
        string clusterName = null);

    /// <summary>
    /// Gets the peer information by the peer node uri substring. Returns the found peer and other peers.
    /// </summary>
    /// <param name="peerUriSelectorString">Peer uri substring to get peer info for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    /// <exception cref="QdrantNoPeersFoundForUriSubstringException">Occurs when no nodes found for uri substring.</exception>
    /// <exception cref="QdrantMoreThanOnePeerFoundForUriSubstringException">Occurs when more than one node found for uri substring.</exception>
    Task<GetPeerResponse> GetPeerInfo(
        string peerUriSelectorString,
        CancellationToken cancellationToken,
        string clusterName = null);

    /// <summary>
    /// Gets the peer information by the peer node uri substring. Returns the found peer and other peers.
    /// </summary>
    /// <param name="peerId">Cluster node peer is to get peer info for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    /// <exception cref="QdrantNoPeersFoundForUriSubstringException">Occurs when no nodes found for uri substring.</exception>
    /// <exception cref="QdrantMoreThanOnePeerFoundForUriSubstringException">Occurs when more than one node found for uri substring.</exception>
    Task<GetPeerResponse> GetPeerInfo(
        ulong peerId,
        CancellationToken cancellationToken,
        string clusterName = null);

    /// <summary>
    /// Gets the peer information by the peer node uri substring. Returns the found peer and other peers.
    /// </summary>
    /// <param name="clusterNodeUriSubstring">Cluster node uri substring to get peer info for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="QdrantNoPeersFoundForUriSubstringException">Occurs when no nodes found for uri substring.</exception>
    /// <exception cref="QdrantMoreThanOnePeerFoundForUriSubstringException">Occurs when more than one node found for uri substring.</exception>
    [Obsolete($"Use one of the {nameof(GetPeerInfo)} methods.")]
    Task<GetPeerResponse>
        GetPeerInfoByUriSubstring(
        string clusterNodeUriSubstring,
        CancellationToken cancellationToken);
}
