using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;

/// <summary>
/// Query to perform.
/// </summary>
[JsonDerivedType(typeof(SpecificPointQuery))]
[JsonDerivedType(typeof(NearestPointsQuery))]
[JsonDerivedType(typeof(RecommendPointsQuery))]
[JsonDerivedType(typeof(DiscoverPointsQuery))]
[JsonDerivedType(typeof(ContextQuery))]
[JsonDerivedType(typeof(OrderByQuery))]
[JsonDerivedType(typeof(FusionQuery))]
public abstract class PointsQuery
{
    internal sealed class SpecificPointQuery : PointsQuery
    {
        /// <summary>
        /// The point id to get point for.
        /// </summary>
        [JsonConverter(typeof(PointIdJsonConverter))]
        public PointId PointId { get; set; }
    }

    internal sealed class NearestPointsQuery : PointsQuery
    {
        /// <summary>
        /// Look for vectors closest to this.
        /// </summary>
        [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
        public PointIdOrQueryVector PointIdOrQueryVector { get; init; }
    }

    internal sealed class RecommendPointsQuery : PointsQuery
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

    internal sealed class DiscoverPointsQuery : PointsQuery
    {
        [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
        public PointIdOrQueryVector Target { set; get; }

        [JsonConverter(typeof(PointsDiscoveryContextCollectionJsonConverter))]
        public ICollection<PointsDiscoveryContext> Context { get; set; }
    }

    internal sealed class ContextQuery : PointsQuery
    {
        [JsonConverter(typeof(PointsDiscoveryContextCollectionJsonConverter))]
        public ICollection<PointsDiscoveryContext> Context { get; set; }
    }

    internal sealed class OrderByQuery : PointsQuery
    {
        /// <summary>
        /// Order the records by a selected payload field.
        /// </summary>
        /// <remarks>When you use the <see cref="OrderBySelector"/> parameter, pagination is disabled.</remarks>
        public OrderBySelector OrderBy { get; set; }
    }

    internal sealed class FusionQuery : PointsQuery
    {
        public FusionAlgorithm Fusion { get; } = FusionAlgorithm.Rrf;
    }
}
