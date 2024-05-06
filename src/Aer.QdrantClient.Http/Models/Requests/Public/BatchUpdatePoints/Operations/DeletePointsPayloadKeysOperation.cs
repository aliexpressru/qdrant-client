using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the delete points payload keys operation.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class DeletePointsPayloadKeysOperation : BatchUpdatePointsOperationBase
{
    /// <summary>
    /// Delete points payload keys request.
    /// </summary>
    public required DeletePointsPayloadKeysRequest DeletePayload { set; get; }
}
