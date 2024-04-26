using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;

/// <summary>
/// Represents a discovery API request.
/// </summary>
[JsonDerivedType(typeof(DiscoverPointsByIdRequest))]
[JsonDerivedType(typeof(DiscoverPointsByExampleRequest))]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public abstract class DiscoverPointsByRequest
{
    #region Nested classes

    internal sealed class DiscoverPointsByIdRequest : DiscoverPointsByRequest
    {
        /// <summary>
        /// Look for vectors closest to this.
        /// </summary>
        [JsonConverter(typeof(PointIdJsonConverter))]
        public PointId Target { get;}

        /// <summary>
        /// Pairs of positive - negative examples to constrain the search.
        /// </summary>
        public List<DiscoveryContextUnit> Context { get; }

        /// <summary>
        /// The one discovery context positive-nragtive pair.
        /// </summary>
        public class DiscoveryContextUnit
        {
            /// <summary>
            /// Look for vectors closest to those.
            /// </summary>
            [JsonConverter(typeof(PointIdJsonConverter))]
            public PointId Positive { get; }

            /// <summary>
            /// Try to avoid vectors like this.
            /// </summary>
            [JsonConverter(typeof(PointIdJsonConverter))]
            public PointId Negative { get; }

            /// <summary>
            /// Initializes a new instance of <see cref="DiscoveryContextUnit"/>.
            /// </summary>
            /// <param name="positive">Positive point id.</param>
            /// <param name="negative">Negative point id.</param>
            public DiscoveryContextUnit(PointId positive, PointId negative)
            {
                Positive = positive;
                Negative = negative;
            }
        }

        public DiscoverPointsByIdRequest(
            PointId target,
            IEnumerable<KeyValuePair<PointId, PointId>> positiveNegativeContextPairs,
            uint limit) : base(limit)
        {
            Target = target;

            var gotCount = positiveNegativeContextPairs.TryGetNonEnumeratedCount(out var contextPaitCount);

            Context = gotCount
                ? new(contextPaitCount)
                : [];

            foreach (var (positivePoint, negativePoint) in positiveNegativeContextPairs)
            {
                Context.Add(new(positive: positivePoint, negative: negativePoint));
            }
        }
    }

    internal sealed class DiscoverPointsByExampleRequest : DiscoverPointsByRequest
    {
        /// <summary>
        /// Look for vectors closest to this.
        /// </summary>
        public float[] Target { get; }

        /// <summary>
        /// Pairs of positive - negative examples to constrain the search.
        /// </summary>
        public List<DiscoveryContextUnit> Context { get; }

        /// <summary>
        /// The one discovery context positive-nragtive pair.
        /// </summary>
        public class DiscoveryContextUnit
        {
            /// <summary>
            /// Look for vectors closest to those.
            /// </summary>
            public float[] Positive { get; }

            /// <summary>
            /// Try to avoid vectors like this.
            /// </summary>
            public float[] Negative { get; }

            /// <summary>
            /// Initializes a new instance of <see cref="DiscoveryContextUnit"/>.
            /// </summary>
            /// <param name="positive">Positive vector example.</param>
            /// <param name="negative">Negative vector example.</param>
            public DiscoveryContextUnit(float[] positive, float[] negative)
            {
                Positive = positive;
                Negative = negative;
            }
        }

        public DiscoverPointsByExampleRequest(
            float[] target,
            IEnumerable<KeyValuePair<float[], float[]>> positiveNegativeContextPairs,
            uint limit) : base(limit)
        {
            Target = target;

            var gotCount = positiveNegativeContextPairs.TryGetNonEnumeratedCount(out var contextPaitCount);

            Context = gotCount
                ? new(contextPaitCount)
                : [];

            foreach (var (positivePoint, negativePoint) in positiveNegativeContextPairs)
            {
                Context.Add(new(positive: positivePoint, negative: negativePoint));
            }
        }
    }

    #endregion

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
    /// Initializes a new instance of the <see cref="DiscoverPointsByRequest"/> class.
    /// </summary>
    /// <param name="limit">Maximal number of discovered points to return.</param>
    protected internal DiscoverPointsByRequest(uint limit)
    {
        Limit = limit;
    }
}
