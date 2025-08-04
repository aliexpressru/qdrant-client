using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the search points distance matrix API request.
/// </summary>
public sealed class SearchPointsDistanceMatrixRequest
{
    /// <summary>
    /// The shard selector to perform operation only on specified shards.
    /// If not set - perform operation on all shards.
    /// </summary>
    [JsonConverter(typeof(ShardSelectorJsonConverter))]
    public ShardSelector ShardKey { get; set; }

    /// <summary>
    /// Look only for points which satisfy the filter conditions.
    /// </summary>
    [JsonConverter(typeof(QdrantFilterJsonConverter))]
    public QdrantFilter Filter { get; set; }

    /// <summary>
    /// How many points to select and search within. Default is 10.
    /// </summary>
    public uint Sample { set; get; } = 10;

    /// <summary>
    /// How many neighbours per sample to find. Default is 3.
    /// </summary>
    public uint Limit { set; get; } = 3;

    /// <summary>
    /// Name of the vector that should be used to calculate recommendations.
    /// Only for collections with multiple named vectors.
    /// If not provided, the default vector field will be used.
    /// </summary>
    public string Using { get; set; }
}
