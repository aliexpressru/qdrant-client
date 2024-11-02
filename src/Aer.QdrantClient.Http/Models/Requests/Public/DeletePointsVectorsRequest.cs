using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the request to delete named points vectors.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
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
    /// The shard selector to perform operation only on specified shards.
    /// If not set - perform operation on all shards.
    /// </summary>
    [JsonConverter(typeof(ShardSelectorJsonConverter))]
    public ShardSelector ShardKey { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="DeletePointsVectorsRequest"/> with vector names
    /// and point ids to delete vectors for.
    /// </summary>
    /// <param name="vectorNamesToDelete">Names of vectors to delete.</param>
    /// <param name="pointsToDeleteVectorsFor">Point ids to delete vectors for.</param>
    public DeletePointsVectorsRequest(
        IEnumerable<string> vectorNamesToDelete,
        IEnumerable<PointId> pointsToDeleteVectorsFor)
    {
        Vectors = vectorNamesToDelete;
        Points = pointsToDeleteVectorsFor;
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
