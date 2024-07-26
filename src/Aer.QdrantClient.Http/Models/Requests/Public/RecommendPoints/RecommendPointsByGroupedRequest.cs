using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the recommend points grouped by specified field request.
/// </summary>
[JsonDerivedType(typeof(RecommendPointsByIdGroupedRequest))]
[JsonDerivedType(typeof(RecommendPointsByExampleGroupedRequest))]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public abstract class RecommendPointsByGroupedRequest : RecommendPointsByRequest
{
    #region Nested classes

    internal sealed class RecommendPointsByIdGroupedRequest : RecommendPointsByGroupedRequest
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

        public RecommendPointsByIdGroupedRequest(
            IEnumerable<PointId> positive,
            IEnumerable<PointId> negative,
            string groupBy,
            uint groupsLimit,
            uint groupSize
        ) : base(groupBy, groupsLimit, groupSize)
        {
            Positive = positive;
            Negative = negative;
        }
    }

    internal sealed class RecommendPointsByExampleGroupedRequest : RecommendPointsByGroupedRequest
    {
        /// <summary>
        /// Look for vectors closest to those.
        /// </summary>
        public IEnumerable<float[]> Positive { get; }

        /// <summary>
        /// Try to avoid vectors like this.
        /// </summary>
        public IEnumerable<float[]> Negative { get; }

        public RecommendPointsByExampleGroupedRequest(
            IEnumerable<float[]> positive,
            IEnumerable<float[]> negative,
            string groupBy,
            uint groupsLimit,
            uint groupSize) : base(groupBy, groupsLimit, groupSize)
        {
            Positive = positive;
            Negative = negative;
        }
    }

    #endregion

    /// <summary>
    /// Payload field to group by, must be a string or number field.
    /// If the field contains more than 1 value, all values will be used for grouping.
    /// One point can be in multiple groups.
    /// </summary>
    public string GroupBy { get; }

    /// <summary>
    /// Maximum amount of groups to return.
    /// </summary>
    public new uint Limit { get; }

    /// <summary>
    /// Maximum amount of points to return per group.
    /// </summary>
    public uint GroupSize { get; }

    /// <summary>
    /// Look for points in another collection using the group ids.
    /// </summary>
    public LookupSearchParameters WithLookup { set; get; }

    internal RecommendPointsByGroupedRequest(
        string groupBy,
        uint groupsLimit,
        uint groupSize) : base(groupsLimit)
    {
        GroupBy = groupBy;
        Limit = groupsLimit;
        GroupSize = groupSize;
    }
}
