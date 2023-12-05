// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the request to delete named points vectors.
/// </summary>
public sealed class DeletePointsVectorsRequest
{
    /// <summary>
    /// Names of vectors to delete.
    /// </summary>
    public IEnumerable<string> Vectors { get; }

    /// <summary>
    /// Deletes values from each point in this list.
    /// </summary>
    [JsonConverter(typeof(PointIdCollectionJsonConverter))]
    public IEnumerable<PointId> Points { get; }

    /// <summary>
    /// Deletes values from points that satisfy this filter condition.
    /// </summary>
    [JsonConverter(typeof(QdrantFilterJsonConverter))]
    public QdrantFilter Filter { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="DeletePointsVectorsRequest"/> with vector names
    /// and point ids to delete vectors for.
    /// </summary>
    /// <param name="vectorNamesToDelete">Names of vectors to delete.</param>
    /// <param name="pointsToDelteVectorsFor">Point ids to delete vectors for.</param>
    public DeletePointsVectorsRequest(
        IEnumerable<string> vectorNamesToDelete,
        IEnumerable<PointId> pointsToDelteVectorsFor)
    {
        Vectors = vectorNamesToDelete;
        Points = pointsToDelteVectorsFor;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DeletePointsVectorsRequest"/> with vector names
    /// and point filter to delete vectors for.
    /// </summary>
    /// <param name="vectorNamesToDelete">Names of vectors to delete.</param>>
    /// <param name="pointsFilterToDeleteVectorsFor">Points filter to delete vectors for.</param>
    public DeletePointsVectorsRequest(
        IEnumerable<string> vectorNamesToDelete,
        QdrantFilter pointsFilterToDeleteVectorsFor)
    {
        Vectors = vectorNamesToDelete;
        Filter = pointsFilterToDeleteVectorsFor;
    }
}
