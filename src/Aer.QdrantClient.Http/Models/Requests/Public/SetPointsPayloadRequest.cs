using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the request to set payload to each point by its id or by filter match.
/// </summary>
/// <typeparam name="TPayload">The type of the point payload.</typeparam>
public sealed class SetPointsPayloadRequest<TPayload>
    where TPayload : class
{
    /// <summary>
    /// The point payload.
    /// </summary>
    public TPayload Payload { get; }

    /// <summary>
    /// Assigns payload to each point in this list.
    /// </summary>
    [JsonConverter(typeof(PointIdCollectionJsonConverter))]
    public IEnumerable<PointId> Points { get; }

    /// <summary>
    /// Assigns payload to each point that satisfy this filter condition.
    /// </summary>
    [JsonConverter(typeof(QdrantFilterJsonConverter))]
    public QdrantFilter Filter { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="SetPointsPayloadRequest{TPayload}"/> with payload
    /// and point ids to set payload for.
    /// </summary>
    /// <param name="payload">Payload to set.</param>
    /// <param name="pointsToSetPayloadFor">Point ids to set payload for.</param>
    public SetPointsPayloadRequest(TPayload payload, IEnumerable<PointId> pointsToSetPayloadFor)
    {
        Payload = payload;
        Points = pointsToSetPayloadFor;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SetPointsPayloadRequest{TPayload}"/> with payload
    /// and point filter to set payload for.
    /// </summary>
    /// <param name="payload">Payload to set.</param>
    /// <param name="pointsFilterToSetPayloadFor">Points filter to set payload for.</param>
    public SetPointsPayloadRequest(TPayload payload, QdrantFilter pointsFilterToSetPayloadFor)
    {
        Payload = payload;
        Filter = pointsFilterToSetPayloadFor;
    }
}
