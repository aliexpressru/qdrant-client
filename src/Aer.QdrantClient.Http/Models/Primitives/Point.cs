using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents a single Qdrant point.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
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
    public VectorBase Vector{ get; init; }

    /// <summary>
    /// Gets or sets the point payload.
    /// </summary>
    [JsonConverter(typeof(PayloadJsonConverter))]
    public Payload Payload { get; init; }

    /// <summary>
    /// Gets or sets the point shard key.
    /// </summary>
    [JsonConverter(typeof(ShardKeyJsonConverter))]
    public ShardKey ShardKey { get; init; }

    /// <summary>
    /// Order-by value. Represents an order of the point if order-by selector is used.
    /// </summary>
    public double OrderValue { get; init; }
}
