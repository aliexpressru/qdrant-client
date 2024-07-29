using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;

/// <summary>
/// Query to perform.
/// </summary>
public abstract class PointsQuery
{
    internal sealed class SpecificPointQuery : PointsQuery
    {
        /// <summary>
        /// The point id to get point for.
        /// </summary>
        [JsonConverter(typeof(PointIdJsonConverter))]
        public PointId PointId { get; init; }
    }

    internal sealed class NearestPointsQuery : PointsQuery
    {
        /// <summary>
        /// Look for vectors closest to this.
        /// </summary>
        [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
        public PointIdOrQueryVector Nearest { get; init; }
    }

    internal sealed class RecommendPointsQuery : PointsQuery
    {
        [JsonConverter(typeof(PointIdOrQueryVectorCollectionJsonConverter))]
        public IEnumerable<PointIdOrQueryVector> Positive { get; init; }

        [JsonConverter(typeof(PointIdOrQueryVectorCollectionJsonConverter))]
        public IEnumerable<PointIdOrQueryVector> Negative { get; init; }

        /// <summary>
        /// How to use positive and negative examples to find the results.
        /// </summary>
        public RecommendStrategy Strategy { set; get; }
    }

    internal sealed class DiscoverPointsQuery : PointsQuery
    {
        [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
        public PointIdOrQueryVector Target { get; init; }

        [JsonConverter(typeof(PointsDiscoveryContextCollectionJsonConverter))]
        public ICollection<PointsDiscoveryContext> Context { get; } = new List<PointsDiscoveryContext>();
    }

    internal sealed class ContextQuery : PointsQuery
    {
        [JsonConverter(typeof(PointsDiscoveryContextCollectionJsonConverter))]
        public ICollection<PointsDiscoveryContext> Context { get; } = new List<PointsDiscoveryContext>();
    }

    internal sealed class OrderByQuery : PointsQuery
    {
        /// <summary>
        /// Order the records by a selected payload field.
        /// </summary>
        /// <remarks>When you use the <see cref="OrderBySelector"/> parameter, pagination is disabled.</remarks>
        public OrderBySelector OrderBy { get; init; }
    }

    internal sealed class FusionQuery : PointsQuery
    {
        public FusionAlgorithm Fusion { get; } = FusionAlgorithm.Rrf;
    }
}
