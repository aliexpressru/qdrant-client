namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents a string shard key value.
/// </summary>
internal class StringShardKey : ShardKey
{
    /// <summary>
    /// The shard key value.
    /// </summary>
    public string ShardKeyValue { get; }

    /// <summary>
    /// Initializes new instance of <see cref="ShardKey"/> using string shard key.
    /// </summary>
    /// <param name="shardKeyValue">The shard key value.</param>
    public StringShardKey(string shardKeyValue)
    {
        ShardKeyValue = shardKeyValue;
    }

    /// <inheritdoc/>
    public override bool IsInteger() => false;

    /// <inheritdoc/>
    public override bool IsString() => true;

    /// <inheritdoc/>
    public override ulong GetInteger()
        => throw new InvalidCastException($"Can't cast string shard key value {ShardKeyValue} to integer");

    /// <inheritdoc/>
    public override string GetString() => ShardKeyValue;
}
