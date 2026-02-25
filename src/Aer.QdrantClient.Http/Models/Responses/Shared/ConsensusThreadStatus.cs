namespace Aer.QdrantClient.Http.Models.Responses.Shared;

/// <summary>
/// Represents the consensus thread status.
/// </summary>
public enum ConsensusThreadStatus
{
    /// <summary>
    /// Consensus thread is active.
    /// </summary>
    Working,

    /// <summary>
    /// Consensus thread is stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// Consensus thread is stopped with error.
    /// </summary>
    StoppedWithErr
}
