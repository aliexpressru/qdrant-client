using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;

/// <summary>
/// Represents a universal query API request.
/// </summary>
public class QueryPointsRequest
{
    /// <summary>
    /// The shard selector to perform operation only on specified shards.
    /// If not set - perform operation on all shards.
    /// </summary>
    [JsonConverter(typeof(ShardSelectorJsonConverter))]
    public ShardSelector ShardKey { get; set; }

    /// <summary>
    /// Sub-requests to perform first. If present, the query will be performed on the results of the prefetch(es).
    /// </summary>
    public List<PrefetchPoints> Prefetch { get; set; } = new();

    /// <summary>
    /// Query to perform. If missing without prefetches, returns points ordered by their IDs.
    /// </summary>
    public PointsQuery Query { get; set; }

    /// <summary>
    /// Define which vector name to use for querying. If missing, the default vector is used.
    /// </summary>
    public string Using { get; set; }

    /// <summary>
    /// Look only for points which satisfy the filter conditions.
    /// </summary>
    [JsonConverter(typeof(QdrantFilterJsonConverter))]
    public QdrantFilter Filter { get; set; }

    /// <summary>
    /// Additional search parameters.
    /// </summary>
    public VectorSearchParameters Params { get; set; }

    /// <summary>
    /// Define a minimal score threshold for the result.
    /// If defined, less similar results will not be returned.
    /// Score of the returned result might be higher or smaller than the
    /// threshold depending on the Distance function used. E.g. for
    /// cosine similarity only higher scores will be returned.
    /// </summary>
    public float? ScoreThreshold { get; set; }

    /// <summary>
    /// Max number of results to return.
    /// </summary>
    public uint Limit { get; set; } = 10;

    /// <summary>
    /// Offset of the first result to return. May be used to paginate results.
    /// Large offset values may cause performance issues.
    /// </summary>
    public uint Offset { get; set; } = 0;

    /// <summary>
    /// Whether the vector should be returned with the response.
    /// </summary>
    [JsonConverter(typeof(VectorSelectorJsonConverter))]
    public VectorSelector WithVector { get; set; }

    /// <summary>
    /// Whether the payload should be returned with the response.
    /// </summary>
    [JsonConverter(typeof(PayloadPropertiesSelectorJsonConverter))]
    public PayloadPropertiesSelector WithPayload { set; get; }

    /// <summary>
    /// The location used to lookup vectors. If not specified - use current collection.
    /// </summary>
    /// <remarks>The other collection should have the same vector size as the current collection.</remarks>
    public VectorsLookupLocation LookupFrom { set; get; }
}
