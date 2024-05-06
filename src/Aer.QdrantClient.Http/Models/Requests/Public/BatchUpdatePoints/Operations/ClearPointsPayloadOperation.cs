using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the points payload clear operation.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class ClearPointsPayloadOperation : BatchUpdatePointsOperationBase
{
    /// <summary>
    /// Clear points payload request.
    /// </summary>
    public required ClearPointsPayloadRequest ClearPayload { set; get; }
}
