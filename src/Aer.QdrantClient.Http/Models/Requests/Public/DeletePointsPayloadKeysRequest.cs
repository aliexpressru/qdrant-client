using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents a request to delete payload keys from point.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class DeletePointsPayloadKeysRequest
{
    /// <summary>
    /// List of payload keys to remove from payload.
    /// </summary>
    public IEnumerable<string> Keys { get; }

    /// <summary>
    /// Deletes values from each point in this list.
    /// </summary>
    [JsonConverter(typeof(PointIdIEnumerableJsonConverter))]
    public IEnumerable<PointId> Points { get; }

    /// <summary>
    /// Deletes values from points that satisfy this filter condition.
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
    /// Initializes a new instance of <see cref="OverwritePointsPayloadRequest"/> with payload
    /// and point ids to overwrite payload for.
    /// </summary>
    /// <param name="payloadKeysToDelete">Payload keys to delete.</param>
    /// <param name="pointsToDeletePayloadKeysFor">Point ids to delete payload keys for.</param>
    public DeletePointsPayloadKeysRequest(
        IEnumerable<string> payloadKeysToDelete,
        IEnumerable<PointId> pointsToDeletePayloadKeysFor)
    {
        Keys = payloadKeysToDelete;
        Points = pointsToDeletePayloadKeysFor;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="OverwritePointsPayloadRequest"/> with payload
    /// and point filter to overwrite payload for.
    /// </summary>
    /// <param name="payloadKeysToDelete">Payload keys to delete.</param>
    /// <param name="pointsFilterToDeletePayloadKeysFor">Points filter to delete payload for.</param>
    public DeletePointsPayloadKeysRequest(
        IEnumerable<string> payloadKeysToDelete,
        QdrantFilter pointsFilterToDeletePayloadKeysFor)
    {
        Keys = payloadKeysToDelete;
        Filter = pointsFilterToDeletePayloadKeysFor;
    }
}
