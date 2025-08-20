using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests;

/// <summary>
/// Represents the scroll points request.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ScrollPointsRequest
{
    /// <summary>
    /// Look only for points which satisfies this conditions. If not provided - all points.
    /// </summary>
    [JsonConverter(typeof(QdrantFilterJsonConverter))]
    public QdrantFilter Filter { get; set; }

    /// <summary>
    /// Whether the whole payload or only selected payload properties should be returned with the response.
    /// </summary>
    [JsonConverter(typeof(PayloadPropertiesSelectorJsonConverter))]
    public PayloadPropertiesSelector WithPayload { set; get; }

    /// <summary>
    /// Whether the vector, all named vectors or only selected named vectors should be returned with the response.
    /// </summary>
    [JsonConverter(typeof(VectorSelectorJsonConverter))]
    public VectorSelector WithVector { get; set; }

    /// <summary>
    /// Page size. Default: 10.
    /// </summary>
    public ulong? Limit { get; set; }

    /// <summary>
    /// Start ID to read points from.
    /// </summary>
    [JsonConverter(typeof(PointIdJsonConverter))]
    public PointId Offset { get; set; }

    /// <summary>
    /// The shard selector to perform operation only on specified shards.
    /// If not set - perform operation on all shards.
    /// </summary>
    [JsonConverter(typeof(ShardSelectorJsonConverter))]
    public ShardSelector ShardKey { get; set; }

    /// <summary>
    /// Order the records by a selected payload field.
    /// </summary>
    /// <remarks>When you use the <see cref="OrderBySelector"/> parameter, pagination is disabled.</remarks>
    public OrderBySelector OrderBy { get; set; }
}
