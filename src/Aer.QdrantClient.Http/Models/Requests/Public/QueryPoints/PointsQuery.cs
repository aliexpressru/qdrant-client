using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Formulas;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
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
[JsonDerivedType(typeof(FormulaQuery))]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public abstract class PointsQuery
{
    internal sealed class NearestPointsQuery : PointsQuery
    {
        /// <summary>
        /// Represents a Maximal Marginal Relevance parameters.
        /// </summary>
        public sealed class MmrParameters
        {
            /// <summary>
            /// Tunable parameter for the MMR algorithm. Determines the balance between diversity and relevance.
            /// A higher value favors diversity(dissimilarity to selected results), while a lower value favors relevance(similarity to the query vector).
            /// Must be in the range[0, 1]. Default value is 0.5.
            /// </summary>
            public double? Diversity { get; init; }

            /// <summary>
            /// The maximum number of candidates to consider for re-ranking.
            /// If not specified, the limit query value is used.
            /// </summary>
            public uint? CandidatesLimit { get; init; }
        }

        /// <summary>
        /// Look for vectors closest to this.
        /// </summary>
        [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
        public PointIdOrQueryVector Nearest { get; }

        /// <summary>
        /// Is not <c>null</c> defines parameters for Maximal Marginal Relevance (MMR) algorithm.
        /// </summary>
        public MmrParameters Mmr { get; }

        internal NearestPointsQuery(PointIdOrQueryVector nearest, double? mmrDiversity = null, uint? mmrCandidatesLimit = null)
        {
            Nearest = nearest;

            if (mmrDiversity is not null
               || mmrCandidatesLimit is not null)
            {
                if (mmrDiversity is < 0 or > 1)
                {
                    throw new ArgumentException(
                        $"MMR diversity value must be in the range [0, 1], but was {mmrDiversity}");
                }

                if (mmrCandidatesLimit is < 0 or > 16384)
                {
                    throw new ArgumentException(
                        $"MMR candidates limit value must be in the range [0, 16384], but was {mmrCandidatesLimit}");
                }

                Mmr = new MmrParameters()
                {
                    Diversity = mmrDiversity,
                    CandidatesLimit = mmrCandidatesLimit
                };
            }
        }
    }

    internal sealed class RecommendPointsQuery : PointsQuery
    {
        public sealed class RecommendPointsQueryUnit
        {
            [JsonConverter(typeof(PointIdOrQueryVectorCollectionJsonConverter))]
            public ICollection<PointIdOrQueryVector> Positive { get; init; }

            [JsonConverter(typeof(PointIdOrQueryVectorCollectionJsonConverter))]
            public ICollection<PointIdOrQueryVector> Negative { get; init; }

            /// <summary>
            /// How to use positive and negative examples to find the results.
            /// </summary>
            public RecommendStrategy? Strategy { set; get; }
        }

        public RecommendPointsQueryUnit Recommend { get; }

        internal RecommendPointsQuery(
            ICollection<PointIdOrQueryVector> positive,
            ICollection<PointIdOrQueryVector> negative,
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
        public sealed class DiscoverPointsQueryUnit
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
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Required for serialization")]
        public string Sample { get; } = "random";
    }

    internal sealed class FormulaQuery : PointsQuery
    {
        [JsonConverter(typeof(QdrantFormulaJsonConverter))]
        public QdrantFormula Formula { get; }

        public Dictionary<string, object> Defaults { get; }

        internal FormulaQuery(QdrantFormula formula, Dictionary<string, object> defaults)
        {
            Formula = formula;
            Defaults = defaults;
        }
    }

    /// <summary>
    /// Creates a "find nearest points" query.
    /// </summary>
    /// <param name="pointIdOrQueryVector">The point id or vector to find nearest to.</param>
    /// <param name="mmrDiversity">
    /// Tunable parameter for the MMR algorithm. Determines the balance between diversity and relevance.
    /// A higher value favors diversity(dissimilarity to selected results), while a lower value favors relevance(similarity to the query vector).
    /// Must be in the range[0, 1]. Default value is 0.5.
    /// </param>
    /// <param name="mmrCandidatesLimit">The maximum number of candidates to consider for re-ranking. If not specified, the limit query value is used.</param>
    public static PointsQuery CreateFindNearestPointsQuery(
        PointIdOrQueryVector pointIdOrQueryVector,
        double? mmrDiversity = null,
        uint? mmrCandidatesLimit = null,
        VectorSearchParameters.AcornParameters acornParameters = null)
        =>
            new NearestPointsQuery(
                pointIdOrQueryVector,
                mmrDiversity: mmrDiversity,
                mmrCandidatesLimit: mmrCandidatesLimit);

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

    /// <summary>
    /// Creates a score boosting formula query.
    /// </summary>
    /// <param name="formula">The score calculating formula.</param>
    /// <param name="defaults">
    /// The defaults for cases when the variable (either from payload or prefetch score) is not found.
    /// Key - variable name, value - default variable value.
    /// </param>
    public static PointsQuery CreateFormulaQuery(QdrantFormula formula, Dictionary<string, object> defaults = null)
        => new FormulaQuery(formula, defaults);

    /// <summary>
    /// Implicitly converts query vector to an instance of <see cref="PointsQuery"/>.
    /// </summary>
    /// <param name="queryVector">The query vector to convert.</param>
    public static implicit operator PointsQuery(VectorBase queryVector) => CreateFindNearestPointsQuery(queryVector);

    /// <summary>
    /// Implicitly converts query vector to an instance of <see cref="PointsQuery"/>.
    /// </summary>
    /// <param name="queryVector">The query vector to convert.</param>
    public static implicit operator PointsQuery(QueryVector queryVector) => CreateFindNearestPointsQuery(queryVector);

    /// <summary>
    /// Implicitly converts point id to an instance of <see cref="PointsQuery"/>.
    /// </summary>
    /// <param name="pointId">The point id to convert.</param>
    public static implicit operator PointsQuery(PointId pointId) => CreateFindNearestPointsQuery(pointId);
}
