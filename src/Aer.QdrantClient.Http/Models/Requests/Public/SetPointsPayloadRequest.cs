using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the request to set payload to each point by its id or by filter match.
/// </summary>
/// <typeparam name="TPayload">The type of the point payload.</typeparam>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class SetPointsPayloadRequest<TPayload>
    where TPayload : class
{
    /// <summary>
    /// The point payload.
    /// </summary>
    public TPayload Payload { get; }

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
    /// Initializes a new instance of <see cref="SetPointsPayloadRequest{TPayload}"/> with payload
    /// and point ids to set payload for.
    /// </summary>
    /// <param name="payload">Payload to set.</param>
    /// <param name="pointsToSetPayloadFor">Point ids to set payload for.</param>
    public SetPointsPayloadRequest(TPayload payload, IEnumerable<PointId> pointsToSetPayloadFor)
    {
        Payload = payload;
        Points = pointsToSetPayloadFor;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SetPointsPayloadRequest{TPayload}"/> with payload
    /// and point filter to set payload for.
    /// </summary>
    /// <param name="payload">Payload to set.</param>
    /// <param name="pointsFilterToSetPayloadFor">Points filter to set payload for.</param>
    public SetPointsPayloadRequest(TPayload payload, QdrantFilter pointsFilterToSetPayloadFor)
    {
        Payload = payload;
        Filter = pointsFilterToSetPayloadFor;
    }
}
