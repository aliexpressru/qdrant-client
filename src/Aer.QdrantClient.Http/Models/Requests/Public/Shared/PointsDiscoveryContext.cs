using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents a points discovery context.
/// </summary>
public class PointsDiscoveryContext
{
    /// <summary>
    /// Look for vectors closest to those.
    /// </summary>
    [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
    public PointIdOrQueryVector Positive { get; }

    /// <summary>
    /// Try to avoid vectors like this.
    /// </summary>
    [JsonConverter(typeof(PointIdOrQueryVectorJsonConverter))]
    public PointIdOrQueryVector Negative { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="PointsDiscoveryContext"/> from positive and negative vector examples.
    /// </summary>
    /// <param name="positive">Look for vectors closest to those.</param>
    /// <param name="negative">Try to avoid vectors like this.</param>
    public PointsDiscoveryContext(PointIdOrQueryVector positive, PointIdOrQueryVector negative)
    {
        Positive = positive;
        Negative = negative;
    }
}
