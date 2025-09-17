namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// The type of the snapshot. Used in compound operation results to distinguish between different snapshots returned.
/// </summary>
public enum SnapshotType
{
    /// <summary>
    /// Unknown snapshot type.
    /// This type is returned by non-compound snapshot-related operations.
    /// </summary>
    Unspecified,
    
    /// <summary>
    /// Collection snapshot.
    /// </summary>
    Collection,
    
    /// <summary>
    /// Shard snapshot.
    /// </summary>
    Shard,
    
    /// <summary>
    /// Entire storage snapshot.
    /// </summary>
    Storage,
}
