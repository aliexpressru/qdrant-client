using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the points search request.
/// </summary>
public class SearchPointsRequest
{
    #region Nested classes

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

    #endregion

    /// <summary>
    /// Look for vectors closest to this
    /// </summary>
    [JsonConverter(typeof(SearchVectorJsonConverter))]
    public SearchVector Vector { get; }

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
    /// Whether the payload should be returned with the response.
    /// </summary>
    [JsonConverter(typeof(PayloadPropertiesSelectorJsonConverter))]
    public PayloadPropertiesSelector WithPayload { set; get; }

    /// <summary>
    /// Whether the vector should be returned with the response.
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
    /// Initializes a new instance of the <see cref="SearchPointsRequest"/> class.
    /// </summary>
    /// <param name="vector">The vector to search for.</param>
    /// <param name="limit">Maximal number of nearest points to return.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    public SearchPointsRequest(
        SearchVector vector,
        uint limit,
        VectorSelector withVector = null,
        PayloadPropertiesSelector withPayload = null)
    {
        Vector = vector;
        Limit = limit;
        WithVector = withVector ?? VectorSelector.None;
        WithPayload = withPayload;
    }
}
