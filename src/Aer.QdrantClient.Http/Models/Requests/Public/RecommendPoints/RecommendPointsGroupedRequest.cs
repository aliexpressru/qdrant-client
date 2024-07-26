using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents a class for building instances of <see cref="RecommendPointsByGroupedRequest"/> grouped point recommendation requests.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class RecommendPointsGroupedRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecommendPointsByGroupedRequest"/> class.
    /// </summary>
    /// <param name="positiveVectorExamples">Recommend points closest to specified vectors.</param>
    /// <param name="groupBy">Payload field to group by, must be a string or number field.</param>
    /// <param name="groupsLimit">Maximum amount of groups to return.</param>
    /// <param name="groupSize">Maximum amount of points to return per group.</param>
    /// <param name="negativeVectorExamples">Optional vectors to avoid similarity with.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    public static RecommendPointsByGroupedRequest ByPointIds(
        IEnumerable<PointId> positiveVectorExamples,
        string groupBy,
        uint groupsLimit,
        uint groupSize,
        IEnumerable<PointId> negativeVectorExamples = null,
        VectorSelector withVector = null,
        PayloadPropertiesSelector withPayload = null)
    {
        var ret = new RecommendPointsByGroupedRequest.RecommendPointsByIdGroupedRequest(
            positiveVectorExamples,
            negativeVectorExamples,
            groupBy,
            groupsLimit,
            groupSize
        )
        {
            WithVector = withVector ?? VectorSelector.None,
            WithPayload = withPayload
        };

        return ret;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecommendPointsByGroupedRequest"/> class.
    /// </summary>
    /// <param name="positiveVectorExamples">Recommend points closest to specified vectors.</param>
    /// <param name="groupBy">Payload field to group by, must be a string or number field.</param>
    /// <param name="groupsLimit">Maximum amount of groups to return.</param>
    /// <param name="groupSize">Maximum amount of points to return per group.</param>
    /// <param name="negativeVectorExamples">Optional vectors to avoid similarity with.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    public static RecommendPointsByGroupedRequest ByVectorExamples(
        IEnumerable<float[]> positiveVectorExamples,
        string groupBy,
        uint groupsLimit,
        uint groupSize,
        IEnumerable<float[]> negativeVectorExamples = null,
        VectorSelector withVector = null,
        PayloadPropertiesSelector withPayload = null)
    {
        var ret = new RecommendPointsByGroupedRequest.RecommendPointsByExampleGroupedRequest(
            positiveVectorExamples,
            negativeVectorExamples,
            groupBy,
            groupsLimit,
            groupSize)
        {
            WithVector = withVector ?? VectorSelector.None,
            WithPayload = withPayload
        };

        return ret;
    }
}
