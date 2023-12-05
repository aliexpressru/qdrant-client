using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the points payload overwrite request.
/// </summary>
/// <typeparam name="TPayload">The type of the point payload.</typeparam>
public sealed class OverwritePointsPayloadRequest<TPayload>
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
    /// Initializes a new instance of <see cref="OverwritePointsPayloadRequest{TPayload}"/> with payload
    /// and point ids to overwrite payload for.
    /// </summary>
    /// <param name="payload">Payload to overwrite.</param>
    /// <param name="pointsToOverwritePayloadFor">Point ids to overwrite payload for.</param>
    public OverwritePointsPayloadRequest(
        TPayload payload,
        IEnumerable<PointId> pointsToOverwritePayloadFor)
    {
        Payload = payload;
        Points = pointsToOverwritePayloadFor;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="OverwritePointsPayloadRequest{TPayload}"/> with payload
    /// and point filter to overwrite payload for.
    /// </summary>
    /// <param name="payload">Payload to overwrite.</param>
    /// <param name="pointsFilterToOverwritePayloadFor">Points filter to overwrite payload for.</param>
    public OverwritePointsPayloadRequest(
        TPayload payload,
        QdrantFilter pointsFilterToOverwritePayloadFor)
    {
        Payload = payload;
        Filter = pointsFilterToOverwritePayloadFor;
    }
}
