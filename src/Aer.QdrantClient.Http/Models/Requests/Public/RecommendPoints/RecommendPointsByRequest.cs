using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the recommend points request.
/// </summary>
[JsonDerivedType(typeof(RecommendPointsByIdRequest))]
[JsonDerivedType(typeof(RecommendPointsByExampleRequest))]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public abstract class RecommendPointsByRequest
{
    #region Nested classes

    internal sealed class RecommendPointsByIdRequest : RecommendPointsByRequest
    {
        /// <summary>
        /// Look for vectors closest to those.
        /// </summary>
        [JsonConverter(typeof(PointIdCollectionJsonConverter))]
        public IEnumerable<PointId> Positive { get; }

        /// <summary>
        /// Try to avoid vectors like this.
        /// </summary>
        [JsonConverter(typeof(PointIdCollectionJsonConverter))]
        public IEnumerable<PointId> Negative { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="RecommendPointsByRequest.RecommendPointsByIdRequest"/>.
        /// </summary>
        /// <param name="positive">Recommend points closest to specified vectors.</param>
        /// <param name="negative">Optional vectors to avoid similarity with.</param>
        /// <param name="limit">Maximal number of points to return.</param>
        public RecommendPointsByIdRequest(
            IEnumerable<PointId> positive,
            IEnumerable<PointId> negative,
            uint limit) : base(limit)
        {
            Positive = positive;
            Negative = negative;
        }
    }

    internal sealed class RecommendPointsByExampleRequest : RecommendPointsByRequest
    {
        /// <summary>
        /// Look for vectors closest to those.
        /// </summary>
        public IEnumerable<float[]> Positive { get; }

        /// <summary>
        /// Try to avoid vectors like this.
        /// </summary>
        public IEnumerable<float[]> Negative { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="RecommendPointsByRequest.RecommendPointsByExampleRequest"/>.
        /// </summary>
        /// <param name="positive">Recommend points closest to specified vectors.</param>
        /// <param name="negative">Optional vectors to avoid similarity with.</param>
        /// <param name="limit">Maximal number of points to return.</param>
        public RecommendPointsByExampleRequest(
            IEnumerable<float[]> positive,
            IEnumerable<float[]> negative,
            uint limit) : base(limit)
        {
            Positive = positive;
            Negative = negative;
        }
    }

    #endregion

    /// <summary>
    /// How to use positive and negative examples to find the results.
    /// </summary>
    public RecommendStrategy Strategy { set; get; }

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
    /// Initializes a new instance of the <see cref="RecommendPointsByRequest"/> class.
    /// </summary>
    /// <param name="limit">Maximal number of recommended points to return.</param>
    protected internal RecommendPointsByRequest(uint limit)
    {
        Limit = limit;
    }
}
