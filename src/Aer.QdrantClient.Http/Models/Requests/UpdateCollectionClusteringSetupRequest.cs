// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests;

/// <summary>
/// Represents the request to update collection clustering (sharding) information.
/// </summary>
public abstract class UpdateCollectionClusteringSetupRequest
{
    #region Nested classes

    /// <summary>
    /// Represents a request to move shard form peer to peer.
    /// </summary>
    internal class MoveShardRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// The move shard operation description.
        /// </summary>
        public required ShardOperationDescription MoveShard { set; get; }
    }

    /// <summary>
    /// Represents a request to replicate shard form peer to peer.
    /// </summary>
    internal class ReplicateShardRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// The replicate shard operation description.
        /// </summary>
        public required ShardOperationDescription ReplicateShard { set; get; }
    }

    /// <summary>
    /// Represents a request to abort an ongoing shard transfer process.
    /// </summary>
    internal class AbortShardTransferRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// The abort shard replication or transfer operation description.
        /// </summary>
        public required ShardOperationDescription AbortTransfer { set; get; }
    }

    /// <summary>
    /// Represents a request to drop an existing shard replica.
    /// </summary>
    internal class DropShardReplicaRequest : UpdateCollectionClusteringSetupRequest
    {
        /// <summary>
        /// Represnets a description of a replica to drop.
        /// </summary>
        public DropShardReplicaDescriptor DropReplica { set; get; }

        /// <summary>
        /// Represnets a description of a replica to drop.
        /// </summary>
        public class DropShardReplicaDescriptor
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
    /// Represents a descriptor for the shard operation to perform.
    /// </summary>
    internal class ShardOperationDescription
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
    }

    #endregion

    /// <summary>
    /// Returns the move shard operation request.
    /// </summary>
    /// <param name="shardId">The identifier of the shard to move.</param>
    /// <param name="fromPeerId">Source peer identifier.</param>
    /// <param name="toPeerId">Target peer identifier.</param>
    public static UpdateCollectionClusteringSetupRequest CreateMoveShardRequest(uint shardId, ulong fromPeerId, ulong toPeerId)
    {
        if (fromPeerId == toPeerId)
        {
            // looks like an attempt to move collection shard from peer to itself causes qdrant to halt
            throw new InvalidOperationException($"Can't move collection shard {shardId} from peer {fromPeerId} to itself.");
        }

        return new MoveShardRequest()
        {
            MoveShard = new ShardOperationDescription()
            {
                ShardId = shardId,
                FromPeerId = fromPeerId,
                ToPeerId = toPeerId
            }
        };
    }

    /// <summary>
    /// Returns the replicate shard operation request.
    /// </summary>
    /// <param name="shardId">The identifier of the shard to replicate.</param>
    /// <param name="fromPeerId">Source peer identifier.</param>
    /// <param name="toPeerId">Target peer identifier.</param>
    public static UpdateCollectionClusteringSetupRequest CreateReplicateShardRequest(
        uint shardId,
        ulong fromPeerId,
        ulong toPeerId)
    {
        if (fromPeerId == toPeerId)
        {
            // looks like an attempt to move collection shard from peer to itself causes qdrant to halt
            throw new InvalidOperationException($"Can't replicate collection shard {shardId} from peer {fromPeerId} to itself.");
        }

        return new ReplicateShardRequest()
        {
            ReplicateShard = new ShardOperationDescription()
            {
                ShardId = shardId,
                FromPeerId = fromPeerId,
                ToPeerId = toPeerId
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
    /// Returns the drop shard replica operation request.
    /// </summary>
    /// <param name="shardId">The identifier of the shard to drop.</param>
    /// <param name="peerId">Thw peer identifier to drop shard on.</param>
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
}
