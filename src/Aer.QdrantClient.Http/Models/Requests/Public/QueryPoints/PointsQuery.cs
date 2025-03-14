using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;

/// <summary>
/// Query to perform.
/// </summary>
[JsonDerivedType(typeof(NearestPointsQuery))]
[JsonDerivedType(typeof(RecommendPointsQuery))]
[JsonDerivedType(typeof(DiscoverPointsQuery))]
[JsonDerivedType(typeof(ContextQuery))]
[JsonDerivedType(typeof(OrderByQuery))]
[JsonDerivedType(typeof(FusionQuery))]
[JsonDerivedType(typeof(SampleQuery))]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public abstract class PointsQuery
{
    internal sealed class NearestPointsQuery : PointsQuery
    {
        /// <summary>
        /// Look for vectors closest to this.
        /// </summary>
        [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
        public PointIdOrQueryVector Nearest { get; }

        internal NearestPointsQuery(PointIdOrQueryVector nearest)
        {
            Nearest = nearest;
        }
    }

    internal sealed class RecommendPointsQuery : PointsQuery
    {
        public class RecommendPointsQueryUnit
        {
            [JsonConverter(typeof(PointIdOrQueryVectorCollectionJsonConverter))]
            public IEnumerable<PointIdOrQueryVector> Positive { get; init; }

            [JsonConverter(typeof(PointIdOrQueryVectorCollectionJsonConverter))]
            public IEnumerable<PointIdOrQueryVector> Negative { get; init; }

            /// <summary>
            /// How to use positive and negative examples to find the results.
            /// </summary>
            public RecommendStrategy? Strategy { set; get; }
        }

        public RecommendPointsQueryUnit Recommend { get; }

        internal RecommendPointsQuery(
            IEnumerable<PointIdOrQueryVector> positive,
            IEnumerable<PointIdOrQueryVector> negative,
            RecommendStrategy? strategy
        )
        {
            Recommend = new RecommendPointsQueryUnit()
            {
                Positive = positive,
                Negative = negative,
                Strategy = strategy
            };
        }
    }

    internal sealed class DiscoverPointsQuery : PointsQuery
    {
        public class DiscoverPointsQueryUnit
        {
            [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
            public PointIdOrQueryVector Target { get; init; }

            [JsonConverter(typeof(PointsDiscoveryContextCollectionJsonConverter))]
            public ICollection<PointsDiscoveryContext> Context { get; init; }
        }

        public DiscoverPointsQueryUnit Discover { get; }

        internal DiscoverPointsQuery(PointIdOrQueryVector target, ICollection<PointsDiscoveryContext> context)
        {
            Discover = new DiscoverPointsQueryUnit()
            {
                Target = target,
                Context = context
            };
        }
    }

    internal sealed class ContextQuery : PointsQuery
    {
        [JsonConverter(typeof(PointsDiscoveryContextCollectionJsonConverter))]
        public ICollection<PointsDiscoveryContext> Context { get; }

        internal ContextQuery(ICollection<PointsDiscoveryContext> context)
        {
            Context = context;
        }
    }

    internal sealed class OrderByQuery : PointsQuery
    {
        /// <summary>
        /// Order the records by a selected payload field.
        /// </summary>
        /// <remarks>When you use the <see cref="OrderBySelector"/> parameter, pagination is disabled.</remarks>
        public OrderBySelector OrderBy { get; }

        internal OrderByQuery(OrderBySelector orderBy)
        {
            OrderBy = orderBy;
        }
    }

    internal sealed class FusionQuery : PointsQuery
    {
        public FusionAlgorithm Fusion { get; }

        internal FusionQuery(FusionAlgorithm fusionAlgorithm)
        {
            Fusion = fusionAlgorithm;
        }
    }

    internal sealed class SampleQuery : PointsQuery
    {
        public string Sample { get; } = "random";
    }

    /// <summary>
    /// Creates a "find nearest points" query.
    /// </summary>
    /// <param name="pointIdOrQueryVector">The point id or vector to find nearest to.</param>
    public static PointsQuery CreateFindNearestPointsQuery(PointIdOrQueryVector pointIdOrQueryVector)
        =>
            new NearestPointsQuery(pointIdOrQueryVector);

    /// <summary>
    /// Creates a "recommend points" query.
    /// </summary>
    /// <param name="positiveVectorExamples">Recommend points closest to specified vectors.</param>
    /// <param name="strategy">How to use positive and negative examples to find the results.</param>
    /// <param name="negativeVectorExamples">Optional vectors to avoid similarity with.</param>
    public static PointsQuery CreateRecommendPointsQuery(
        ICollection<PointIdOrQueryVector> positiveVectorExamples,
        RecommendStrategy? strategy = null,
        ICollection<PointIdOrQueryVector> negativeVectorExamples = null)
        =>
            new RecommendPointsQuery(positiveVectorExamples, negativeVectorExamples, strategy);

    /// <summary>
    /// Creates a "discover points" query.
    /// </summary>
    /// <param name="positiveNegativeContextPairs">Pairs of positive - negative examples to constrain the search.</param>
    /// <param name="target">Look for vectors closest to this.</param>
    public static PointsQuery CreateDiscoverPointsQuery(
        ICollection<PointsDiscoveryContext> positiveNegativeContextPairs,
        PointIdOrQueryVector target = null)
        =>
            new DiscoverPointsQuery(target, positiveNegativeContextPairs);

    /// <summary>
    /// Creates a "points discovery context" query.
    /// </summary>
    /// <param name="context">Pairs of positive - negative examples to constrain the search.</param>
    public static PointsQuery CreatePointsContextQuery(ICollection<PointsDiscoveryContext> context)
        => new ContextQuery(context);

    /// <summary>
    /// Creates an "order by" query.
    /// </summary>
    /// <param name="orderBySelector">The selector for the field that the results should be ordered by.</param>
    public static PointsQuery CreateOrderByQuery(OrderBySelector orderBySelector)
        => new OrderByQuery(orderBySelector);

    /// <summary>
    /// Creates a "fusion" query.
    /// </summary>
    /// <param name="fusionAlgorithm">The type of the algorithm used to combine prefetch results.</param>
    public static PointsQuery CreateFusionQuery(FusionAlgorithm fusionAlgorithm = FusionAlgorithm.Rrf)
        => new FusionQuery(fusionAlgorithm);

    /// <summary>
    /// Creates a "random sample" query.
    /// </summary>
    public static PointsQuery CreateSampleQuery() => new SampleQuery();
}
