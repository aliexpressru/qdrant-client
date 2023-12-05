using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the recommend points request.
/// </summary>
[JsonDerivedType(typeof(RecommendPointsByIdRequest))]
[JsonDerivedType(typeof(RecommendPointsByExampleRequest))]
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

        public RecommendPointsByExampleRequest(
            IEnumerable<float[]> positive,
            IEnumerable<float[]> negative,
            uint limit) : base(limit)
        {
            Positive = positive;
            Negative = negative;
        }
    }

    /// <summary>
    /// Represents the search parameters.
    /// </summary>
    public class SearchParameters
    {
        /// <summary>
        /// Params relevant to HNSW index. Size of the beam in a beam-search.
        /// Larger the value - more accurate the result, more time required for search.
        /// </summary>
        public uint? HnswEf { get; set; }

        /// <summary>
        /// Search without approximation. If set to true, search may run long but with exact results.
        /// </summary>
        public bool Exact { get; set; } = false;

        /// <summary>
        /// Quantization parameters.
        /// </summary>
        public QuantizationParameters Quantization { get; set; }

        /// <summary>
        /// If enabled, the engine will only perform search among indexed or small segments.
        /// Using this option prevents slow searches in case of delayed index,
        /// but does not guarantee that all uploaded vectors will be included in search results.
        /// </summary>
        public bool IndexedOnly { get; set; } = false;

        /// <summary>
        /// Represents quantization parameters.
        /// </summary>
        public class QuantizationParameters
        {
            /// <summary>
            /// If <c>true</c>, quantized vectors are ignored. Default is <c>false</c>.
            /// </summary>
            public bool Ignore { get; set; } = false;

            /// <summary>
            /// If <c>true</c>, use original vectors to re-score top-k results.
            /// Might require more time in case if original vectors are stored on disk. Default is <c>false</c>.
            /// </summary>
            public bool Rescore { get; set; } = false;

            /// <summary>
            /// Defines how many extra vectors should be pre-selected using quantized index,
            /// and then re-scored using original vectors.
            /// </summary>
            /// <remarks>
            /// For example, if oversampling is 2.4 and limit is 100, then 240 vectors will be pre-selected
            /// using quantized index, and then top-100 will be returned after re-scoring.
            /// </remarks>
            public double? Oversampling { get; set; }
        }
    }

    /// <summary>
    /// Reresents the location used to lookup vectors.
    /// </summary>
    public class LookupLocation
    {
        /// <summary>
        /// The name of the collection to lookup vectors in.
        /// </summary>
        public string Collection { set; get; }

        /// <summary>
        /// Optional name of the vector field within the collection.
        /// If not provided, the default vector field will be used.
        /// </summary>
        public string Vector { set; get; }
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
    public SearchParameters Params { get; set; }

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
    public LookupLocation LookupFrom { set; get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecommendPointsByRequest"/> class.
    /// </summary>
    /// <param name="limit">Maximal number of nearest points to return.</param>
    internal RecommendPointsByRequest(uint limit)
    {
        Limit = limit;
    }
}
