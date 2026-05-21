using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// The standard Qdrant async operation response.
/// </summary>
public class DefaultAsyncOperationResponse : QdrantResponseBase<QdrantOperationResult>
{
    /// <summary>
    /// Creates a new instance of <see cref="DefaultAsyncOperationResponse"/>.
    /// For serialization purposes.
    /// </summary>
    public DefaultAsyncOperationResponse()
    { }

    internal DefaultAsyncOperationResponse(QdrantResponseBase childResponse) : base(childResponse)
    { }
}
