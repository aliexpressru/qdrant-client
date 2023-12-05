namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Defines source of truth for snapshot recovery.
/// </summary>
public enum SnapshotPriority
{
    /// <summary>
    /// Restore snapshot without any additional synchronization.
    /// </summary>
    NoSync,

    /// <summary>
    /// Prefer snapshot data over the current state.
    /// </summary>
    Snapshot,

    /// <summary>
    /// Prefer existing data over the snapshot.
    /// </summary>
    Replica
}
