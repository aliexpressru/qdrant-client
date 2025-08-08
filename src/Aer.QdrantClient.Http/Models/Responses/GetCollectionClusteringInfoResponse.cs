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
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class GetCollectionClusteringInfoResponse : QdrantResponseBase<GetCollectionClusteringInfoResponse.CollectionClusteringInfo>
{
    /// <summary>
    /// Represents information about the collection sharding.
    /// </summary>
    public class CollectionClusteringInfo
    {
        /// <summary>
        /// ID of this peer.
        /// </summary>
        public ulong PeerId { set; get; }
        
        /// <summary>
        /// The uri of this peer.
        /// </summary>
        public string PeerUri { set; get; }

        /// <summary>
        /// Total number of shards.
        /// </summary>
        public uint ShardCount { set; get; }

        /// <summary>
        /// Number of shards in the <see cref="ShardState.Partial"/> state.
        /// This state indicates node still being replicated.
        /// </summary>
        public uint PartialShardCount =>
            (LocalShards is {Length: > 0} localShards
                ? (uint) localShards.Sum(s => s.State == ShardState.Partial
                    ? 1
                    : 0)
                : 0U)
            +
            (RemoteShards is {Length: > 0} remoteShards
                ? (uint) remoteShards.Sum(s => s.State == ShardState.Partial
                    ? 1
                    : 0)
                : 0U);

        /// <summary>
        /// Number of shards in the <see cref="ShardState.Dead"/> state.
        /// </summary>
        public uint DeadShardCount =>
            (LocalShards is {Length: > 0} localShards
                ? (uint) localShards.Sum(s => s.State == ShardState.Dead
                    ? 1
                    : 0)
                : 0U)
            +
            (RemoteShards is {Length: > 0} remoteShards
                ? (uint) remoteShards.Sum(s => s.State == ShardState.Dead
                    ? 1
                    : 0)
                : 0U);

        /// <summary>
        /// Local shard information.
        /// </summary>
        public LocalShardInfo[] LocalShards { set; get; }

        /// <summary>
        /// Remote shard information.
        /// </summary>
        public RemoteShardInfo[] RemoteShards { set; get; }

        /// <summary>
        /// Ongoing shard transfers operations.
        /// </summary>
        public ShardTransferInfo[] ShardTransfers { set; get; }
        
        /// <summary>
        /// Ongoing resharding operations.
        /// </summary>
        public ReshardingOperationInfo[] ReshardingOperations { set; get; }
    }

    /// <summary>
    /// Information about a single shard.
    /// </summary>
    public class LocalShardInfo
    {
        /// <summary>
        /// Local shard identifier.
        /// </summary>
        public uint ShardId { set; get; }

        /// <summary>
        /// Number of points in the shard.
        /// </summary>
        public ulong PointsCount { set; get; }

        /// <summary>
        /// State of the single shard within a replica set.
        /// </summary>
        public ShardState State { set; get; }
    }

    /// <summary>
    /// Information about a single remote shard.
    /// </summary>
    public class RemoteShardInfo
    {
        /// <summary>
        /// Remote shard identifier.
        /// </summary>
        public uint ShardId { set; get; }

        /// <summary>
        /// Peer identifier.
        /// </summary>
        public ulong PeerId { set; get; }

        /// <summary>
        /// Peer uri.
        /// </summary>
        public string PeerUri { set; get; }

        /// <summary>
        /// State of the single shard within a replica set.
        /// </summary>
        public ShardState State { set; get; }
    }

    /// <summary>
    /// Represents information about ongoing shard transfer operation.
    /// </summary>
    public class ShardTransferInfo
    {
        /// <summary>
        /// The transferring shard identifier.
        /// </summary>
        public uint ShardId { set; get; }

        /// <summary>
        /// The peer id that the shard is being transferred from.
        /// </summary>
        public ulong From { set; get; }

        /// <summary>
        /// The peer id that the shard is being transferred to.
        /// </summary>
        public ulong To { set; get; }

        /// <summary>
        /// If <c>true</c> transfer is a synchronization of a replicas.
        /// If <c>false</c> transfer is a moving of a shard from one peer to another.
        /// </summary>
        public bool Sync { set; get; }
    }
    
    /// <summary>
    /// Represents a resharding operation information. 
    /// </summary>
    public class ReshardingOperationInfo
    { 
        /// <summary>
        /// Resharding direction, scale up or down in number of shards.
        /// </summary>
        public ReshardingOperationDirection Direction { set; get; }
        
        /// <summary>
        /// The id of the shards being added or removed.
        /// </summary>
        public uint ShardId { set; get; }
        
        /// <summary>
        /// The peer id that the shard is being added or removed from.
        /// </summary>
        public uint PeerId { set; get; }
        
        /// <summary>
        /// The peer uri that the shard is being added or removed from.
        /// </summary>
        public string PeerUri { set; get; }

        /// <summary>
        /// The shard key for the resharding operation.
        /// </summary>
        [JsonConverter(typeof(ShardKeyJsonConverter))]
        public ShardKey ShardKey { set; get; }
    }
}
