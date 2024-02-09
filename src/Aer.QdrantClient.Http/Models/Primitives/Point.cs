using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents a single Qdrant point.
/// </summary>
public class Point
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
    public required VectorBase Vector{ get; init; }

    /// <summary>
    /// Gets or sets the point payload.
    /// </summary>
    [JsonConverter(typeof(PayloadJsonConverter))]
    public Payload Payload { get; set; }

    /// <summary>
    /// Gets or sets the point shard key.
    /// </summary>
    [JsonConverter(typeof(ShardKeyJsonConverter))]
    public ShardKey ShardKey { get; set; }
}
