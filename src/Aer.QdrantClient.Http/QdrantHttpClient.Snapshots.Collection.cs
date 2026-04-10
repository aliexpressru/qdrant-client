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
    public async Task<ListSnapshotsResponse> ListCollectionSnapshots(
        string collectionName,
        CancellationToken cancellationToken)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(ListCollectionSnapshots), null);

        var url =
            $"/collections/{collectionName}/snapshots";

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
                snapshot.SnapshotType = SnapshotType.Collection;
            }
        }

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<CreateSnapshotResponse> CreateCollectionSnapshot(
        string collectionName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(CreateCollectionSnapshot), null);

        var url =
            $"/collections/{collectionName}/snapshots?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<CreateSnapshotResponse>(
            url,
            HttpMethod.Post,
            collectionName,
            cancellationToken,
            retryCount: 0);

        response.Result?.SnapshotType = SnapshotType.Collection;

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> RecoverCollectionFromSnapshot(
        string collectionName,
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null)
    {
        // We are calling another overload here so no diagnostic timer

        var localSnapshotUri = new Uri($"file:///qdrant/snapshots/{collectionName}/{snapshotName}");

        var response = await RecoverCollectionFromSnapshot(
            collectionName,
            localSnapshotUri,
            cancellationToken,
            isWaitForResult,
            snapshotPriority,
            snapshotChecksum);

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> RecoverCollectionFromSnapshot(
        string collectionName,
        Uri snapshotLocationUri,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(RecoverCollectionFromSnapshot), null);

        var url =
            $"/collections/{collectionName}/snapshots/recover?wait={ToUrlQueryString(isWaitForResult)}";

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
    public async Task<DefaultOperationResponse> RecoverCollectionFromUploadedSnapshot(
        string collectionName,
        Stream snapshotContent,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null
    )
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(RecoverCollectionFromUploadedSnapshot), null);

        var url =
            $"/collections/{collectionName}/snapshots/upload?wait={ToUrlQueryString(isWaitForResult)}";

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
            collectionName,
            snapshotContent,
            cancellationToken);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DownloadSnapshotResponse> DownloadCollectionSnapshot(
        string collectionName,
        string snapshotName,
        CancellationToken cancellationToken)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DownloadCollectionSnapshot), null);

        var url =
            $"/collections/{collectionName}/snapshots/{snapshotName}";

        HttpRequestMessage message = new(HttpMethod.Get, url);

        var response = await DownloadSnapshot(
            collectionName,
            snapshotName,
            message,
            cancellationToken);

        response.Result?.SnapshotType = SnapshotType.Collection;

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> DeleteCollectionSnapshot(
        string collectionName,
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true
    )
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DeleteCollectionSnapshot), null);

        var url =
            $"/collections/{collectionName}/snapshots/{snapshotName}?wait={ToUrlQueryString(isWaitForResult)}";

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
