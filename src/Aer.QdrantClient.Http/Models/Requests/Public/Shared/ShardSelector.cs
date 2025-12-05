using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// The shard selector. Can select shards by string or integer shard key values.
/// </summary>
public abstract class ShardSelector
{
    /// <summary>
    /// Represents a shard selector using string shard keys.
    /// </summary>
    /// <remarks>
    /// Initializes new instance of <see cref="ShardSelector"/> using string shard key.
    /// </remarks>
    internal sealed class StringShardKeyShardSelector : ShardSelector
    {
        /// <summary>
        /// Shard key values.
        /// </summary>
        public string[] ShardKeyValues { get; internal init; }

        /// <summary>
        /// Shard key value when only one shard key selected.
        /// </summary>
        public string ShardKeyValue { get; internal init; }

        /// <summary>
        /// Fallback shard key value. If the shard with <see cref="ShardKeyValue"/>
        /// is not found, the request is routed to the fallback shard.
        /// </summary>
        public ShardKey FallbackShardKeyValue { get; internal init; }
    }

    /// <summary>
    /// Represents a shard selector using integer shard keys.
    /// </summary>
    /// <remarks>
    /// Initializes new instance of <see cref="ShardSelector"/> using integer shard key.
    /// </remarks>
    internal sealed class IntegerShardKeyShardSelector : ShardSelector
    {
        /// <summary>
        /// Shard key values.
        /// </summary>
        public ulong[] ShardKeyValues { get; internal init; }

        /// <summary>
        /// Shard key value when only one shard key selected.
        /// </summary>
        public ulong? ShardKeyValue { get; internal init; }

        /// <summary>
        /// Fallback shard key value. If the shard with <see cref="ShardKeyValue"/>
        /// is not found, the request is routed to the fallback shard.
        /// </summary>
        public ShardKey FallbackShardKeyValue { get; internal init; }
    }

    #region Factory methods

    /// <summary>
    /// Creates a shard key selector using a single string shard key value.
    /// </summary>
    /// <param name="shardKeyValue">The shard key value.</param>
    /// <param name="fallbackShardKeyValue">Fallback shard key value. If the shard with <paramref name="shardKeyValue"/>
    /// is not found, the request is routed to the fallback shard.</param>
    public static ShardSelector String(string shardKeyValue, ShardKey fallbackShardKeyValue = null) =>
        new StringShardKeyShardSelector()
        {
            ShardKeyValue = shardKeyValue,
            FallbackShardKeyValue = fallbackShardKeyValue
        };

    /// <summary>
    /// Creates a shard key selector using string shard key values.
    /// </summary>
    /// <param name="shardKeyValues">The shard key values.</param>
    public static ShardSelector String(params string[] shardKeyValues) =>
        new StringShardKeyShardSelector()
        {
            ShardKeyValues = shardKeyValues
        };

    /// <summary>
    /// Creates a shard key selector using integer shard key value.
    /// </summary>
    /// <param name="shardKeyValue">The shard key value.</param>
    /// <param name="fallbackShardKeyValue">Fallback shard key value. If the shard with <paramref name="shardKeyValue"/>
    /// is not found, the request is routed to the fallback shard.</param>
    public static ShardSelector Integer(ulong shardKeyValue, ShardKey fallbackShardKeyValue = null) =>
        new IntegerShardKeyShardSelector()
        {
            ShardKeyValue = shardKeyValue,
            FallbackShardKeyValue = fallbackShardKeyValue
        };

    /// <summary>
    /// Creates a shard key selector using integer shard key values.
    /// </summary>
    /// <param name="shardKeyValues">The shard key values.</param>
    public static ShardSelector Integer(params ulong[] shardKeyValues) =>
        new IntegerShardKeyShardSelector()
        {
            ShardKeyValues = shardKeyValues
        };

    #endregion

    #region Operators

    /// <summary>
    /// Performs an implicit conversion from <see cref="ulong"/> to <see cref="ShardSelector"/>.
    /// </summary>
    /// <param name="shardKeyValue">The shard key value.</param>
    public static implicit operator ShardSelector(ulong shardKeyValue) => Integer(shardKeyValue);

    /// <summary>
    /// Performs an implicit conversion from <see cref="ulong"/> array to <see cref="ShardSelector"/>.
    /// </summary>
    /// <param name="shardKeyValues">The shard key values.</param>
    public static implicit operator ShardSelector(ulong[] shardKeyValues)
    {
        if (shardKeyValues is null or { Length: 0 })
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

        return Integer(checked((ulong)shardKeyValue));
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="string"/> to <see cref="ShardSelector"/>.
    /// </summary>
    /// <param name="shardKeyValue">The shard key value.</param>
    public static implicit operator ShardSelector(string shardKeyValue)
    {
        if (string.IsNullOrEmpty(shardKeyValue))
        {
            throw new ArgumentNullException(nameof(shardKeyValue));
        }

        return String(shardKeyValue);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="string"/> array to <see cref="ShardSelector"/>.
    /// </summary>
    /// <param name="shardKeyValues">The shard key values.</param>
    public static implicit operator ShardSelector(string[] shardKeyValues)
    {
        if (shardKeyValues is null or { Length: 0 })
        {
            throw new ArgumentNullException(nameof(shardKeyValues));
        }

        return String(shardKeyValues);
    }

    #endregion
}
