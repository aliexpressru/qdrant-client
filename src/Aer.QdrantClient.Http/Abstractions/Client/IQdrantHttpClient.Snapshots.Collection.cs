using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Abstractions;

public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Get list of snapshots for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to get a snapshot list.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<ListSnapshotsResponse> ListCollectionSnapshots(
        string collectionName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Create new snapshot for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to create a snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<CreateSnapshotResponse> CreateCollectionSnapshot(
        string collectionName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true);

    /// <summary>
    /// Recover local collection data from a local snapshot by its name. This will overwrite any data, stored on
    /// this node, for the collection. If collection does not exist - it will be created.
    /// The snapshot path should be <c>/qdrant/snapshots/{collectionName}/{snapshotName}</c> on the Qdrant node.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="snapshotName">The name of the local snapshot file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverCollectionFromSnapshot(
        string collectionName,
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null);

    /// <summary>
    /// Recover local collection data from a possibly remote snapshot. This will overwrite any data, stored on
    /// this node, for the collection. If collection does not exist - it will be created.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="snapshotLocationUri">The snapshot location in URI format. Can be either a URL or a <c>file:///</c> path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverCollectionFromSnapshot(
        string collectionName,
        Uri snapshotLocationUri,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null);

    /// <summary>
    /// Recover local collection data from an uploaded snapshot. This will overwrite any data,
    /// stored on this node, for the collection. If collection does not exist - it will be created.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="snapshotContent">The snapshot content stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverCollectionFromUploadedSnapshot(
        string collectionName,
        Stream snapshotContent,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null);

    /// <summary>
    /// Download specified snapshot from a collection as a file stream.
    /// </summary>
    /// <param name="collectionName">Name of the collection to download snapshot for.</param>
    /// <param name="snapshotName">Name of the snapshot to download.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<DownloadSnapshotResponse> DownloadCollectionSnapshot(
        string collectionName,
        string snapshotName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Delete snapshot for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete snapshot for.</param>
    /// <param name="snapshotName">Name of the snapshot to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<DefaultOperationResponse> DeleteCollectionSnapshot(
        string collectionName,
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult);
}
