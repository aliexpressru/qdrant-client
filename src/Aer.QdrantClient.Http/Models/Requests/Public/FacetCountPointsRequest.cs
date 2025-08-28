using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents a faceting count points request.
/// </summary>
public sealed class FacetCountPointsRequest
{
    /// <summary>
    /// Payload key to use for faceting.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Max number of results to return.
    /// </summary>
    public uint Limit { get; }

    /// <summary>
    /// The shard selector to perform operation only on specified shards.
    /// If not set - perform operation on all shards.
    /// </summary>
    [JsonConverter(typeof(ShardSelectorJsonConverter))]
    public ShardSelector ShardKey { get; set; }

    /// <summary>
    /// Filter conditions - only consider points that satisfy these conditions.
    /// </summary>
    [JsonConverter(typeof(QdrantFilterJsonConverter))]
    public QdrantFilter Filter { get; set; }

    /// <summary>
    /// Whether to do a more expensive exact count for each of the values in the facet. Default is false.
    /// If set to <c>false</c> the way Qdrant the counts for each value is approximate to achieve fast results.
    /// </summary>
    public bool Exact { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="FacetCountPointsRequest"/>.
    /// </summary>
    /// <param name="key">Payload key to use for faceting.</param>
    /// <param name="limit">Max number of hits to return. Default is <c>10</c>.</param>
    /// <param name="exact">Whether to do a more expensive exact count for each of the values in the facet. Default is <c>false</c>.</param>
    /// <param name="filter">Filter conditions - only consider points that satisfy these conditions.</param>
    /// <param name="shardSelector">The shard selector to perform operation only on specified shards.</param>
    public FacetCountPointsRequest(
        string key,
        uint limit = 10,
        bool exact = false,
        QdrantFilter filter = null,
        ShardSelector shardSelector = null)
    {
        Key = key;
        Limit = limit;
        Filter = filter;
        ShardKey = shardSelector;
        Exact = exact;
    }
}
