using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Requests;

/// <summary>
/// Request for recovering a single collection or collection shard from a snapshot.
/// </summary>
internal class RecoverEntityFromSnapshotRequest
{
    /// <summary>
    /// Snapshot location.
    /// Examples:
    /// <ul>
    /// <li>URL <c>http://localhost:8080/collections/my_collection/snapshots/my_snapshot</c></li>
    /// <li>Local path <c>file:///qdrant/snapshots/test_collection-2022-08-04-10-49-10.snapshot</c></li>
    /// </ul>
    /// </summary>
    public Uri Location { get; }

    /// <summary>
    /// Defines which data should be used as a source of truth if there are other replicas
    /// in the cluster. If set to <see cref="SnapshotPriority.Snapshot"/>, the snapshot
    /// will be used as a source of truth, and the current state will be overwritten.
    /// If set to <see cref="SnapshotPriority.Replica"/>, the current state will be used
    /// as a source of truth, and after recovery if will be synchronized with the snapshot.
    /// </summary>
    public SnapshotPriority? Priority { set; get; }

    public RecoverEntityFromSnapshotRequest(Uri location)
    {
        Location = location;
    }
}
