namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the "delete points vectors" operation.
/// </summary>
internal sealed class DeletePointsVectorsOperation : BatchUpdatePointsOperationBase
{
    /// <summary>
    /// Delete points vectors request.
    /// </summary>
    public required DeletePointsVectorsRequest DeleteVectors { set; get; }
}
