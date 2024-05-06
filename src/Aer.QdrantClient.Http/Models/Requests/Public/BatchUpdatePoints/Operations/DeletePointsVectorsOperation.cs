using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the "delete points vectors" operation.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class DeletePointsVectorsOperation : BatchUpdatePointsOperationBase
{
    /// <summary>
    /// Delete points vectors request.
    /// </summary>
    public required DeletePointsVectorsRequest DeleteVectors { set; get; }
}
