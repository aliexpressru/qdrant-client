using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents a single Qdrant point.
/// </summary>
public class Point
{
    /// <summary>
    /// Gets the point identifier.
    /// </summary>
    [JsonConverter(typeof(PointIdJsonConverter))]
    public required PointId Id { get; init; }

    /// <summary>
    /// Gets the point vector.
    /// </summary>
    [JsonConverter(typeof(VectorJsonConverter))]
    public VectorBase Vector{ get; init; }

    /// <summary>
    /// Gets the point payload.
    /// If all payload properties were filtered out, will be empty.
    /// If payload was not requested at all, will be <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Check the <see cref="IsPayloadNullOrEmpty"/> property to determine whether the payload is null or empty.
    /// </remarks>
    [JsonConverter(typeof(PayloadJsonConverter))]
    public Payload Payload { get; init; }

    /// <summary>
    /// Gets the point shard key.
    /// </summary>
    [JsonConverter(typeof(ShardKeyJsonConverter))]
    public ShardKey ShardKey { get; init; }

    /// <summary>
    /// Order-by value. Represents an order of the point if order-by selector is used.
    /// </summary>
    public double OrderValue { get; init; }
    
    /// <summary>
    /// If set to <c>true</c>, indicates that the payload is either null or empty.
    /// </summary>
    public bool IsPayloadNullOrEmpty => Payload == null || Payload.IsEmpty;
}
