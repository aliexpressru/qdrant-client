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
