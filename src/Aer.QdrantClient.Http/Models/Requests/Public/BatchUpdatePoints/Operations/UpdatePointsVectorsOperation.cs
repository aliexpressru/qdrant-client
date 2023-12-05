namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the update points vectors operation.
/// </summary>
internal sealed class UpdatePointsVectorsOperation : BatchUpdatePointsOperationBase
{
    /// <summary>
    /// Update points vectors request.
    /// </summary>
    public required UpdatePointsVectorsRequest UpdateVectors { set; get; }
}
