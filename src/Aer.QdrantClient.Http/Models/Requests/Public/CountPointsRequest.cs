using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the points count request.
/// </summary>
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
    ///
    /// </summary>
    /// <param name="isCountExactPointsNumber">If <c>true</c>, count exact number of points. If <c>false</c>, count approximate number of points, which is faster.</param>
    /// <param name="filter">Count only points which satisfy the filter conditions.</param>
    public CountPointsRequest(bool isCountExactPointsNumber = true, QdrantFilter filter = null)
    {
        Exact = isCountExactPointsNumber;
        Filter = filter;
    }
}
