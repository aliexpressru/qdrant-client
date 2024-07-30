using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the recommend points grouped by specified field request.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class RecommendPointsGroupedRequest : RecommendPointsRequest
{
    /// <summary>
    /// Payload field to group by, must be a string or number field.
    /// If the field contains more than 1 value, all values will be used for grouping.
    /// One point can be in multiple groups.
    /// </summary>
    public string GroupBy { get; }

    /// <summary>
    /// Maximum amount of points to return per group.
    /// </summary>
    public uint GroupSize { get; }

    /// <summary>
    /// Look for points in another collection using the group ids.
    /// </summary>
    public LookupSearchParameters WithLookup { set; get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecommendPointsGroupedRequest"/> class.
    /// </summary>
    /// <param name="positiveVectorExamples">Recommend points closest to specified vectors.</param>
    /// <param name="groupBy">Payload field to group by, must be a string or number field.</param>
    /// <param name="groupsLimit">Maximum amount of groups to return.</param>
    /// <param name="groupSize">Maximum amount of points to return per group.</param>
    /// <param name="negativeVectorExamples">Optional vectors to avoid similarity with.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public RecommendPointsGroupedRequest(
        ICollection<PointIdOrQueryVector> positiveVectorExamples,
        string groupBy,
        uint groupsLimit,
        uint groupSize,
        ICollection<PointIdOrQueryVector> negativeVectorExamples = null,
        VectorSelector withVector = null,
        PayloadPropertiesSelector withPayload = null,
        ShardSelector shardSelector = null
    ) : base(
        positiveVectorExamples,
        groupsLimit,
        negativeVectorExamples,
        withVector,
        withPayload,
        shardSelector)
    {
        GroupBy = groupBy;
        GroupSize = groupSize;
    }
}
