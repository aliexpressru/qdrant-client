using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Responses.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using static Aer.QdrantClient.Http.Models.Responses.GetClusterTelemetryResponse;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the response of the Qdrant cluster-wide telemetry collector.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class GetClusterTelemetryResponse : QdrantResponseBase<ClusterTelemetryInfo>
{
    /// <summary>
    /// Top-level result object returned by GET /cluster/telemetry.
    /// </summary>
    public class ClusterTelemetryInfo
    {
        /// <summary>
        /// Collections telemetry.
        /// </summary>
        public Dictionary<string, CollectionTelemetry> Collections { get; set; }

        /// <summary>
        /// Cluster telemetry.
        /// </summary>
        public ClusterTelemetry Cluster { get; set; }
    }

    /// <summary>
    /// A single collection telemetry.
    /// </summary>
    public class CollectionTelemetry
    {
        /// <summary>
        /// The collection name.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Telemetry for each collection shard.
        /// Note that array indexes do not correspond to shard indexes.
        /// </summary>
        public ShardInfo[] Shards { get; set; }

        /// <summary>
        /// Ongoing resharding operations.
        /// </summary>
        public ReshardingOperationInfo[] Reshardings { get; set; }

        /// <summary>
        /// Ongoing shard transfers.
        /// </summary>
        public ShardTransferInfo[] ShardTransfers { get; set; }

        /// <summary>Telemetry for one replica set, which corresponds to a single logical shard.</summary>
        public class ShardInfo
        {
            /// <summary>
            /// The numeric identifier of this shard.
            /// </summary>
            public uint Id { get; set; }

            /// <summary>
            /// Replica information.
            /// </summary>
            public ReplicaInfo[] Replicas { get; set; }

            /// <summary>
            /// User-defined sharding key associated with this shard, when custom sharding is used.
            /// May be a <see cref="string"/> or an <see cref="int"/>.
            /// </summary>
            [JsonConverter(typeof(ShardKeyJsonConverter))]
            public ShardKey Key { get; set; }

            /// <summary>
            /// Telemetry for the replica of a shard.
            /// </summary>
            public class ReplicaInfo
            {
                /// <summary>The peer ID of the node hosting this remote replica.</summary>
                public ulong PeerId { get; set; }

                /// <summary>
                /// State of the single shard within a replica set.
                /// </summary>
                public ShardState State { get; set; }

                /// <summary>
                /// Health status of this local shard.
                /// </summary>
                public QdrantCollectionStatus Status { get; set; }

                /// <summary>
                /// Total number of points that have been fully optimized.
                /// </summary>
                public long TotalOptimizedPoints { get; set; }

                /// <summary>
                /// Estimated vectors size in bytes.
                /// </summary>
                public long VectorsSizeBytes { get; set; }

                /// <summary>
                /// Estimated payloads size in bytes.
                /// </summary>
                public long PayloadsSizeBytes { get; set; }

                /// <summary>
                /// Approximate number of points.
                /// </summary>
                public long NumPoints { get; set; }

                /// <summary>
                /// Approximate number of vectors.
                /// </summary>
                public long NumVectors { get; set; }

                /// <summary>
                /// Approximate number of vectors by name.
                /// </summary>
                public Dictionary<string, long> NumVectorsByName { get; set; }

                /// <summary>
                /// Shard cleaning task status.
                /// After a resharding, a cleanup task is performed to remove outdated points from this shard.
                /// TODO: expand to class.
                /// </summary>
                public JsonObject ShardCleaningStatus { get; set; }

                /// <summary>
                /// Partial snapshot information.
                /// </summary>
                public PartialSnapshotInfo PartialSnapshot { get; set; }

                /// <summary>
                /// Snapshot creation and recovery activity for a single collection.
                /// </summary>
                public class PartialSnapshotInfo
                {
                    /// <summary>
                    /// The number of ongoing create snapshot requests.
                    /// </summary>
                    public int OngoingCreateSnapshotRequests { get; set; }

                    /// <summary>
                    /// Is this shard recovering from a snapshot.
                    /// </summary>
                    public bool IsRecovering { get; set; }

                    /// <summary>
                    /// The snapshot recovery timestamp.
                    /// </summary>
                    public ulong RecoveryTimestamp { get; set; }
                }
            }
        }
    }

    /// <summary>
    /// Represents luster-level telemetry.
    /// </summary>
    public class ClusterTelemetry
    {
        /// <summary>
        /// Whether distributed (cluster) mode is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Number of peers in the cluster.
        /// </summary>
        public ulong NumberOfPeers { get; set; }

        /// <summary>
        /// All known peers in the cluster, keyed by peer ID string.
        /// </summary>
        public Dictionary<string, PeerInfo> Peers { get; set; }

        /// <summary>Basic information about a single cluster peer.</summary>
        public class PeerInfo
        {
            /// <summary>
            /// URI of the peer.
            /// </summary>
            public string Uri { get; set; }

            /// <summary>
            /// Whether this peer responded for this request.
            /// </summary>
            public bool Responsive { get; init; }

            /// <summary>
            /// If <see cref="Responsive"/> is <c>true</c>, these details should be available.
            /// </summary>
            public DistributedPeerDetails Details { get; init; }

            /// <summary>
            /// Represents the single cluster peer info.
            /// </summary>
            public class DistributedPeerDetails
            {
                /// <summary>
                /// The Qdrant version running on this peer.
                /// </summary>
                public string Version { get; set; }

                /// <summary>
                /// Whether this node has voting rights in the current Raft quorum.
                /// </summary>
                public bool IsVoter { get; set; }

                /// <summary>
                /// Current Raft term number.
                /// </summary>
                public long Term { get; set; }

                /// <summary>
                /// Index of the last committed Raft log entry.
                /// </summary>
                public long Commit { get; set; }

                /// <summary>
                /// Number of consensus operations that are queued but not yet committed.
                /// </summary>
                public int PendingOperations { get; set; }

                /// <summary>
                /// The Raft role this node is currently playing (<c>null</c> when role is not yet determined).
                /// </summary>
                public PeerRole Role { get; set; }

                /// <summary>
                /// Current operational status of the background consensus thread.
                /// </summary>
                public ConsensusThreadState ConsensusThreadStatus { get; set; }
            }
        }
    }
}
