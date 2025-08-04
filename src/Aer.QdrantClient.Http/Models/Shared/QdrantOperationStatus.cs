namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a qdrant operation status.
/// </summary>
public enum QdrantOperationStatus
{
    /// <summary>
    /// Operation is successfully completed.
    /// </summary>
    Ok,

    /// <summary>
    /// Async operation is acknowledged and will complete in the future.
    /// </summary>
    Acknowledged,

    /// <summary>
    /// Operation is successfully completed synchronously (for data manipulation operations).
    /// </summary>
    Completed
}

