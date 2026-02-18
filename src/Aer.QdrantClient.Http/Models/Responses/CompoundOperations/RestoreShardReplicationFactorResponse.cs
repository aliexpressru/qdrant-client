using Aer.QdrantClient.Http.Infrastructure.Replication;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents a result of restoring shard replication factor.
/// Note that successful response status does not indicate that
/// the replications actually took place. It only
/// indicates that all the preparation work had been done.
/// Use <see cref="ShardReplicator"/> from the result property to execute required replication sequence.
/// </summary>
public sealed class RestoreShardReplicationFactorResponse : QdrantResponseBase<ShardReplicator>
{
    /// <summary>
    /// Creates a new instance of <see cref="ReplicateShardsToPeerResponse"/>.
    /// </summary>
    public RestoreShardReplicationFactorResponse()
    { }

    internal RestoreShardReplicationFactorResponse(QdrantResponseBase childResponse) : base(childResponse)
    { }
}
