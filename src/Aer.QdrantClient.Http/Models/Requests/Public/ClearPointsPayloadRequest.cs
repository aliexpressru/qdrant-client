// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the request to clear points payload by point identifiers.
/// </summary>
public sealed class ClearPointsPayloadRequest
{
    /// <summary>
    /// The point identifiers to clear payload for.
    /// </summary>
    [JsonConverter(typeof(PointIdCollectionJsonConverter))]
    public IEnumerable<PointId> Points { get; }

    /// <summary>
    /// Clears payload of each point that satisfy this filter condition.
    /// </summary>
    [JsonConverter(typeof(QdrantFilterJsonConverter))]
    public QdrantFilter Filter { get; }

    /// <summary>
    /// Create request to clear payload for points by point ids.
    /// </summary>
    /// <param name="pointIdsToClearPayloadFor">Point ids to clear payload for.</param>
    public ClearPointsPayloadRequest(IEnumerable<PointId> pointIdsToClearPayloadFor)
    {
        Points = pointIdsToClearPayloadFor;
        Filter = null;
    }

    /// <summary>
    /// Create request to clear payload for points by point filter.
    /// </summary>
    /// <param name="pointsFilterToClearPayloadFor">Points filter to clear payload for.</param>
    public ClearPointsPayloadRequest(QdrantFilter pointsFilterToClearPayloadFor)
    {
        Filter = pointsFilterToClearPayloadFor;
        Points = null;
    }
}
