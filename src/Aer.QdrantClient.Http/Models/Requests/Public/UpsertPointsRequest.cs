using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the points upsert request.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class UpsertPointsRequest
{
    /// <summary>
    /// Represents a point to upsert to qdrant.
    /// </summary>
    public sealed class UpsertPoint
    {
        /// <summary>
        /// The point identifier.
        /// </summary>
        [JsonConverter(typeof(PointIdJsonConverter))]
        public PointId Id { get; }

        /// <summary>
        /// The point payload. May contain either any serializable object or a dictionary or an already serialized JSON string.
        /// </summary>
        [JsonConverter(typeof(ObjectPayloadJsonConverter))]
        public object Payload { get; }

        /// <summary>
        /// The point vector.
        /// </summary>
        [JsonConverter(typeof(VectorJsonConverter))]
        public VectorBase Vector { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpsertPoint"/> class.
        /// </summary>
        /// <param name="id">The point identifier.</param>
        /// <param name="vector">The point vector.</param>
        /// <param name="payload">The point payload.</param>
        public UpsertPoint(
            PointId id,
            VectorBase vector,
            object payload)
        {
            Id = id;
            Vector = vector;
            Payload = payload;
        }
    }
    
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

    /// <summary>
    /// The points to upsert.
    /// </summary>
    public IEnumerable<UpsertPoint> Points { get; set; }
    
    /// <summary>
    /// The points' batch to upsert.
    /// </summary>
    public UpsertPointsBatch Batch { get; set; }

    /// <summary>
    /// The shard selector to perform operation only on specified shards.
    /// If not set - perform operation on all shards.
    /// </summary>
    [JsonConverter(typeof(ShardSelectorJsonConverter))]
    public ShardSelector ShardKey { get; set; }

    /// <summary>
    /// If specified, only points that match this filter will be updated, others will be inserted.
    /// </summary>
    [JsonConverter(typeof(QdrantFilterJsonConverter))]
    public QdrantFilter UpdateFilter { get; set; }
}
