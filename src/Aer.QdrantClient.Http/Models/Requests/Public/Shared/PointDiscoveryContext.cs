using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents a point discovery context.
/// </summary>
public class PointDiscoveryContext
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
    /// Initializes a new instance of <see cref="PointDiscoveryContext"/> from positive and negative vector examples.
    /// </summary>
    /// <param name="positive">Look for vectors closest to those.</param>
    /// <param name="negative">Try to avoid vectors like this.</param>
    public PointDiscoveryContext(PointIdOrQueryVector positive, PointIdOrQueryVector negative)
    {
        Positive = positive;
        Negative = negative;
    }
}
