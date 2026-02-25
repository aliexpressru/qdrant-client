using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Responses.Shared;

/// <summary>
/// Represents a resharding operation information. 
/// </summary>
public sealed class ReshardingOperationInfo
{
    /// <summary>
    /// Resharding direction, scale up or down in number of shards.
    /// </summary>
    public ReshardingOperationDirection Direction { init; get; }

    /// <summary>
    /// The id of the shards being added or removed.
    /// </summary>
    public uint ShardId { init; get; }

    /// <summary>
    /// The peer id that the shard is being added or removed from.
    /// </summary>
    public ulong PeerId { init; get; }

    /// <summary>
    /// The peer uri that the shard is being added or removed from.
    /// </summary>
    public string PeerUri { set; get; }

    /// <summary>
    /// The shard key for the resharding operation.
    /// </summary>
    [JsonConverter(typeof(ShardKeyJsonConverter))]
    public ShardKey ShardKey { init; get; }
}
