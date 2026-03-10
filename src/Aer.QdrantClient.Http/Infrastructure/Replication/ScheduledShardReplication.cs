namespace Aer.QdrantClient.Http.Infrastructure.Replication;

/// <summary>
/// Represents a shard replication operation scheduled to be executed.
/// </summary>
/// <param name="ShardId">The id of the shard to replicate.</param>
/// <param name="SourcePeerId">The source peer id to replicate shard from.</param>
/// <param name="SourcePeerUri">The source peer uri to replicate shard from.</param>
/// <param name="TargetPeerId">The target peer id to replicate shard to.</param>
/// <param name="TargetPeerUri">The target peer uri to replicate shard to.</param>
public record ScheduledShardReplication(
    uint ShardId,
    ulong SourcePeerId,
    string SourcePeerUri,
    ulong TargetPeerId,
    string TargetPeerUri
);
