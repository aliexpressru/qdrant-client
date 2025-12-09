using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents information about current cluster status and structure.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class GetClusterInfoResponse : QdrantResponseBase<GetClusterInfoResponse.ClusterInfo>
{
    /// <summary>
    /// Represents information about current cluster status and structure.
    /// </summary>
    public sealed class ClusterInfo
    {
        /// <summary>
        /// Current cluster status <c>enabled</c>, <c>disabled</c>.
        /// </summary>
        public string Status { init; get; }

        /// <summary>
        /// This node peer identifier.
        /// </summary>
        public ulong PeerId { init; get; }

        /// <summary>
        /// All cluster nodes peer information by string peer ids.
        /// </summary>
        public Dictionary<string, PeerInfoUint> Peers { init; get; }

        /// <summary>
        /// All cluster nodes peer information by parsed ulong peer ids.
        /// </summary>
        public Dictionary<ulong, PeerInfoUint> ParsedPeers =>
            Peers.ToDictionary(
                p => ulong.Parse(p.Key),
                p => p.Value);

        /// <summary>
        /// All peer ids.
        /// </summary>
        public List<ulong> AllPeerIds =>
            [.. Peers.Select(x => ulong.Parse(x.Key))];

        /// <summary>
        /// The nodes consensus status.
        /// </summary>
        public RaftInfoUnit RaftInfo { init; get; }

        /// <summary>
        /// Information about current consensus thread status.
        /// </summary>
        public ConsensusThreadStatusUnit ConsensusThreadStatus { init; get; }

        /// <summary>
        /// Consequent failures of message send operations in consensus by peer address.
        /// On the first success to send to that peer - entry is removed from this hashmap.
        /// </summary>
        public Dictionary<string, MessageSendFailureUnit> MessageSendFailures { init; get; }
    }

    /// <summary>
    /// Represents a consensus message send failure information.
    /// </summary>
    public sealed class MessageSendFailureUnit
    {
        /// <summary>
        /// Failures count.
        /// </summary>
        public uint Count { init; get; }

        /// <summary>
        /// The latest message send error.
        /// </summary>
        public string LatestError { init; get; }

        /// <summary>
        /// The latest error timestamp.
        /// </summary>
        public DateTime LatestErrorTimestamp { init; get; }
    }

    /// <summary>
    /// Represents a consensus status information.
    /// </summary>
    public sealed class ConsensusThreadStatusUnit
    {
        /// <summary>
        /// The consensus thread status.
        /// TODO: convert to enum
        /// </summary>
        public string ConsensusThreadStatus { init; get; }

        /// <summary>
        /// The consensus status last update.
        /// </summary>
        public DateTime LastUpdate { init; get; }

        /// <summary>
        /// The consensus status error.
        /// </summary>
        public string Err { init; get; }
    }

    /// <summary>
    /// Represents one cluster node information.
    /// </summary>
    public sealed class PeerInfoUint
    {
        /// <summary>
        /// The peer URI.
        /// </summary>
        public string Uri { init; get; }
    }

    /// <summary>
    /// Represents RAFT consensus protocol status.
    /// </summary>
    public sealed class RaftInfoUnit
    {
        /// <summary>
        /// The term number.
        /// </summary>
        public uint Term { init; get; }

        /// <summary>
        /// The commit number.
        /// </summary>
        public uint Commit { init; get; }

        /// <summary>
        /// Number of pending operations.
        /// </summary>
        public uint PendingOperations { init; get; }

        /// <summary>
        /// The cluster consensus leader peer id.
        /// </summary>
        public ulong? Leader { init; get; }

        /// <summary>
        /// This node role.
        /// </summary>
        public string Role { init; get; }

        /// <summary>
        /// Is this peer a voter or a learner.
        /// </summary>
        public bool IsVoter { init; get; }
    }
}
