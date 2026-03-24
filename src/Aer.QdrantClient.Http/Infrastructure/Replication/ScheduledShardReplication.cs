using static Aer.QdrantClient.Http.Infrastructure.Replication.ScheduledShardReplication;

namespace Aer.QdrantClient.Http.Infrastructure.Replication;

/// <summary>
/// Represents a shard replication operation scheduled to be executed.
/// </summary>
/// <param name="ShardId">The id of the shard to replicate.</param>
/// <param name="SourcePeerId">The source peer id to replicate shard from.</param>
/// <param name="SourcePeerUri">The source peer uri to replicate shard from.</param>
/// <param name="TargetPeerId">
/// The target peer id to replicate shard to.
/// If the shard is scheduled for deletion from the <paramref name="SourcePeerId"/> peer,
/// this property will be <c>null</c>.
/// </param>
/// <param name="TargetPeerUri">
/// The target peer uri to replicate shard to.
/// If the shard is scheduled for deletion from the <paramref name="SourcePeerId"/> peer,
/// this property will be <c>null</c>.
/// </param>
/// <param name="Action">The planned replicator action.</param>
/// <param name="StepNumber">The number of this replication operation in overall replication plan.</param>
public record ScheduledShardReplication(
    uint ShardId,
    ulong SourcePeerId,
    string SourcePeerUri,
    ulong? TargetPeerId,
    string TargetPeerUri,
    ReplicatorAction Action,
    int StepNumber
)
{
    internal CollectionClusteringState ExpectedInitialState { get; set; }

    /// <summary>
    /// The action that the replicator will perform on selected shard.
    /// </summary>
    public enum ReplicatorAction
    {
        /// <summary>
        /// Replicate shard from one peer to another.
        /// </summary>
        AddReplica,

        /// <summary>
        /// Delete shard replica from a peer.
        /// </summary>
        DropReplica,

        /// <summary>
        /// Move shard replica from one peer to another.
        /// </summary>
        MoveReplica
    }
}
