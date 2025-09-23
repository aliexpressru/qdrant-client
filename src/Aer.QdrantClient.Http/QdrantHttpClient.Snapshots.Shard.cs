using System.Diagnostics.CodeAnalysis;

#if  NETSTANDARD2_0
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
#endif

using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <summary>
    /// Returns a list of all snapshots for a shard from a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to get a snapshot list.</param>
    /// <param name="shardId">Id of the shard for which to list snapshots.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<ListSnapshotsResponse> ListShardSnapshots(
        string collectionName,
        uint shardId,
        CancellationToken cancellationToken)
    {
        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots";

        var response = await ExecuteRequest<ListSnapshotsResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        if (response.Result is {Length: > 0})
        {
            foreach (var snapshot in response.Result)
            {
                snapshot.SnapshotType = SnapshotType.Shard;
            }
        }

        return response;
    }

    /// <summary>
    /// Create new snapshot of a shard for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to create a snapshot.</param>
    /// <param name="shardId">Id of the shard for which to create a snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    public async Task<CreateSnapshotResponse> CreateShardSnapshot(
        string collectionName,
        uint shardId,
        CancellationToken cancellationToken,
        bool isWaitForResult = true)
    {
        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<CreateSnapshotResponse>(
            url,
            HttpMethod.Post,
            cancellationToken,
            retryCount: 0);

        if (response.Result is not null)
        {
            response.Result.SnapshotType = SnapshotType.Shard;
        }
        
        return response;
    }

    /// <summary>
    /// Recover shard of a local collection data from a snapshot.
    /// This will overwrite any data, stored in this shard, for the collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="shardId">Id of the shard which to recover from a snapshot.</param>
    /// <param name="snapshotLocationUri">The snapshot location in URI format.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksum">Optional SHA256 checksum to verify snapshot integrity before recovery.</param>
    public async Task<DefaultOperationResponse> RecoverShardFromSnapshot(
        string collectionName,
        uint shardId,
        Uri snapshotLocationUri,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null)
    {
        if (snapshotLocationUri.IsFile)
        { 
            throw new NotSupportedException("Recovering shard from file URI snapshot is not supported");
        }

        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots/recover?wait={ToUrlQueryString(isWaitForResult)}";

        var request = new RecoverEntityFromSnapshotRequest(snapshotLocationUri, snapshotPriority, snapshotChecksum);

        var response = await ExecuteRequest<RecoverEntityFromSnapshotRequest, DefaultOperationResponse>(
            url,
            HttpMethod.Put,
            request,
            cancellationToken,
            retryCount: 0);

        return response;
    }

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
    public async Task<DefaultOperationResponse> RecoverShardFromUploadedSnapshot(
        string collectionName,
        uint shardId,
        Stream snapshotContent,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null
    )
    {
        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots/upload?wait={ToUrlQueryString(isWaitForResult)}";

        if (snapshotPriority.HasValue)
        {
            url += $"&priority={ToUrlQueryString(snapshotPriority.Value)}";
        }

        if (!string.IsNullOrEmpty(snapshotChecksum))
        {
            url += $"&checksum={snapshotChecksum}";
        }

        var result = await RecoverFromUploadedSnapshot(
            url,
            snapshotContent,
            cancellationToken);

        return result;
    }

    /// <summary>
    /// Downloads the specified snapshot of a shard from a collection as a file stream.
    /// </summary>
    /// <param name="collectionName">Name of the collection to download snapshot for.</param>
    /// <param name="shardId">Id of the shard for which to download snapshot.</param>
    /// <param name="snapshotName">Name of the snapshot to download.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<DownloadSnapshotResponse> DownloadShardSnapshot(
        string collectionName,
        uint shardId,
        string snapshotName,
        CancellationToken cancellationToken)
    {
        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots/{snapshotName}";

        HttpRequestMessage message = new(HttpMethod.Get, url);

        var result = await DownloadSnapshot(
            snapshotName,
            message,
            cancellationToken);

        if (result.Result is not null)
        {
            result.Result.SnapshotType = SnapshotType.Shard;
        }

        return result;
    }

    /// <summary>
    /// Deletes the specified snapshot of a shard from a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete shard snapshot for.</param>
    /// <param name="shardId">Id of the shard for which to delete snapshot.</param>
    /// <param name="snapshotName">Name of the snapshot to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    public async Task<DefaultOperationResponse> DeleteShardSnapshot(
        string collectionName,
        uint shardId,
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true
    )
    {
        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots/{snapshotName}?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<DefaultOperationResponse>(
            url,
            HttpMethod.Delete,
            cancellationToken,
            retryCount: 0);

        return response;
    }
}
