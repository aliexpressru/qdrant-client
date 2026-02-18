using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents a result of replicating collections to a cluster node.
/// Note that replicate collections operation is asynchronous and success result means
/// that there were no errors during operation start.
/// </summary>
public sealed class ReplicateShardsToPeerResponse : QdrantResponseBase<ReplicateShardsToPeerResponse.ReplicateShardsToPeerResponseUnit>
{
    /// <summary>
    /// Creates a new instance of <see cref="ReplicateShardsToPeerResponse"/>.
    /// </summary>
    public ReplicateShardsToPeerResponse()
    { }

    internal ReplicateShardsToPeerResponse(QdrantResponseBase childResponse) : base(childResponse)
    { }

    /// <summary>
    /// Represents the information if the operation is successful or not and returns a map of shards per peer for which the operation was not required to execute.
    /// </summary>
    public sealed record ReplicateShardsToPeerResponseUnit(bool IsSuccess, Dictionary<ulong, HashSet<uint>> AlreadyReplicatedShardsByPeers)
    {
        /// <summary>
        /// Initializes a new ReplicateShardsToPeerResponseUnit instance with empty UntouchedPeerShards
        /// </summary>
        /// <param name="IsSuccess">Returns if the operation is successful.</param>
        public ReplicateShardsToPeerResponseUnit(bool IsSuccess) : this(IsSuccess, [])
        {
        }
    }
}
