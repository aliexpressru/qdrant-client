using Aer.QdrantClient.Http.Abstractions;

namespace Aer.QdrantClient.Http.Infrastructure.Snapshots;

/// <summary>
/// Represents a collection recovery on a single peer.
/// </summary>
/// <param name="PeerClient">The target qdrant peer node client.</param>
/// <param name="SnapshotLocationUri">The snapshot to recover collection from.</param>
/// <param name="SnapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
public record ScheduledPeerCollectionRecovery(
    IQdrantHttpClient PeerClient,
    Uri SnapshotLocationUri,
    string SnapshotChecksum = null);
