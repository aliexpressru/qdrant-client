namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represemts an integer shard key value.
/// </summary>
internal class IntegerShardKey : ShardKey
{
    /// <summary>
    /// The shard key value.
    /// </summary>
    public ulong ShardKeyValue { get; }

    /// <summary>
    /// Initializes new istance of <see cref="ShardKey"/> using integer shard key.
    /// </summary>
    /// <param name="shardKeyValue">The shard key value.</param>
    public IntegerShardKey(ulong shardKeyValue)
    {
        ShardKeyValue = shardKeyValue;
    }

    /// <inheritdoc/>
    public override bool IsInteger() => true;

    /// <inheritdoc/>
    public override bool IsString() => false;

    /// <inheritdoc/>
    public override ulong GetInteger() => ShardKeyValue;

    /// <inheritdoc/>
    public override string GetString()
        => throw new InvalidCastException($"Can't cast integer shard key value {ShardKeyValue} to string");
}
