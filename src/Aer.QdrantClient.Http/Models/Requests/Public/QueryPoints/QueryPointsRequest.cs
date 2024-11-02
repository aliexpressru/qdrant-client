using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;

/// <summary>
/// Represents a universal query API request.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
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
    public ICollection<PrefetchPoints> Prefetch { get; set; }

    /// <summary>
    /// Query to perform. If missing without prefetches, returns points ordered by their IDs.
    /// </summary>
    public PointsQuery Query { get; }

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
    public uint Limit { get; set; }

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

    /// <summary>
    /// Initializes new instance of <see cref="QueryPointsRequest"/>
    /// </summary>
    /// <param name="query">The universal points query.</param>
    /// <param name="limit">Maximal number of nearest points to return.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public QueryPointsRequest(
        PointsQuery query,
        uint limit = 10,
        VectorSelector withVector = null,
        PayloadPropertiesSelector withPayload = null,
        ShardSelector shardSelector = null)
    {
        Query = query;
        Limit = limit;
        WithVector = withVector;
        WithPayload = withPayload;
        ShardKey = shardSelector;
    }
}
