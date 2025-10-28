namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The type of the qdrant optimizer status.
/// </summary>
public enum QdrantOptimizerStatus
{
    /// <summary>
    /// Represents the unknown qdrant collection optimizer status. Might indicate an exception or a bug.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Represents the OK qdrant collection optimizer status.
    /// </summary>
    Ok = 1,

    /// <summary>
    /// Represents the error qdrant collection optimizer status.
    /// </summary>
    Error = 2
}
