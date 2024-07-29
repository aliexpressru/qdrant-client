using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;

/// <summary>
/// Represents a discovery API request.
/// </summary>
[SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class DiscoverPointsRequest
{
    /// <summary>
    /// Pairs of positive - negative examples to constrain the search.
    /// </summary>
    [JsonConverter(typeof(PointsDiscoveryContextCollectionJsonConverter))]
    public ICollection<PointsDiscoveryContext> Context { get; } = new List<PointsDiscoveryContext>();

    /// <summary>
    /// Look for vectors closest to this.
    /// </summary>
    [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
    public PointIdOrQueryVector Target { get; }

    /// <summary>
    /// Look only for points which satisfy the filter conditions.
    /// </summary>
    [JsonConverter(typeof(QdrantFilterJsonConverter))]
    public QdrantFilter Filter { get; set; }

    /// <summary>
    /// Additional search parameters.
    /// </summary>
    public VectorSearchParameters Params { get; set; }

    /// <summary>
    /// Max number of results to return.
    /// </summary>
    public uint Limit { get; }

    /// <summary>
    /// Offset of the first result to return. May be used to paginate results.
    /// Large offset values may cause performance issues.
    /// </summary>
    public uint Offset { get; set; } = 0;

    /// <summary>
    /// Whether the whole payload or only selected payload properties should be returned with the response.
    /// </summary>
    [JsonConverter(typeof(PayloadPropertiesSelectorJsonConverter))]
    public PayloadPropertiesSelector WithPayload { set; get; }

    /// <summary>
    /// Whether the vector, all named vectors or only selected named vectors should be returned with the response.
    /// </summary>
    [JsonConverter(typeof(VectorSelectorJsonConverter))]
    public VectorSelector WithVector { get; set; }

    /// <summary>
    /// Define a minimal score threshold for the result.
    /// If defined, less similar results will not be returned.
    /// Score of the returned result might be higher or smaller than the
    /// threshold depending on the Distance function used. E.g. for
    /// cosine similarity only higher scores will be returned.
    /// </summary>
    public float? ScoreThreshold { get; set; }

    /// <summary>
    /// Name of the vector that should be used to calculate recommendations.
    /// Only for collections with multiple named vectors.
    /// If not provided, the default vector field will be used.
    /// </summary>
    public string Using { get; set; }

    /// <summary>
    /// The location used to lookup vectors. If not specified - use current collection.
    /// </summary>
    /// <remarks>The other collection should have the same vector size as the current collection.</remarks>
    public VectorsLookupLocation LookupFrom { set; get; }

    /// <summary>
    /// The shard selector to perform operation only on specified shards.
    /// If not set - perform operation on all shards.
    /// </summary>
    [JsonConverter(typeof(ShardSelectorJsonConverter))]
    public ShardSelector ShardKey { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoverPointsRequest"/> class.
    /// </summary>
    /// <param name="positiveNegativeContextPairs">Pairs of positive - negative examples to constrain the search.</param>
    /// <param name="limit">Maximal number of nearest points to return.</param>
    /// <param name="target">Look for vectors closest to this.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    public DiscoverPointsRequest(
        IEnumerable<KeyValuePair<PointIdOrQueryVector, PointIdOrQueryVector>> positiveNegativeContextPairs,
        uint limit,
        PointIdOrQueryVector target = null,
        VectorSelector withVector = null,
        PayloadPropertiesSelector withPayload = null)
    {
        Target = target;
        WithVector = withVector;
        WithPayload = withPayload;
        Limit = limit;

        foreach (var pointsDiscoveryContextPair in positiveNegativeContextPairs)
        {
            var discoveryContext = new PointsDiscoveryContext(
                pointsDiscoveryContextPair.Key,
                pointsDiscoveryContextPair.Value);

            Context.Add(discoveryContext);
        }
    }
}
