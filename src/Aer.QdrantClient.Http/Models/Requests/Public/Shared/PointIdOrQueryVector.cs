using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents a point id or a search vector - dense or sparse.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class PointIdOrQueryVector
{
    /// <summary>
    /// The point id.
    /// </summary>
    [JsonConverter(typeof(PointIdJsonConverter))]
    internal PointId PointId { get; }

    /// <summary>
    /// The search vector.
    /// </summary>
    [JsonConverter(typeof(QueryVectorJsonConverter))]
    internal QueryVector QueryVector { get; }

    /// <summary>
    /// Private ctor to enforce only implicit conversions.
    /// </summary>
    private PointIdOrQueryVector()
    { }

    /// <summary>
    /// Initializes a new instance of <see cref="PointIdOrQueryVector"/> with point id.
    /// </summary>
    /// <param name="pointId">The point id.</param>
    private PointIdOrQueryVector(PointId pointId)
    {
        PointId = pointId;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="PointIdOrQueryVector"/> with search vector.
    /// </summary>
    /// <param name="queryVector">The search vector.</param>
    private PointIdOrQueryVector(QueryVector queryVector)
    {
        QueryVector = queryVector;
    }

    /// <summary>
    /// Implicitly converts <see cref="PointId"/> to <see cref="PointIdOrQueryVector"/>.
    /// </summary>
    /// <param name="pointId">The point id to convert.</param>
    public static implicit operator PointIdOrQueryVector(PointId pointId) => new(pointId);

    /// <summary>
    /// Implicitly converts <see cref="QueryVector"/> to <see cref="PointIdOrQueryVector"/>.
    /// </summary>
    /// <param name="queryVector">The query vector to convert.</param>
    public static implicit operator PointIdOrQueryVector(QueryVector queryVector) => new(queryVector);

    /// <summary>
    /// Implicitly converts <see cref="float"/> dense vector components array to <see cref="PointIdOrQueryVector"/>.
    /// </summary>
    /// <param name="vector">The vector components.</param>
    public static implicit operator PointIdOrQueryVector(float[] vector) => new(vector);

    /// <summary>
    /// Implicitly converts the sparse vector components array to <see cref="PointIdOrQueryVector"/>.
    /// </summary>
    /// <param name="sparseVectorComponents">The sparse vector components.</param>
    public static implicit operator PointIdOrQueryVector((uint[] Indices, float[] Values) sparseVectorComponents) =>
        new(sparseVectorComponents);

    /// <summary>
    /// Implicitly converts the <see cref="VectorBase"/> to <see cref="PointIdOrQueryVector"/>.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    public static implicit operator PointIdOrQueryVector(VectorBase vector) => new(vector);
}
