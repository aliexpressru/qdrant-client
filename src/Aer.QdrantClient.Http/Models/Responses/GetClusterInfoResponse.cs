﻿using Aer.QdrantClient.Http.Models.Responses.Base;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents information about current cluster status and structure.
/// </summary>
public sealed class GetClusterInfoResponse : QdrantResponseBase<GetClusterInfoResponse.ClusterInfo>
{
    /// <summary>
    /// Represents information about current cluster status and structure.
    /// </summary>
    public class ClusterInfo
    {
        /// <summary>
        /// Current scluster status <c>enabled</c>, <c>disabled</c>.
        /// </summary>
        public string Status { set; get; }

        /// <summary>
        /// This node peer identifier.
        /// </summary>
        public ulong PeerId { set; get; }

        /// <summary>
        /// All other cluster nodes peer information.
        /// </summary>
        public Dictionary<string, PeerInfoUint> Peers { set; get; }

        /// <summary>
        /// The nodes consensus status.
        /// </summary>
        public RaftInfoUnit RaftInfo { set; get; }

        /// <summary>
        /// Information about current consensus thread status.
        /// </summary>
        public ConsensusThreadStatusUnit ConsensusThreadStatus { set; get; }

        /// <summary>
        /// Consequent failures of message send operations in consensus by peer address.
        /// On the first success to send to that peer - entry is removed from this hashmap.
        /// </summary>
        public Dictionary<string, MessageSendFailureUnit> MessageSendFailures { set; get; }
    }

    /// <summary>
    /// Represents a consensus message send failure information.
    /// </summary>
    public class MessageSendFailureUnit
    {
        /// <summary>
        /// Failures count.
        /// </summary>
        public uint Count { set; get; }

        /// <summary>
        /// The latest message send error.
        /// </summary>
        public string LatestError { set; get; }
    }

    /// <summary>
    /// Represents a consensus status information.
    /// </summary>
    public class ConsensusThreadStatusUnit
    {
        /// <summary>
        /// The consensus thread status.
        /// TODO: convert to enum
        /// </summary>
        public string ConsensusThreadStatus { set; get; }

        /// <summary>
        /// The consensus status last update.
        /// </summary>
        public DateTime LastUpdate { set; get; }

        /// <summary>
        /// The consensus status error.
        /// </summary>
        public string Err { set; get; }
    }

    /// <summary>
    /// Represents one cluster node information.
    /// </summary>
    public class PeerInfoUint
    {
        /// <summary>
        /// The peer URI.
        /// </summary>
        public string Uri { set; get; }
    }

    /// <summary>
    /// Represents RAFT consensus protocol status.
    /// </summary>
    public class RaftInfoUnit
    {
        /// <summary>
        /// The term number.
        /// </summary>
        public uint Term { set; get; }

        /// <summary>
        /// The commit number.
        /// </summary>
        public uint Commit { set; get; }

        /// <summary>
        /// Number of pending operations.
        /// </summary>
        public uint PendingOperations { set; get; }

        /// <summary>
        /// The cluster consensus leader peer id.
        /// </summary>
        public ulong Leader { set; get; }

        /// <summary>
        /// This node role.
        /// </summary>
        public string Role { set; get; }

        /// <summary>
        /// Is this peer a voter or a learner.
        /// </summary>
        public bool IsVoter { set; get; }
    }
}
