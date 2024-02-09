namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The shard selector. Can select shards by string or integer shard key values.
/// </summary>
public abstract class ShardSelector
{
    /// <summary>
    /// Represents a shard selector using string shard keys.
    /// </summary>
    internal class StringShardKeyShardSelector : ShardSelector
    {
        /// <summary>
        /// Shard key value.
        /// </summary>
        public string[] ShardKeyValues { get; }

        /// <summary>
        /// Initializes new istance of <see cref="ShardSelector"/> using string shard key.
        /// </summary>
        /// <param name="shardKeyValues">The shard key values.</param>
        public StringShardKeyShardSelector(string[] shardKeyValues)
        {
            ShardKeyValues = shardKeyValues;
        }
    }

    /// <summary>
    /// Represents a shard selector using integer shard keys.
    /// </summary>
    internal class IntegerShardKeyShardSelector : ShardSelector
    {
        /// <summary>
        /// Shard key value.
        /// </summary>
        public ulong[] ShardKeyValues { get; }

        /// <summary>
        /// Initializes new istance of <see cref="ShardSelector"/> using integer shard key.
        /// </summary>
        /// <param name="shardKeyValues">The shard key value.</param>
        public IntegerShardKeyShardSelector(params ulong[] shardKeyValues)
        {
            ShardKeyValues = shardKeyValues;
        }
    }

    /// <summary>
    /// Creates a shard key selector using string shard key values.
    /// </summary>
    /// <param name="shardKeyValues">The shard key values.</param>
    public static ShardSelector String(params string[] shardKeyValues)
        => new StringShardKeyShardSelector(shardKeyValues);

    /// <summary>
    /// Creates a shard key selector using integer shard key values.
    /// </summary>
    /// <param name="shardKeyValues">The shard key values.</param>
    public static ShardSelector Integer(params ulong[] shardKeyValues)
        => new IntegerShardKeyShardSelector(shardKeyValues);
}
