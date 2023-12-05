// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the delete points payload keys operation.
/// </summary>
internal sealed class DeletePointsPayloadKeysOperation : BatchUpdatePointsOperationBase
{
    /// <summary>
    /// Delete points payload keys request.
    /// </summary>
    public required DeletePointsPayloadKeysRequest DeletePayload { set; get; }
}
