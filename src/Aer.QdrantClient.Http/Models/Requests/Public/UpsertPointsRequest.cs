using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the points upsert request.
/// </summary>
/// <typeparam name="TPayload">The type of the point payload.</typeparam>
public sealed class UpsertPointsRequest<TPayload>
    where TPayload : class
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
        /// The point payload.
        /// </summary>
        public TPayload Payload { get; }

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
            TPayload payload)
        {
            Id = id;
            Vector = vector;
            Payload = payload;
        }
    }

    /// <summary>
    /// The points to upsert.
    /// </summary>
    public required IEnumerable<UpsertPoint> Points { get; set; }

    /// <summary>
    /// The shard selector to perform operation only on specified shards.
    /// If not set - perform operation on all shards.
    /// </summary>
    [JsonConverter(typeof(ShardSelectorJsonConverter))]
    public ShardSelector ShardKey { get; set; }
}
