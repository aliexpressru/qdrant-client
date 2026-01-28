using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents a result of drain cluster node operation.
/// Note that drain node operation is asynchronous and success result means
/// that there were no errors during operation start.
/// </summary>
public sealed class DrainPeerResponse : QdrantResponseBase<bool>
{
    /// <summary>
    /// Creates a new instance of <see cref="DrainPeerResponse"/>.
    /// </summary>
    public DrainPeerResponse()
    { }

    internal DrainPeerResponse(QdrantResponseBase childResponse) : base(childResponse)
    { }
}
