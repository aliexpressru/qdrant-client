using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Shared;
using static Aer.QdrantClient.Http.Models.Responses.ReplicateShardsToPeerResponse;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents a result of replicating collections to a cluster node.
/// Note that replicate collections operation is asynchronous and success result means
/// that there were no errors during operation start.
/// </summary>
public sealed class ReplicateShardsToPeerResponse : QdrantResponseBase<IReadOnlyList<ReplicateShardToPeerResult>>
{
    /// <summary>
    /// Represents the outcome of a shard replication operation to a peer.
    /// </summary>
    /// <param name="IsSuccess">
    /// A value indicating whether the replication operation was successful.
    /// If <c>false</c>, all other properties can be null.
    /// If <c>true</c>, all other properties are guaranteed to be not null.
    /// </param>
    /// <param name="ShardId">
    /// The identifier of the shard that was replicated.
    /// Can be <c>null</c> if <see cref="IsSuccess"/> is <c>false</c>.
    /// </param>
    /// <param name="SourcePeerId">
    /// The identifier of the source peer from which the shard was replicated.
    /// Can be <c>null</c> if <see cref="IsSuccess"/> is <c>false</c>.
    /// </param>
    /// <param name="TargetPeerId">
    /// The identifier of the target peer to which the shard was replicated.
    /// Can be <c>null</c> if <see cref="IsSuccess"/> is <c>false</c>.
    /// </param>
    /// <param name="CollectionName">The name of the replicated collection.</param>
    public record ReplicateShardToPeerResult(
        bool IsSuccess,
        uint? ShardId,
        ulong? SourcePeerId,
        ulong? TargetPeerId,
        string CollectionName
    )
    {
        internal static ReplicateShardToPeerResult Fail(
            uint? ShardId = null,
            ulong? SourcePeerId = null,
            ulong? TargetPeerId = null,
            string CollectionName = null
        ) => new(IsSuccess: false, ShardId: ShardId, SourcePeerId: SourcePeerId, TargetPeerId: TargetPeerId, CollectionName);
    }

    /// <summary>
    /// Creates a new instance of <see cref="ReplicateShardsToPeerResponse"/>.
    /// </summary>
    public ReplicateShardsToPeerResponse() { }

    internal ReplicateShardsToPeerResponse(QdrantResponseBase childResponse)
        : base(childResponse) { }

    internal static ReplicateShardsToPeerResponse Fail(
        QdrantStatus status,
        double time,
        uint? ShardId = null,
        ulong? SourcePeerId = null,
        ulong? TargetPeerId = null
    ) =>
        new()
        {
            Result = [ReplicateShardToPeerResult.Fail(ShardId, SourcePeerId, TargetPeerId)],
            Status = status,
            Time = time,
        };
}
