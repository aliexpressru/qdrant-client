using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents a builder class for building instances of <see cref="RecommendPointsByRequest"/> point recommendation requests.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public static class RecommendPointsRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecommendPointsByRequest"/> class with point id vector examples.
    /// </summary>
    /// <param name="positiveVectorExamples">Recommend points closest to specified vectors.</param>
    /// <param name="limit">Maximal number of points to return.</param>
    /// <param name="negativeVectorExamples">Optional vectors to avoid similarity with.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public static RecommendPointsByRequest ByPointIds(
        IEnumerable<PointId> positiveVectorExamples,
        uint limit,
        IEnumerable<PointId> negativeVectorExamples = null,
        VectorSelector withVector = null,
        PayloadPropertiesSelector withPayload = null,
        ShardSelector shardSelector = null)
    {
        var ret = new RecommendPointsByRequest.RecommendPointsByIdRequest(
            positiveVectorExamples,
            negativeVectorExamples,
            limit)
        {
            WithVector = withVector ?? VectorSelector.None,
            WithPayload = withPayload,
            ShardKey = shardSelector
        };

        return ret;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecommendPointsByRequest"/> class with raw vector examples.
    /// </summary>
    /// <param name="positiveVectorExamples">Recommend points closest to specified vectors.</param>
    /// <param name="limit">Maximal number of points to return.</param>
    /// <param name="negativeVectorExamples">Optional vectors to avoid similarity with.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public static RecommendPointsByRequest ByVectorExamples(
        IEnumerable<float[]> positiveVectorExamples,
        uint limit,
        IEnumerable<float[]> negativeVectorExamples = null,
        VectorSelector withVector = null,
        PayloadPropertiesSelector withPayload = null,
        ShardSelector shardSelector = null)
    {
        var ret = new RecommendPointsByRequest.RecommendPointsByExampleRequest(
            positiveVectorExamples,
            negativeVectorExamples,
            limit)
        {
            WithVector = withVector ?? VectorSelector.None,
            WithPayload = withPayload,
            ShardKey = shardSelector
        };

        return ret;
    }
}
