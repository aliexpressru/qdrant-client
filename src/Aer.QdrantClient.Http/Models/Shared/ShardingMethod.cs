using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The collection sharding method.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public enum ShardingMethod
{
    /// <summary>
    /// In this mode, the shard_number means the number of shards per shard key,
    /// where points will be distributed evenly.
    /// </summary>
    /// <remarks>
    /// For example, if you have 10 shard keys and a collection config with these settings:
    /// <code>
    /// {
    /// "shard_number": 1,
    /// "sharding_method": "custom",
    /// "replication_factor": 2
    /// }
    /// </code>
    /// Then you will have 1 * 10 * 2 = 20 total physical shards in the collection.
    /// </remarks>
    Custom
}
