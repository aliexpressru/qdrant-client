using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Requests;

/// <summary>
/// Represents the request to update collection clustering (sharding) information.
/// </summary>
[JsonDerivedType(typeof(MoveShardRequest))]
[JsonDerivedType(typeof(ReplicateShardRequest))]
[JsonDerivedType(typeof(ReplicatePointsRequest))]
[JsonDerivedType(typeof(AbortShardTransferRequest))]
[JsonDerivedType(typeof(DropShardReplicaRequest))]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public abstract class UpdateCollectionClusteringSetupRequest
{
    #region Nested classes

    /// <summary>
    /// Represents a request to move shard form peer to peer.
    /// </summary>
    internal sealed class MoveShardRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// The move shard operation description.
        /// </summary>
        public required ShardOperationDescription MoveShard { set; get; }
    }

    /// <summary>
    /// Represents a request to replicate shard form peer to peer.
    /// </summary>
    internal sealed class ReplicateShardRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// The replicate shard operation description.
        /// </summary>
        public required ShardOperationDescription ReplicateShard { set; get; }
    }

    /// <summary>
    /// Represents a request to replicate shard form peer to peer.
    /// </summary>
    internal sealed class ReplicatePointsRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// The replicate points from shard to shard operation description.
        /// </summary>
        public required ReplicatePointsOperationDescription ReplicatePoints { set; get; }
    }

    /// <summary>
    /// Represents a request to abort an ongoing shard transfer process.
    /// </summary>
    internal sealed class AbortShardTransferRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// The abort shard replication or transfer operation description.
        /// </summary>
        public required ShardOperationDescription AbortTransfer { set; get; }
    }

    /// <summary>
    /// Represents a request to restart an ongoing shard transfer process.
    /// </summary>
    internal sealed class RestartShardTransferRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// The restart shard replication or transfer operation description.
        /// </summary>
        public required ShardOperationDescription RestartTransfer { set; get; }
    }

    /// <summary>
    /// Represents a request to start a resharding operation.
    /// </summary>
    internal sealed class StartReshardingOperationRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// The start resharding operation description.
        /// </summary>
        public required ReshardingOperationDescription StartResharding { set; get; }
    }

    /// <summary>
    /// Represents a request to abort a resharding operation.
    /// </summary>
    internal sealed class AbortReshardingOperationRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// The abort resharding operation description.
        /// </summary>
        public object AbortResharding { set; get; } = new();
    }

    /// <summary>
    /// Represents a request to drop an existing shard replica.
    /// </summary>
    internal sealed class DropShardReplicaRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// Represents a description of a replica to drop.
        /// </summary>
        public DropShardReplicaDescriptor DropReplica { set; get; }

        /// <summary>
        /// Represents a description of a replica to drop.
        /// </summary>
        public sealed class DropShardReplicaDescriptor
        {
            /// <summary>
            /// The shard identifier of the replica to drop.
            /// </summary>
            public uint ShardId { set; get; }

            /// <summary>
            /// The peer identifier of the replica to drop.
            /// </summary>
            public ulong PeerId { set; get; }
        }
    }

    /// <summary>
    /// Represents a request to create a sharding key.
    /// </summary>
    internal sealed class CreateShardingKeyRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// A sharing key operation description to create a sharding key.
        /// </summary>
        public required ShardKeyOperationDescription CreateShardingKey { set; get; }

        /// <summary>
        /// Represents a descriptor for the shard key operation to perform.
        /// </summary>
        internal sealed class ShardKeyOperationDescription
        {
            /// <summary>
            /// The shard key to create.
            /// </summary>
            [JsonConverter(typeof(ShardKeyJsonConverter))]
            public required ShardKey ShardKey { set; get; }

            /// <summary>
            /// How many shards to create for this key. If not specified, will use the default value from config.
            /// </summary>
            public uint? ShardsNumber { set; get; }

            /// <summary>
            /// How many replicas to create for each shard. If not specified, will use the default value from config.
            /// </summary>
            public uint? ReplicationFactor { set; get; }

            /// <summary>
            /// Placement of shards for this key - array of peer ids, that can be used to place shards for this key.
            /// If not specified, will be randomly placed among all peers.
            /// </summary>
            public ulong[] Placement { set; get; }
        }
    }

    /// <summary>
    /// Represents a request to drop an existing a sharding key.
    /// </summary>
    internal sealed class DropShardingKeyRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// A sharing key operation description to drop a sharding key.
        /// </summary>
        public required DropShardKeyOperationDescription DropShardingKey { set; get; }

        internal sealed class DropShardKeyOperationDescription
        {
            /// <summary>
            /// The shard key to drop.
            /// </summary>
            [JsonConverter(typeof(ShardKeyJsonConverter))]
            public required ShardKey ShardKey { set; get; }
        }
    }

    /// <summary>
    /// Represents a descriptor for the shard operation to perform.
    /// </summary>
    internal sealed class ShardOperationDescription
    {
        /// <summary>
        /// The shard identifier.
        /// </summary>
        public required uint ShardId { init; get; }

        /// <summary>
        /// Source peer identifier.
        /// </summary>
        public required ulong FromPeerId { init; get; }

        /// <summary>
        /// Target peer identifier.
        /// </summary>
        public required ulong ToPeerId { init; get; }

        /// <summary>
        /// The shard transfer method. If not set, <see cref="ShardTransferMethod.StreamRecords"/> will be used.
        /// </summary>
        public ShardTransferMethod? Method { set; get; }
    }

    /// <summary>
    /// Represents a replicate points between shards operation to perform.
    /// </summary>
    internal sealed class ReplicatePointsOperationDescription
    {
        /// <summary>
        /// The source shard key for the operation.
        /// </summary>
        [JsonConverter(typeof(ShardKeyJsonConverter))]
        public required ShardKey FromShardKey { init; get; }

        /// <summary>
        /// The target shard key for the operation.
        /// </summary>
        [JsonConverter(typeof(ShardKeyJsonConverter))]
        public required ShardKey ToShardKey { init; get; }

        /// <summary>
        /// The filter to select the points to replicate.
        /// </summary>
        [JsonConverter(typeof(QdrantFilterJsonConverter))]
        public required QdrantFilter Filter { init; get; }
    }

    /// <summary>
    /// Represents a descriptor for the resharding operation to perform.
    /// </summary>
    internal sealed class ReshardingOperationDescription
    {
        /// <summary>
        /// Resharding direction, scale up or down in number of shards.
        /// </summary>
        public ReshardingOperationDirection Direction { set; get; }

        /// <summary>
        /// The peer id to perform resharding on.
        /// </summary>
        public ulong? PeerId { init; get; }

        /// <summary>
        /// The shard key for the resharding operation.
        /// </summary>
        [JsonConverter(typeof(ShardKeyJsonConverter))]
        public ShardKey ShardKey { set; get; }
    }

    #endregion

    /// <summary>
    /// Returns the move shard operation request.
    /// </summary>
    /// <param name="shardId">The identifier of the shard to move.</param>
    /// <param name="fromPeerId">Source peer identifier.</param>
    /// <param name="toPeerId">Target peer identifier.</param>
    /// <param name="shardTransferMethod">
    /// Method for transferring the shard from one node to another.
    /// If not set, <see cref="ShardTransferMethod.StreamRecords"/> will be used.
    /// </param>
    public static UpdateCollectionClusteringSetupRequest CreateMoveShardRequest(
        uint shardId,
        ulong fromPeerId,
        ulong toPeerId,
        ShardTransferMethod? shardTransferMethod = null)
    {
        if (fromPeerId == toPeerId)
        {
            // looks like an attempt to move collection shard from peer to itself causes qdrant to halt
            throw new InvalidOperationException(
                $"Can't move collection shard {shardId} from peer {fromPeerId} to itself.");
        }

        return new MoveShardRequest()
        {
            MoveShard = new ShardOperationDescription()
            {
                ShardId = shardId,
                FromPeerId = fromPeerId,
                ToPeerId = toPeerId,
                Method = shardTransferMethod
            }
        };
    }

    /// <summary>
    /// Returns the replicate shard operation request.
    /// </summary>
    /// <param name="shardId">The identifier of the shard to replicate.</param>
    /// <param name="fromPeerId">Source peer identifier.</param>
    /// <param name="toPeerId">Target peer identifier.</param>
    /// <param name="shardTransferMethod">Method for transferring the shard from one node to another.</param>
    public static UpdateCollectionClusteringSetupRequest CreateReplicateShardRequest(
        uint shardId,
        ulong fromPeerId,
        ulong toPeerId,
        ShardTransferMethod shardTransferMethod)
    {
        if (fromPeerId == toPeerId)
        {
            // looks like an attempt to move collection shard from peer to itself causes qdrant to halt
            throw new InvalidOperationException(
                $"Can't replicate collection shard {shardId} from peer {fromPeerId} to itself.");
        }

        return new ReplicateShardRequest()
        {
            ReplicateShard = new ShardOperationDescription()
            {
                ShardId = shardId,
                FromPeerId = fromPeerId,
                ToPeerId = toPeerId,
                Method = shardTransferMethod
            }
        };
    }

    /// <summary>
    /// Returns the replicate points from shard to shard operation request.
    /// </summary>
    /// <param name="fromShardKey">Source shard key to replicate points from.</param>
    /// <param name="toShardKey">Target shard key to replicate points to.</param>
    /// <param name="filter">Filter to select points to replicate from the <paramref name="fromShardKey"/>.</param>
    public static UpdateCollectionClusteringSetupRequest CreateReplicatePointsRequest(
        ShardKey fromShardKey,
        ShardKey toShardKey,
        QdrantFilter filter)
    {
        return new ReplicatePointsRequest()
        {
            ReplicatePoints = new ReplicatePointsOperationDescription()
            {
                FromShardKey = fromShardKey,
                ToShardKey = toShardKey,
                Filter = filter
            }
        };
    }

    /// <summary>
    /// Returns the abort shard transfer operation request.
    /// </summary>
    /// <param name="shardId">The identifier of the shard to abort transfer of.</param>
    /// <param name="fromPeerId">Source peer identifier.</param>
    /// <param name="toPeerId">Target peer identifier.</param>
    public static UpdateCollectionClusteringSetupRequest CreateAbortShardTransferRequest(
        uint shardId,
        ulong fromPeerId,
        ulong toPeerId)
        =>
            new AbortShardTransferRequest()
            {
                AbortTransfer = new ShardOperationDescription()
                {
                    ShardId = shardId,
                    FromPeerId = fromPeerId,
                    ToPeerId = toPeerId
                }
            };

    /// <summary>
    /// Returns the abort shard transfer operation request.
    /// </summary>
    /// <param name="shardId">The identifier of the shard to abort transfer of.</param>
    /// <param name="fromPeerId">Source peer identifier.</param>
    /// <param name="toPeerId">Target peer identifier.</param>
    /// <param name="shardTransferMethod">Method for transferring the shard from one node to another.</param>
    public static UpdateCollectionClusteringSetupRequest CreateRestartShardTransferRequest(
        uint shardId,
        ulong fromPeerId,
        ulong toPeerId,
        ShardTransferMethod shardTransferMethod)
        =>
            new RestartShardTransferRequest()
            {
                RestartTransfer = new ShardOperationDescription()
                {
                    ShardId = shardId,
                    FromPeerId = fromPeerId,
                    ToPeerId = toPeerId,
                    Method = shardTransferMethod
                }
            };

    /// <summary>
    /// Returns the drop shard replica operation request.
    /// </summary>
    /// <param name="shardId">The identifier of the shard to drop.</param>
    /// <param name="peerId">The peer identifier to drop shard on.</param>
    public static UpdateCollectionClusteringSetupRequest CreateDropShardReplicaRequest(
        uint shardId,
        ulong peerId)
        =>
            new DropShardReplicaRequest()
            {
                DropReplica = new DropShardReplicaRequest.DropShardReplicaDescriptor()
                {
                    ShardId = shardId,
                    PeerId = peerId,
                }
            };

    /// <summary>
    /// Returns the create sharding key operation request.
    /// </summary>
    /// <param name="shardKey">The shard key for shards to create.</param>
    /// <param name="shardsNumber">How many shards to create for this key. If not specified, will use the default value from config.</param>
    /// <param name="replicationFactor">How many replicas to create for each shard. If not specified, will use the default value from config.</param>
    /// <param name="placement">
    /// Placement of shards for this key - array of peer ids, that can be used to place shards for this key.
    /// If not specified, will be randomly placed among all peers.
    /// </param>
    public static UpdateCollectionClusteringSetupRequest CreateCreateShardingKeyRequest(
        ShardKey shardKey,
        uint? shardsNumber = null,
        uint? replicationFactor = null,
        ulong[] placement = null)
        =>
            new CreateShardingKeyRequest()
            {
                CreateShardingKey = new CreateShardingKeyRequest.ShardKeyOperationDescription()
                {
                    ShardKey = shardKey,
                    ShardsNumber = shardsNumber,
                    ReplicationFactor = replicationFactor,
                    Placement = placement
                }
            };

    /// <summary>
    /// Returns the drop sharding key operation request.
    /// </summary>
    /// <param name="shardKey">The shard key for shards to drop.</param>
    /// <returns></returns>
    public static UpdateCollectionClusteringSetupRequest CreateDropShardingKeyRequest(
        ShardKey shardKey)
        =>
            new DropShardingKeyRequest()
            {
                DropShardingKey = new DropShardingKeyRequest.DropShardKeyOperationDescription()
                {
                    ShardKey = shardKey
                }
            };
}
