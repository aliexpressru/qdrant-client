using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Http;

/// <summary>
/// Interface for Qdrant HTTP API client.
/// </summary>
public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Returns a list of all snapshots for a shard from a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to get a snapshot list.</param>
    /// <param name="shardId">Id of the shard for which to list snapshots.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<ListSnapshotsResponse> ListShardSnapshots(
        string collectionName,
        uint shardId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Create new snapshot of a shard for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to create a snapshot.</param>
    /// <param name="shardId">Id of the shard for which to create a snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<CreateSnapshotResponse> CreateShardSnapshot(
        string collectionName,
        uint shardId,
        CancellationToken cancellationToken,
        bool isWaitForResult);

    /// <summary>
    /// Recover shard of a local collection data from a snapshot.
    /// This will overwrite any data, stored in this shard, for the collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="shardId">Id of the shard which to recover from a snapshot.</param>
    /// <param name="snapshotLocationUri">The snapshot location in URI format. File scheme uris are not supported by qdrant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverShardFromSnapshot(
        string collectionName,
        uint shardId,
        Uri snapshotLocationUri,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        SnapshotPriority? snapshotPriority,
        string snapshotChecksum);

    /// <summary>
    /// Recover shard of a local collection from an uploaded snapshot.
    /// This will overwrite any data, stored on this node, for the collection shard.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="shardId">Id of the shard which to recover from a snapshot.</param>
    /// <param name="snapshotContent">The snapshot content stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    Task<DefaultOperationResponse> RecoverShardFromUploadedSnapshot(
        string collectionName,
        uint shardId,
        Stream snapshotContent,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        SnapshotPriority? snapshotPriority,
        string snapshotChecksum);

    /// <summary>
    /// Downloads the specified snapshot of a shard from a collection as a file stream.
    /// </summary>
    /// <param name="collectionName">Name of the collection to download snapshot for.</param>
    /// <param name="shardId">Id of the shard for which to download snapshot.</param>
    /// <param name="snapshotName">Name of the snapshot to download.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<DownloadSnapshotResponse> DownloadShardSnapshot(
        string collectionName,
        uint shardId,
        string snapshotName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified snapshot of a shard from a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete shard snapshot for.</param>
    /// <param name="shardId">Id of the shard for which to delete snapshot.</param>
    /// <param name="snapshotName">Name of the snapshot to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<DefaultOperationResponse> DeleteShardSnapshot(
        string collectionName,
        uint shardId,
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult);

}
