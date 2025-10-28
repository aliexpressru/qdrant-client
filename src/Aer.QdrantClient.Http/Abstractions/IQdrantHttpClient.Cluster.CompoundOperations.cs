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

}
