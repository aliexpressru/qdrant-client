using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents a single point with associated vector.
/// </summary>
public sealed class PointVector
{
    /// <summary>
    /// Gets or sets the point identifier.
    /// </summary>
    [JsonConverter(typeof(PointIdJsonConverter))]
    public required PointId Id { get; init; }

    /// <summary>
    /// Gets or sets the point vector.
    /// </summary>
    [JsonConverter(typeof(VectorJsonConverter))]
    public required VectorBase Vector { get; init; }
}
