namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The type of the qdrant optimizer status.
/// </summary>
public enum QdrantOptimizerStatus
{
    /// <summary>
    /// Represents the unknown qdrant collection optimizer status. Might indicate an exception or a bug.
    /// </summary>
    Unknown,

    /// <summary>
    /// Represents the OK qdrant collection optimizer status.
    /// </summary>
    Ok,

    /// <summary>
    /// Represents the error qdrant collection optimizer status.
    /// </summary>
    Error
}
