using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Diagnostics.Helpers;

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
    /// <inheritdoc/>
    public async Task<ListSnapshotsResponse> ListShardSnapshots(
        string collectionName,
        uint shardId,
        CancellationToken cancellationToken)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(ListShardSnapshots), null);

        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots";

        var response = await ExecuteRequest<ListSnapshotsResponse>(
            url,
            HttpMethod.Get,
            collectionName,
            cancellationToken,
            retryCount: 0);

        if (response.Result is { Count: > 0 })
        {
            foreach (var snapshot in response.Result)
            {
                snapshot.SnapshotType = SnapshotType.Shard;
            }
        }

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<CreateSnapshotResponse> CreateShardSnapshot(
        string collectionName,
        uint shardId,
        CancellationToken cancellationToken,
        bool isWaitForResult = true)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(CreateShardSnapshot), null);

        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<CreateSnapshotResponse>(
            url,
            HttpMethod.Post,
            collectionName,
            cancellationToken,
            retryCount: 0);

        response.Result?.SnapshotType = SnapshotType.Shard;

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> RecoverShardFromSnapshot(
        string collectionName,
        uint shardId,
        Uri snapshotLocationUri,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(RecoverShardFromSnapshot), null);

        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots/recover?wait={ToUrlQueryString(isWaitForResult)}";

        var request = new RecoverEntityFromSnapshotRequest(snapshotLocationUri, snapshotPriority, snapshotChecksum);

        var response = await ExecuteRequest<RecoverEntityFromSnapshotRequest, DefaultOperationResponse>(
            url,
            HttpMethod.Put,
            request,
            collectionName,
            cancellationToken,
            retryCount: 0);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
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
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(RecoverShardFromUploadedSnapshot), null);

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

        var response = await RecoverFromUploadedSnapshot(
            url,
            collectionOrClusterName: collectionName,
            snapshotContent,
            cancellationToken);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DownloadSnapshotResponse> DownloadShardSnapshot(
        string collectionName,
        uint shardId,
        string snapshotName,
        CancellationToken cancellationToken)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DownloadShardSnapshot), null);

        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots/{snapshotName}";

        HttpRequestMessage message = new(HttpMethod.Get, url);

        var response = await DownloadSnapshot(
            collectionOrClusterName: collectionName,
            snapshotName,
            message,
            cancellationToken);

        response.Result?.SnapshotType = SnapshotType.Shard;

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> DeleteShardSnapshot(
        string collectionName,
        uint shardId,
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true
    )
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DeleteShardSnapshot), null);

        var url =
            $"/collections/{collectionName}/shards/{shardId}/snapshots/{snapshotName}?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<DefaultOperationResponse>(
            url,
            HttpMethod.Delete,
            collectionName,
            cancellationToken,
            retryCount: 0);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }
}
