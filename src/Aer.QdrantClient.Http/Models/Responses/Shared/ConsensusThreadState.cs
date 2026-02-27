namespace Aer.QdrantClient.Http.Models.Responses.Shared;

/// <summary>
/// Represents consensus thread state.
/// </summary>
public sealed class ConsensusThreadState
{
    /// <summary>
    /// The consensus thread status.
    /// </summary>
    public ConsensusThreadStatus ConsensusThreadStatus { init; get; }

    /// <summary>
    /// The consensus status last update.
    /// </summary>
    public DateTimeOffset LastUpdate { init; get; }

    /// <summary>
    /// The consensus status error.
    /// </summary>
    public string Err { init; get; }
}
