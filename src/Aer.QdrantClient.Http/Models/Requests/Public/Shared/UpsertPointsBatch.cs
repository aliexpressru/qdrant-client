using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents a points' batch to upsert to qdrant.
/// </summary>
public sealed class UpsertPointsBatch
{
    /// <summary>
    /// Represents a batch of vectors.
    /// </summary>
    public sealed class VectorsBatch
    {
        /// <summary>
        /// Sets the unnamed dense or multi vectors.
        /// </summary>
        public IEnumerable<VectorBase> Vectors { get; }

        /// <summary>
        /// Sets the named (dense, sparse, multi) vectors.
        /// </summary>
        public Dictionary<string, IEnumerable<VectorBase>> NamedVectors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorsBatch"/> class.
        /// </summary>
        /// <param name="vectors">The array of points' vectors. Unnamed dense/multi vectors.</param>
        public VectorsBatch(IEnumerable<VectorBase> vectors)
        {
            Vectors = vectors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorsBatch"/> class.
        /// </summary>
        /// <param name="namedVectors">The array of points' vectors. Named vectors.</param>
        public VectorsBatch(Dictionary<string, IEnumerable<VectorBase>> namedVectors)
        {
            NamedVectors = namedVectors;
        }
    }

    /// <summary>
    /// The points' identifiers.
    /// </summary>
    [JsonConverter(typeof(PointIdIEnumerableJsonConverter))]
    public IEnumerable<PointId> Ids { get; }

    /// <summary>
    /// The points' payloads. Payload may contain either any serializable object or a dictionary or an already serialized JSON string.
    /// </summary>
    [JsonConverter(typeof(ObjectPayloadEnumerableJsonConverter))]
    public IEnumerable<object> Payloads { get; }

    /// <summary>
    /// The batch of points' vectors.
    /// </summary>
    [JsonConverter(typeof(VectorsBatchJsonConverter))]
    public VectorsBatch Vectors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpsertPointsBatch"/> class.
    /// </summary>
    /// <param name="ids">The points' identifiers.</param>
    /// <param name="vectorsBatch">The batch of points' vectors.</param>
    /// <param name="payloads">The points' payloads.</param>
    public UpsertPointsBatch(
        IEnumerable<PointId> ids,
        VectorsBatch vectorsBatch,
        IEnumerable<object> payloads)
    {
        Ids = ids;
        Vectors = vectorsBatch;
        Payloads = payloads;
    }
}