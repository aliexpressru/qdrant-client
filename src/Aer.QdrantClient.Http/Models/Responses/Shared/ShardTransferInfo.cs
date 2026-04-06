using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Responses.Shared;

/// <summary>
/// Represents information about ongoing shard transfer operation.
/// </summary>
public sealed class ShardTransferInfo
{
    /// <summary>
    /// The transferring shard identifier.
    /// </summary>
    public uint ShardId { init; get; }

    /// <summary>
    /// The peer id that the shard is being transferred from.
    /// </summary>
    public ulong From { init; get; }

    /// <summary>
    /// The peer id that the shard is being transferred to.
    /// </summary>
    public ulong To { init; get; }

    /// <summary>
    /// If <c>true</c> transfer is a synchronization of a replicas.
    /// If <c>false</c> transfer is a moving of a shard from one peer to another.
    /// </summary>
    public bool Sync { init; get; }

    /// <summary>
    /// The method used to transfer shards.
    /// </summary>
    [JsonConverter(typeof(JsonStringSnakeCaseLowerEnumConverter<ShardTransferMethod>))]
    public ShardTransferMethod Method { init; get; }

    /// <summary>
    /// Target shard ID if different than source shard ID.
    /// Used exclusively with <see cref="ShardTransferMethod.ReshardingStreamRecords"/> transfer method.
    /// </summary>
    public uint? ToShardId { init; get; }

    /// <summary>
    /// A human-readable report of the transfer progress.
    /// Available only on the source peer.
    /// </summary>
    public string Comment { init; get; }
}
