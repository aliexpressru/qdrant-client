namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the overwrite points payload operation.
/// </summary>
internal sealed class OverwritePointsPayloadOperation : BatchUpdatePointsOperationBase
{
    /// <summary>
    /// Overwrite points payload request.
    /// </summary>
    /// <remarks>
    /// We don't use generic request here due to System.Text.Json limitations.
    /// But this class is internal and never created by end user so we are relatively safe.
    /// </remarks>
    public required object OverwritePayload { set; get; }
}
