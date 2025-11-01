using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the points payload overwrite request.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class OverwritePointsPayloadRequest
{
    /// <summary>
    /// The point payload.
    /// </summary>
    public object Payload { get; }

    /// <summary>
    /// Assigns payload to each point in this list.
    /// </summary>
    [JsonConverter(typeof(PointIdIEnumerableJsonConverter))]
    public IEnumerable<PointId> Points { get; }

    /// <summary>
    /// Assigns payload to each point that satisfy this filter condition.
    /// </summary>
    [JsonConverter(typeof(QdrantFilterJsonConverter))]
    public QdrantFilter Filter { get; }

    /// <summary>
    /// The shard selector to perform operation only on specified shards.
    /// If not set - perform operation on all shards.
    /// </summary>
    [JsonConverter(typeof(ShardSelectorJsonConverter))]
    public ShardSelector ShardKey { get; set; }

    /// <summary>
    /// Assigns payload to each point that satisfy this path of property.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="OverwritePointsPayloadRequest"/> with payload
    /// and point ids to overwrite payload for.
    /// </summary>
    /// <param name="payload">Payload to overwrite.</param>
    /// <param name="pointsToOverwritePayloadFor">Point ids to overwrite payload for.</param>
    /// <param name="nestedPayloadPropertyPath">Assigns payload to each point that satisfy this path of property.</param>
    public OverwritePointsPayloadRequest(
        object payload,
        IEnumerable<PointId> pointsToOverwritePayloadFor,
        string nestedPayloadPropertyPath = null)
    {
        Payload = payload;
        Points = pointsToOverwritePayloadFor;
        Key = nestedPayloadPropertyPath;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="OverwritePointsPayloadRequest"/> with payload
    /// and point filter to overwrite payload for.
    /// </summary>
    /// <param name="payload">Payload to overwrite.</param>
    /// <param name="pointsFilterToOverwritePayloadFor">Points filter to overwrite payload for.</param>
    /// <param name="nestedPayloadPropertyPath">Assigns payload to each point that satisfy this path of property.</param>
    public OverwritePointsPayloadRequest(
        object payload,
        QdrantFilter pointsFilterToOverwritePayloadFor,
        string nestedPayloadPropertyPath = null)
    {
        Payload = payload;
        Filter = pointsFilterToOverwritePayloadFor;
        Key = nestedPayloadPropertyPath;
    }
}
