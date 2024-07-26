using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;

/// <summary>
/// Represents a builder class for building instances of <see cref="DiscoverPointsByRequest"/> point discovery requests.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public static class DiscoverPointsRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoverPointsByRequest"/> class with point id vector target and context.
    /// </summary>
    /// <param name="positiveNegativeContextPairs">Pairs of positive - negative examples to constrain the search.</param>
    /// <param name="limit">Maximal number of nearest points to return.</param>
    /// <param name="target">Look for vectors closest to this.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    public static DiscoverPointsByRequest ByPointIds(
        IEnumerable<KeyValuePair<PointId, PointId>> positiveNegativeContextPairs,
        uint limit,
        PointId target = null,
        VectorSelector withVector = null,
        PayloadPropertiesSelector withPayload = null)
    {
        var ret = new DiscoverPointsByRequest.DiscoverPointsByIdRequest(target, positiveNegativeContextPairs, limit)
        {
            WithVector = withVector ?? VectorSelector.None,
            WithPayload = withPayload
        };

        return ret;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoverPointsByRequest"/> class with raw vector target and context.
    /// </summary>
    /// <param name="positiveNegativeContextPairs">Pairs of positive - negative examples to constrain the search.</param>
    /// <param name="limit">Maximal number of nearest points to return.</param>
    /// <param name="target">Look for vectors closest to this.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    public static DiscoverPointsByRequest ByVectorExamples(
        IEnumerable<KeyValuePair<float[], float[]>> positiveNegativeContextPairs,
        uint limit,
        float[] target = null,
        VectorSelector withVector = null,
        PayloadPropertiesSelector withPayload = null)
    {
        var ret = new DiscoverPointsByRequest.DiscoverPointsByExampleRequest(target, positiveNegativeContextPairs, limit)
        {
            WithVector = withVector ?? VectorSelector.None,
            WithPayload = withPayload
        };

        return ret;
    }
}
