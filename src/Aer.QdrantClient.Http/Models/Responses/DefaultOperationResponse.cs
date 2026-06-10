using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Shared;

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

    internal static DefaultOperationResponse Fail(
        QdrantStatus status,
        double time
    ) =>
        new()
        {
            Result = false,
            Status = status,
            Time = time,
        };
}
