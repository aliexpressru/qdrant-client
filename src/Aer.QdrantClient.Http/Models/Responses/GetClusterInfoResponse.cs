using Aer.QdrantClient.Http.Models.Responses.Base;

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
    }
}
