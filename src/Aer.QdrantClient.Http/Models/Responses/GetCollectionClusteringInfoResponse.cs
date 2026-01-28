using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents current clustering information for the collection.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class GetCollectionClusteringInfoResponse
    : QdrantResponseBase<GetCollectionClusteringInfoResponse.CollectionClusteringInfo>
{
    /// <summary>
    /// Represents information about the collection sharding.
    /// </summary>
    public sealed class CollectionClusteringInfo
    {
        /// <summary>
        /// ID of this peer.
        /// </summary>
        public ulong PeerId { init; get; }

        /// <summary>
        /// The uri of this peer.
        /// </summary>
        public string PeerUri { set; get; }

        /// <summary>
        /// Total number of shards.
        /// </summary>
        public uint ShardCount { init; get; }

        /// <summary>
        /// Number of shards in the <see cref="ShardState.Partial"/> state.
        /// This state indicates node still being replicated.
        /// </summary>
        public uint PartialShardCount =>
            (LocalShards is { Length: > 0 } localShards
                ? (uint)localShards.Sum(s => s.State == ShardState.Partial
                    ? 1
                    : 0)
                : 0U)
            +
            (RemoteShards is { Length: > 0 } remoteShards
                ? (uint)remoteShards.Sum(s => s.State == ShardState.Partial
                    ? 1
                    : 0)
                : 0U);

        /// <summary>
        /// Number of shards in the <see cref="ShardState.Dead"/> state.
        /// </summary>
        public uint DeadShardCount =>
            (LocalShards is { Length: > 0 } localShards
                ? (uint)localShards.Sum(s => s.State == ShardState.Dead
                    ? 1
                    : 0)
                : 0U)
            +
            (RemoteShards is { Length: > 0 } remoteShards
                ? (uint)remoteShards.Sum(s => s.State == ShardState.Dead
                    ? 1
                    : 0)
                : 0U);

        /// <summary>
        /// Local shard information.
        /// </summary>
        public LocalShardInfo[] LocalShards { init; get; }

        /// <summary>
        /// Remote shard information.
        /// </summary>
        public RemoteShardInfo[] RemoteShards { init; get; }

        /// <summary>
        /// Ongoing shard transfers operations.
        /// </summary>
        public ShardTransferInfo[] ShardTransfers { init; get; }

        /// <summary>
        /// Ongoing resharding operations.
        /// </summary>
        public ReshardingOperationInfo[] ReshardingOperations { init; get; }

        /// <summary>
        /// Gets or sets the mapping of peer identifiers to the list of shard identifiers they are responsible for.
        /// </summary>
        public Dictionary<ulong, List<uint>> ShardsByPeers { set; get; }
    }

    /// <summary>
    /// Information about a single shard.
    /// </summary>
    public sealed class LocalShardInfo
    {
        /// <summary>
        /// Local shard identifier.
        /// </summary>
        public uint ShardId { init; get; }

        /// <summary>
        /// Number of points in the shard.
        /// </summary>
        public ulong PointsCount { init; get; }

        /// <summary>
        /// State of the single shard within a replica set.
        /// </summary>
        public ShardState State { init; get; }

        /// <summary>
        /// User-defined sharding key. If no user-defined sharding key created for this collection, this field is <c>null</c>.
        /// </summary>
        [JsonConverter(typeof(ShardKeyJsonConverter))]
        public ShardKey ShardKey { init; get; }
    }

    /// <summary>
    /// Information about a single remote shard.
    /// </summary>
    public sealed class RemoteShardInfo
    {
        /// <summary>
        /// Remote shard identifier.
        /// </summary>
        public uint ShardId { init; get; }

        /// <summary>
        /// Peer identifier.
        /// </summary>
        public ulong PeerId { init; get; }

        /// <summary>
        /// Peer uri.
        /// </summary>
        public string PeerUri { set; get; }

        /// <summary>
        /// State of the single shard within a replica set.
        /// </summary>
        public ShardState State { init; get; }

        /// <summary>
        /// User-defined sharding key. If no user-defined sharding key created for this collection, this field is <c>null</c>.
        /// </summary>
        [JsonConverter(typeof(ShardKeyJsonConverter))]
        public ShardKey ShardKey { init; get; }
    }

    /// <summary>
    /// Represents information about ongoing shard transfer operation.
    /// </summary>
    public sealed class ShardTransferInfo
    {
        /// <summary>
        /// The transferring shard identifier.
        /// </summary>
        public uint ShardId { init; get; }

        /// <summary>
        /// The peer id that the shard is being transferred from.
        /// </summary>
        public ulong From { init; get; }

        /// <summary>
        /// The peer id that the shard is being transferred to.
        /// </summary>
        public ulong To { init; get; }

        /// <summary>
        /// If <c>true</c> transfer is a synchronization of a replicas.
        /// If <c>false</c> transfer is a moving of a shard from one peer to another.
        /// </summary>
        public bool Sync { init; get; }
    }

    /// <summary>
    /// Represents a resharding operation information. 
    /// </summary>
    public sealed class ReshardingOperationInfo
    {
        /// <summary>
        /// Resharding direction, scale up or down in number of shards.
        /// </summary>
        public ReshardingOperationDirection Direction { init; get; }

        /// <summary>
        /// The id of the shards being added or removed.
        /// </summary>
        public uint ShardId { init; get; }

        /// <summary>
        /// The peer id that the shard is being added or removed from.
        /// </summary>
        public uint PeerId { init; get; }

        /// <summary>
        /// The peer uri that the shard is being added or removed from.
        /// </summary>
        public string PeerUri { set; get; }

        /// <summary>
        /// The shard key for the resharding operation.
        /// </summary>
        [JsonConverter(typeof(ShardKeyJsonConverter))]
        public ShardKey ShardKey { init; get; }
    }
}
