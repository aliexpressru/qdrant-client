namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

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

    #region Factory methods

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

    #endregion

    #region Operators

    /// <summary>
    /// Performs an implicit conversion from <see cref="ulong"/> to <see cref="ShardSelector"/>.
    /// </summary>
    /// <param name="shardKeyValue">The shard key value.</param>
    public static implicit operator ShardSelector(ulong shardKeyValue)
    {
        return Integer(shardKeyValue);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="ulong"/> array to <see cref="ShardSelector"/>.
    /// </summary>
    /// <param name="shardKeyValues">The shard key values.</param>
    public static implicit operator ShardSelector(ulong[] shardKeyValues)
    {
        if (shardKeyValues is null or {Length: 0})
        {
            throw new ArgumentNullException(nameof(shardKeyValues));
        }

        return Integer(shardKeyValues);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="int"/> to <see cref="ShardSelector"/>.
    /// </summary>
    /// <param name="shardKeyValue">The shard key value.</param>
    public static implicit operator ShardSelector(int shardKeyValue)
    {
        if (shardKeyValue < 0)
        {
            throw new InvalidOperationException($"Can't use negative integer {shardKeyValue} as shard key selector value");
        }

        return Integer(checked((ulong) shardKeyValue));
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="String"/> to <see cref="ShardSelector"/>.
    /// </summary>
    /// <param name="shardKeyValue">The shard key value.</param>
    public static implicit operator ShardSelector(string shardKeyValue)
    {
        ArgumentException.ThrowIfNullOrEmpty(shardKeyValue);

        return String(shardKeyValue);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="String"/> array to <see cref="ShardSelector"/>.
    /// </summary>
    /// <param name="shardKeyValues">The shard key values.</param>
    public static implicit operator ShardSelector(string[] shardKeyValues)
    {
        if (shardKeyValues is null or {Length: 0})
        {
            throw new ArgumentNullException(nameof(shardKeyValues));
        }

        return String(shardKeyValues);
    }

    #endregion
}
