using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http.Abstractions;

public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Returns a list of all snapshots for the entire storage.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<ListSnapshotsResponse> ListStorageSnapshots(
        CancellationToken cancellationToken,
        string clusterName = null);

    /// <summary>
    /// Creates a new snapshot of the entire storage.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<CreateSnapshotResponse> CreateStorageSnapshot(
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        string clusterName = null);

    /// <summary>
    /// Download specified snapshot of the whole storage as a file stream.
    /// </summary>
    /// <param name="snapshotName">Name of the snapshot to download.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    /// <remarks>
    /// Full storage snapshot is a .tar file with each collection having its own snapshot inside.
    /// Alongside it the config.json maps snapshots to collections.
    /// </remarks>
    Task<DownloadSnapshotResponse> DownloadStorageSnapshot(
        string snapshotName,
        CancellationToken cancellationToken,
        string clusterName = null);

    /// <summary>
    /// Delete snapshot of the whole storage.
    /// </summary>
    /// <param name="snapshotName">Name of the snapshot to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<DefaultOperationResponse> DeleteStorageSnapshot(
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        string clusterName = null);
}
