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
    Listener
}
