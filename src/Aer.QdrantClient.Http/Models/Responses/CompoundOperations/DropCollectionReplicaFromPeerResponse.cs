using Aer.QdrantClient.Http.Models.Responses.Base;
using static Aer.QdrantClient.Http.Models.Responses.DropCollectionReplicaFromPeerResponse;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents a result of a drop collection replica from peer operation.
/// Note that drop collection replica operation is asynchronous and success result means
/// that there were no errors during operation start.
/// </summary>
public sealed class DropCollectionReplicaFromPeerResponse : QdrantResponseBase<DropCollectionReplicaFromPeerResponseUnit>
{
    /// <summary>
    /// Creates a new instance of <see cref="DropCollectionReplicaFromPeerResponse"/>.
    /// </summary>
    public DropCollectionReplicaFromPeerResponse()
    { }

    internal DropCollectionReplicaFromPeerResponse(QdrantResponseBase childResponse) : base(childResponse)
    { }

    /// <summary>
    /// Represents an information about shards for which drop operations were successfully started.
    /// </summary>
    public sealed record DropCollectionReplicaFromPeerResponseUnit(bool IsSuccess, uint[] DroppedShardIds);
}
