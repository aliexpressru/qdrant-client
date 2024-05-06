using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the update points vectors operation.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class UpdatePointsVectorsOperation : BatchUpdatePointsOperationBase
{
    /// <summary>
    /// Update points vectors request.
    /// </summary>
    public required UpdatePointsVectorsRequest UpdateVectors { set; get; }
}
