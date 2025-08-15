namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents the qdrant operation status type.
/// </summary>
public enum QdrantOperationStatusType
{
    /// <summary>
    /// Represents the unknown qdrant operation status with raw status string.
    /// Might indicate an exception or a bug.
    /// </summary>
    Unknown,

    /// <summary>
    /// Represents the successful qdrant operation status.
    /// </summary>
    Ok,

    /// <summary>
    /// Represents the error qdrant operation status with specified error.
    /// </summary>
    Error
}
