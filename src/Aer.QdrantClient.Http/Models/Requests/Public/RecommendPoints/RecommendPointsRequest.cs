using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the recommend points request.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class RecommendPointsRequest
{
    /// <summary>
    /// The vectors to recommend closest to.
    /// </summary>
    [JsonConverter(typeof(PointIdOrQueryVectorCollectionJsonConverter))]
    public ICollection<PointIdOrQueryVector> Positive { get; }

    /// <summary>
    /// The vectors to avoid proximity to.
    /// </summary>
    [JsonConverter(typeof(PointIdOrQueryVectorCollectionJsonConverter))]
    public ICollection<PointIdOrQueryVector> Negative { get; }

    /// <summary>
    /// How to use positive and negative examples to find the results.
    /// </summary>
    public RecommendStrategy? Strategy { set; get; }

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
    /// Max number of results to return.
    /// </summary>
    public uint Limit { get; }

    /// <summary>
    /// Offset of the first result to return. May be used to paginate results.
    /// Large offset values may cause performance issues.
    /// </summary>
    public uint Offset { get; set; } = 0;

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
    /// Define a minimal score threshold for the result.
    /// If defined, less similar results will not be returned.
    /// Score of the returned result might be higher or smaller than the
    /// threshold depending on the Distance function used. E.g. for
    /// cosine similarity only higher scores will be returned.
    /// </summary>
    public float? ScoreThreshold { get; set; }

    /// <summary>
    /// Name of the vector that should be used to calculate recommendations.
    /// Only for collections with multiple named vectors.
    /// If not provided, the default vector field will be used.
    /// </summary>
    public string Using { get; set; }

    /// <summary>
    /// The location used to lookup vectors. If not specified - use current collection.
    /// </summary>
    /// <remarks>The other collection should have the same vector size as the current collection.</remarks>
    public VectorsLookupLocation LookupFrom { set; get; }

    /// <summary>
    /// The shard selector to perform operation only on specified shards.
    /// If not set - perform operation on all shards.
    /// </summary>
    [JsonConverter(typeof(ShardSelectorJsonConverter))]
    public ShardSelector ShardKey { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecommendPointsRequest"/> class with vector examples.
    /// </summary>
    /// <param name="positiveVectorExamples">Recommend points closest to specified vectors.</param>
    /// <param name="limit">Maximal number of points to return.</param>
    /// <param name="negativeVectorExamples">Optional vectors to avoid similarity with.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public RecommendPointsRequest(
        ICollection<PointIdOrQueryVector> positiveVectorExamples,
        uint limit,
        ICollection<PointIdOrQueryVector> negativeVectorExamples = null,
        VectorSelector withVector = null,
        PayloadPropertiesSelector withPayload = null,
        ShardSelector shardSelector = null)
    {
        Positive = positiveVectorExamples;
        Limit = limit;
        Negative = negativeVectorExamples;
        WithVector = withVector;
        WithPayload = withPayload;
        ShardKey = shardSelector;
    }
}
