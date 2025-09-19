using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// The standard Qdrant operation response.
/// </summary>
public sealed class DefaultOperationResponse : QdrantResponseBase<bool?>
{
    /// <summary>
    /// Creates a new instance of <see cref="DefaultOperationResponse"/>.
    /// For serialization purposes.
    /// </summary>
    public DefaultOperationResponse()
    { }

    internal DefaultOperationResponse(QdrantResponseBase childResponse) : base(childResponse)
    { }
}
