using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Http.Models.Requests;

/// <summary>
/// The request to create shards with specified shard key.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DeleteShardKeyRequest
{
    /// <summary>
    /// The shard key for shards to delete.
    /// </summary>
    [JsonConverter(typeof(ShardKeyJsonConverter))]
    public ShardKey ShardKey { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="DeleteShardKeyRequest"/>.
    /// </summary>
    /// <param name="shardKey">The shard key for shards to delete.</param>
    public DeleteShardKeyRequest(
        ShardKey shardKey)
    {
        ShardKey = shardKey;
    }
}
