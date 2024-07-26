using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the points count request.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class CountPointsRequest
{
    /// <summary>
    /// Count only points which satisfy the filter conditions.
    /// </summary>
    [JsonConverter(typeof(QdrantFilterJsonConverter))]
    public QdrantFilter Filter { get; }

    /// <summary>
    /// If <c>true</c>, count exact number of points. If <c>false</c>, count approximate number of points, which is faster.
    /// Approximate count might be unreliable during the indexing process. Default: <c>true</c>.
    /// </summary>
    public bool Exact { get; }

    /// <summary>
    /// The shard selector to perform operation only on specified shards.
    /// If not set - perform operation on all shards.
    /// </summary>
    [JsonConverter(typeof(ShardSelectorJsonConverter))]
    public ShardSelector ShardKey { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="CountPointsRequest"/>.
    /// </summary>
    /// <param name="isCountExactPointsNumber">If <c>true</c>, count exact number of points. If <c>false</c>, count approximate number of points, which is faster.</param>
    /// <param name="filter">Count only points which satisfy the filter conditions.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    public CountPointsRequest(
        bool isCountExactPointsNumber = true,
        QdrantFilter filter = null,
        ShardSelector shardSelector = null)
    {
        Exact = isCountExactPointsNumber;
        Filter = filter;
        ShardKey = shardSelector;
    }
}
