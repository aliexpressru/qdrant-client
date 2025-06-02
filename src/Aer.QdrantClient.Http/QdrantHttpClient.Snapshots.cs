using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;

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
    #region Collection snapshots operations

    /// <summary>
    /// Create new snapshot for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to create a snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    public async Task<CreateSnapshotResponse> CreateCollectionSnapshot(
        string collectionName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true)
    {
        var url =
            $"/collections/{collectionName}/snapshots?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<CreateSnapshotResponse>(
            url,
            HttpMethod.Post,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Get list of snapshots for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to get a snapshot list.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<ListSnapshotsResponse> ListCollectionSnapshots(
        string collectionName,
        CancellationToken cancellationToken)
    {
        var url =
            $"/collections/{collectionName}/snapshots";

        var response = await ExecuteRequest<ListSnapshotsResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Recover local collection data from a local snapshot by its name. This will overwrite any data, stored on
    /// this node, for the collection. If collection does not exist - it will be created.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="snapshotName">The name of the local snapshot file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster</param>
    public async Task<DefaultOperationResponse> RecoverCollectionFromSnapshot(
        string collectionName,
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null)
    {
        var localSnapshotUri = new Uri($"file:///qdrant/snapshots/{collectionName}/{snapshotName}");

        return await RecoverCollectionFromSnapshot(
            collectionName,
            localSnapshotUri,
            cancellationToken,
            isWaitForResult,
            snapshotPriority);
    }

    /// <summary>
    /// Recover local collection data from a possibly remote snapshot. This will overwrite any data, stored on
    /// this node, for the collection. If collection does not exist - it will be created.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="snapshotLocationUri">The snapshot location in URI format.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster</param>
    public async Task<DefaultOperationResponse> RecoverCollectionFromSnapshot(
        string collectionName,
        Uri snapshotLocationUri,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null)
    {
        var url =
            $"/collections/{collectionName}/snapshots/recover?wait={ToUrlQueryString(isWaitForResult)}";

        var request = new RecoverEntityFromSnapshotRequest(snapshotLocationUri)
        {
            Priority = snapshotPriority
        };

        var response = await ExecuteRequest<RecoverEntityFromSnapshotRequest, DefaultOperationResponse>(
            url,
            HttpMethod.Put,
            request,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Recover local collection data from an uploaded snapshot. This will overwrite any data,
    /// stored on this node, for the collection. If collection does not exist - it will be created.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="snapshotContent">The snapshot content stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster</param>
    public async Task<DefaultOperationResponse> RecoverCollectionFromUploadedSnapshot(
        string collectionName,
        Stream snapshotContent,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null
    )
    {
        var url =
            $"/collections/{collectionName}/snapshots/upload?wait={ToUrlQueryString(isWaitForResult)}";

        if (snapshotPriority.HasValue)
        {
            url += $"&priority={ToUrlQueryString(snapshotPriority.Value)}";
        }

        var result = await RecoverFromUploadedSnapshot(
            url,
            snapshotContent,
            cancellationToken);

        return result;
    }

    /// <summary>
    /// Delete snapshot for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete snapshot for.</param>
    /// <param name="snapshotName">Name of the snapshot to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    public async Task<DefaultOperationResponse> DeleteCollectionSnapshot(
        string collectionName,
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true
    )
    {
        var url =
            $"/collections/{collectionName}/snapshots/{snapshotName}?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<DefaultOperationResponse>(
            url,
            HttpMethod.Delete,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Download specified snapshot from a collection as a file stream.
    /// </summary>
    /// <param name="collectionName">Name of the collection to download snapshot for.</param>
    /// <param name="snapshotName">Name of the snapshot to download.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<DownloadSnapshotResponse> DownloadCollectionSnapshot(
        string collectionName,
        string snapshotName,
        CancellationToken cancellationToken)
    {
        var url =
            $"/collections/{collectionName}/snapshots/{snapshotName}";

        HttpRequestMessage message = new(HttpMethod.Get, url);

        var result = await DownloadSnapshot(
            snapshotName,
            message,
            cancellationToken);

        return result;
    }

    #endregion

    #region Shards snapshots operations

    /// <summary>
    /// Create new snapshot of a shard for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to create a snapshot.</param>
    /// <param name="shardId">Id of the shard for which to create a snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    public async Task<CreateSnapshotResponse> CreateShardSnapshot(
        string collectionName,
        int shardId,
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

        return response;
    }

    /// <summary>
    /// Get list of snapshots for a shard of a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection for which to get a snapshot list.</param>
    /// <param name="shardId">Id of the shard for which to list snapshots.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<ListSnapshotsResponse> ListShardSnapshots(
        string collectionName,
        int shardId,
        CancellationToken cancellationToken)
    {
        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots";

        var response = await ExecuteRequest<ListSnapshotsResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Recover shard of a local collection data from a snapshot by its name.
    /// This will overwrite any data, stored in this shard, for the collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="shardId">Id of the shard which to recover from a snapshot.</param>
    /// <param name="snapshotName">The snapshot file name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster</param>
    public async Task<DefaultOperationResponse> RecoverShardFromSnapshot(
        string collectionName,
        int shardId,
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null)
    {
        var localSnapshotUri = new Uri($"file:///qdrant/snapshots/{snapshotName}");

        return await RecoverShardFromSnapshot(
            collectionName,
            shardId,
            localSnapshotUri,
            cancellationToken,
            isWaitForResult,
            snapshotPriority);
    }

    /// <summary>
    /// Recover shard of a local collection data from a snapshot.
    /// This will overwrite any data, stored in this shard, for the collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to restore from a snapshot.</param>
    /// <param name="shardId">Id of the shard which to recover from a snapshot.</param>
    /// <param name="snapshotLocationUri">The snapshot location in URI format.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster</param>
    public async Task<DefaultOperationResponse> RecoverShardFromSnapshot(
        string collectionName,
        int shardId,
        Uri snapshotLocationUri,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null)
    {
        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots/recover?wait={ToUrlQueryString(isWaitForResult)}";

        var request = new RecoverEntityFromSnapshotRequest(snapshotLocationUri)
        {
            Priority = snapshotPriority
        };

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
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster</param>
    public async Task<DefaultOperationResponse> RecoverShardFromUploadedSnapshot(
        string collectionName,
        int shardId,
        Stream snapshotContent,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null
    )
    {
        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots/upload?wait={ToUrlQueryString(isWaitForResult)}";

        if (snapshotPriority.HasValue)
        {
            url += $"&priority={ToUrlQueryString(snapshotPriority.Value)}";
        }

        var result = await RecoverFromUploadedSnapshot(
            url,
            snapshotContent,
            cancellationToken);

        return result;
    }

    /// <summary>
    /// Delete snapshot for a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete snapshot for.</param>
    /// <param name="shardId">Id of the shard for which to delete snapshot.</param>
    /// <param name="snapshotName">Name of the snapshot to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    public async Task<DefaultOperationResponse> DeleteShardSnapshot(
        string collectionName,
        int shardId,
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

    /// <summary>
    /// Download specified snapshot of a shard from a collection as a file stream.
    /// </summary>
    /// <param name="collectionName">Name of the collection to download snapshot for.</param>
    /// <param name="shardId">Id of the shard for which to download snapshot.</param>
    /// <param name="snapshotName">Name of the snapshot to download.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<DownloadSnapshotResponse> DownloadShardSnapshot(
        string collectionName,
        int shardId,
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

        return result;
    }

    #endregion

    private async Task<DefaultOperationResponse> RecoverFromUploadedSnapshot(
        string url,
        Stream snapshotContent,
        CancellationToken cancellationToken)
    {
        var httpMethod = HttpMethod.Post;

        var response = await ExecuteRequestCore<DefaultOperationResponse>(
            CreateMessage,
            cancellationToken,
            retryCount: 0U,
            retryDelay: null,
            onRetry: null);

        return response;

        HttpRequestMessage CreateMessage()
        {
            HttpRequestMessage message = new(httpMethod, url);

            var requestData = new MultipartFormDataContent();
            requestData.Add(new StreamContent(snapshotContent), name: "snapshot");

            message.Content = requestData;

            return message;
        }
    }

    private async Task<DownloadSnapshotResponse> DownloadSnapshot(
        string snapshotName,
        HttpRequestMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultStream = await ExecuteRequestReadAsStream(message, cancellationToken);

            return new DownloadSnapshotResponse(
                snapshotName,
                resultStream.ResponseStream,
                resultStream.ContentLength,
                new QdrantStatus(QdrantOperationStatusType.Ok));
        }
        catch (QdrantCommunicationException qce)
        {
            return new DownloadSnapshotResponse(
                null,
                null,
                -1,
                new QdrantStatus(QdrantOperationStatusType.Error)
                {
                    Exception = qce,
                    Error = qce.Message
                });
        }
        catch (QdrantUnauthorizedAccessException que)
        {
            return new DownloadSnapshotResponse(
                null,
                null,
                -1,
                new QdrantStatus(QdrantOperationStatusType.Error)
                {
                    Exception = que,
                    Error = que.Message
                });
        }
    }
}
