namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents an integer shard key value.
/// </summary>
/// <remarks>
/// Initializes new instance of <see cref="ShardKey"/> using integer shard key.
/// </remarks>
/// <param name="shardKeyValue">The shard key value.</param>
internal sealed class IntegerShardKey(ulong shardKeyValue) : ShardKey
{
    /// <summary>
    /// The shard key value.
    /// </summary>
    public ulong ShardKeyValue { get; } = shardKeyValue;

    /// <inheritdoc/>
    public override bool IsInteger() => true;

    /// <inheritdoc/>
    public override bool IsString() => false;

    /// <inheritdoc/>
    public override ulong GetInteger() => ShardKeyValue;

    /// <inheritdoc/>
    public override string GetString() =>
        throw new InvalidCastException($"Can't cast integer shard key value {ShardKeyValue} to string");
}
