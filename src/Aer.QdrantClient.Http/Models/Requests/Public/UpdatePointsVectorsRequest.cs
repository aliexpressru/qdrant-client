// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the request to update points vectors.
/// </summary>
public sealed class UpdatePointsVectorsRequest
{
    /// <summary>
    /// Points with updated vectors.
    /// </summary>
    public required PointVector[] Points { set; get; }
}
