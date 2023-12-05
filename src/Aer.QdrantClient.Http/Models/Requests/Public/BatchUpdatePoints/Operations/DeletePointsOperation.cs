// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the points delete operation.
/// </summary>
internal sealed class DeletePointsOperation : BatchUpdatePointsOperationBase
{
    /// <summary>
    /// Delete points request.
    /// </summary>
    public required DeletePointsRequest Delete { set; get; }
}
