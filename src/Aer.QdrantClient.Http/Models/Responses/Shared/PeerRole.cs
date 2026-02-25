namespace Aer.QdrantClient.Http.Models.Responses.Shared;

/// <summary>
/// Represents a peer role in cluster.
/// </summary>
public enum PeerRole
{
    /// <summary>
    /// This node is the current Raft leader and handles all writes.
    /// </summary>
    Leader,

    /// <summary>
    /// This node is a follower that replicates entries from the leader.
    /// </summary>
    Follower,

    /// <summary>
    /// This node is campaigning to become the next leader.
    /// </summary>
    Candidate,

    /// <summary>
    /// Pre-vote phase candidate; seeking permission to start a real election.
    /// </summary>
    PreCandidate
}
