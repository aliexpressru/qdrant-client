using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;

/// <summary>
/// Query to perform.
/// </summary>
[JsonDerivedType(typeof(NearestPointsQuery))]
public abstract class PointsQueryBase
{
    internal sealed class SpecificPointQuery : PointsQueryBase
    {
        /// <summary>
        /// The point id to get point for.
        /// </summary>
        [JsonConverter(typeof(PointIdJsonConverter))]
        public PointId PointId { get; set; }
    }

    internal sealed class NearestPointsQuery : PointsQueryBase
    {
        /// <summary>
        /// Look for vectors closest to this.
        /// </summary>
        [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
        public PointIdOrQueryVector PointIdOrQueryVector { get; init; }
    }

    internal sealed class RecommendPointsQuery : PointsQueryBase
    {
        [JsonConverter(typeof(PointIdOrQueryVectorCollectionJsonConverter))]
        public IEnumerable<PointIdOrQueryVector> Positive { set; get; }

        [JsonConverter(typeof(PointIdOrQueryVectorCollectionJsonConverter))]
        public IEnumerable<PointIdOrQueryVector> Negative { set; get; }

        /// <summary>
        /// How to use positive and negative examples to find the results.
        /// </summary>
        public RecommendStrategy Strategy { set; get; }
    }

    internal sealed class DiscoverPointsQuery : PointsQueryBase
    {
        [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
        public PointIdOrQueryVector Target { set; get; }

        public List<ContextPair> Context { get; set; }
    }

    internal sealed class ContextQuery : PointsQueryBase
    {
        public List<ContextPair> ContextPairs { get; set; }
    }

    internal sealed class OrderByQuery : PointsQueryBase
    {
        /// <summary>
        /// Order the records by a selected payload field.
        /// </summary>
        /// <remarks>When you use the <see cref="OrderBySelector"/> parameter, pagination is disabled.</remarks>
        public OrderBySelector OrderBy { get; set; }
    }

    internal sealed class FusionQuery : PointsQueryBase
    {
        public FusionAlgorithm Fusion { get; } = FusionAlgorithm.Rrf;
    }
}
