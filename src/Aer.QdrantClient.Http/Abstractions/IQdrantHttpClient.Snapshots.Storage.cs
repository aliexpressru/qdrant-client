using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Abstractions;

public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Returns a list of all snapshots for the entire storage.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<ListSnapshotsResponse> ListStorageSnapshots(
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new snapshot of the entire storage.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<CreateSnapshotResponse> CreateStorageSnapshot(
        CancellationToken cancellationToken,
        bool isWaitForResult = true);

    /// <summary>
    /// Download specified snapshot of the whole storage as a file stream.
    /// </summary>
    /// <param name="snapshotName">Name of the snapshot to download.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    /// Full storage snapshot is a .tar file with each collection having its own snapshot inside.
    /// Alongside it the config.json maps snapshots to collections.
    /// </remarks>
    Task<DownloadSnapshotResponse> DownloadStorageSnapshot(
        string snapshotName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Delete snapshot of the whole storage.
    /// </summary>
    /// <param name="snapshotName">Name of the snapshot to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<DefaultOperationResponse> DeleteStorageSnapshot(
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true);

    /// <summary>
    /// Recover the whole storage data from snapshot by its name. This will overwrite any data, stored on
    /// this node, for the collection. If collection does not exist - it will be created.
    /// The snapshot path should be <c>/qdrant/snapshots/{snapshotName}</c> on the Qdrant node.
    /// </summary>
    /// <param name="snapshotName">The name of the local snapshot file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverStorageFromSnapshot(
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null);

    /// <summary>
    /// Recover the whole storage data from a possibly remote snapshot.
    /// </summary>
    /// <param name="snapshotLocationUri">The snapshot location in URI format. Can be either a URL or a <c>file:///</c> path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverStorageFromSnapshot(
        Uri snapshotLocationUri,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null);

    /// <summary>
    /// Recover the whole storage from an uploaded snapshot.
    /// </summary>
    /// <param name="snapshotContent">The snapshot content stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverStorageFromUploadedSnapshot(
        Stream snapshotContent,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null);
}
