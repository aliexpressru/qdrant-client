using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Responses.Base;
using System.Text.Json.Serialization;
using static Aer.QdrantClient.Http.Models.Responses.GetCollectionShardKeysResponse;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the list custom shard keys operation response.
/// </summary>
public sealed class GetCollectionShardKeysResponse : QdrantResponseBase<ShardKeysUnit>
{
    /// <summary>
    /// Represents info about all shard keys defined in the cluster.
    /// </summary>
    public class ShardKeysUnit
    {
        /// <summary>
        /// The existing shard keys. Only available when sharding method is custom.
        /// </summary>
        public ShardKeyUnit[] ShardKeys { get; init; }

        /// <summary>
        /// Represents a single shard key info.
        /// </summary>
        public class ShardKeyUnit
        {
            /// <summary>
            /// The existing shard key.
            /// </summary>
            [JsonConverter(typeof(ShardKeyJsonConverter))]
            public ShardKey Key { get; init; }
        }
    }
}
