using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents a class for building instances of <see cref="RecommendPointsByRequest"/> point recommendation requests.
/// </summary>
public class RecommendPointsRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecommendPointsByRequest"/> class.
    /// </summary>
    /// <param name="positiveVectorExamples">Recommend points closest to specified vectors.</param>
    /// <param name="limit">Maximal number of nearest points to return.</param>
    /// <param name="negativeVectorExamples">Optional vectors to avoid similarity with.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    public static RecommendPointsByRequest ByPointIds(
        IEnumerable<PointId> positiveVectorExamples,
        uint limit,
        IEnumerable<PointId> negativeVectorExamples = null,
        VectorSelector withVector = null,
        PayloadPropertiesSelector withPayload = null)
    {
        var ret = new RecommendPointsByRequest.RecommendPointsByIdRequest(
            positiveVectorExamples,
            negativeVectorExamples,
            limit)
        {
            WithVector = withVector ?? VectorSelector.None,
            WithPayload = withPayload
        };

        return ret;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecommendPointsByRequest"/> class.
    /// </summary>
    /// <param name="positiveVectorExamples">Recommend points closest to specified vectors.</param>
    /// <param name="limit">Maximal number of nearest points to return.</param>
    /// <param name="negativeVectorExamples">Optional vectors to avoid similarity with.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    public static RecommendPointsByRequest ByVectorExamples(
        IEnumerable<float[]> positiveVectorExamples,
        uint limit,
        IEnumerable<float[]> negativeVectorExamples = null,
        VectorSelector withVector = null,
        PayloadPropertiesSelector withPayload = null)
    {
        var ret = new RecommendPointsByRequest.RecommendPointsByExampleRequest(
            positiveVectorExamples,
            negativeVectorExamples,
            limit)
        {
            WithVector = withVector ?? VectorSelector.None,
            WithPayload = withPayload
        };

        return ret;
    }
}
