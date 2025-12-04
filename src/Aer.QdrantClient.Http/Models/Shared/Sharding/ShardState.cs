namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// State of the single shard within a replica set.
/// </summary>
public enum ShardState
{
    /// <summary>
    /// Shard is active.
    /// </summary>
    Active,

    /// <summary>
    /// Shard is inaccessible.
    /// </summary>
    Dead,

    /// <summary>
    /// Shard is partial.
    /// </summary>
    Partial,

    /// <summary>
    /// Shard is initializing.
    /// </summary>
    Initializing,

    /// <summary>
    /// Shard is in listener mode.
    /// </summary>
    Listener,

    /// <summary>
    /// Shard is in the process of being transferred to another node.
    /// </summary>
    PartialSnapshot,

    /// <summary>
    /// Shard is in the process of being recovered.
    /// </summary>
    Recovery,

    /// <summary>
    /// Shard is in the process of being resharded.
    /// </summary>
    Resharding,

    /// <summary>
    /// Shard is in the process of being resharded scaling down.
    /// </summary>
    ReshardingScaleDown,

    /// <summary>
    /// Shard is active and read-only.
    /// </summary>
    ActiveRead
}
