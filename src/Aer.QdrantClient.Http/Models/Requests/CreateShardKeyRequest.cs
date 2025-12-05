using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Requests;

/// <summary>
/// The request to create shards with specified shard key.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class CreateShardKeyRequest
{
    /// <summary>
    /// The shard key for shards to create.
    /// </summary>
    [JsonConverter(typeof(ShardKeyJsonConverter))]
    public ShardKey ShardKey { get; }

    /// <summary>
    /// How many shards to create for this key. If not specified, will use the default value from config.
    /// </summary>
    public uint? ShardsNumber { get; }

    /// <summary>
    /// How many replicas to create for each shard. If not specified, will use the default value from config.
    /// </summary>
    public uint? ReplicationFactor { get; }

    /// <summary>
    /// Placement of shards for this key - array of peer ids, that can be used to place shards for this key.
    /// If not specified, will be randomly placed among all peers.
    /// </summary>
    public ulong[] Placement { get; }

    /// <summary>
    /// Initial state of the shards for this key.
    /// If not specified, will be <see cref="ShardState.Initializing"/> first and then <see cref="ShardState.Active"/>.
    /// </summary>
    /// <remarks>
    /// Warning: do not change this unless you know what you are doing
    /// </remarks>
    public string InitialState { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CreateShardKeyRequest"/>.
    /// </summary>
    /// <param name="shardKey">The shard key for shards to create.</param>
    /// <param name="shardsNumber">How many shards to create for this key. If not specified, will use the default value from config.</param>
    /// <param name="replicationFactor">How many replicas to create for each shard. If not specified, will use the default value from config.</param>
    /// <param name="placement">
    /// Placement of shards for this key - array of peer ids, that can be used to place shards for this key.
    /// If not specified, will be randomly placed among all peers.
    /// </param>
    /// <param name="initialState">
    /// Initial state of the shards for this key.
    /// If not specified, will be <see cref="ShardState.Initializing"/> first and then <see cref="ShardState.Active"/>.
    /// Warning: do not change this unless you know what you are doing
    /// </param>
    public CreateShardKeyRequest(
        ShardKey shardKey,
        uint? shardsNumber,
        uint? replicationFactor,
        ulong[] placement,
        ShardState? initialState = null)
    {
        ShardKey = shardKey;
        ShardsNumber = shardsNumber;
        ReplicationFactor = replicationFactor;
        Placement = placement;

        // Not using enum value directly since we need it to be in PascalCase
        // and default serialization format is in snake_case
        InitialState = initialState.HasValue ? initialState.ToString() : null;
    }
}
