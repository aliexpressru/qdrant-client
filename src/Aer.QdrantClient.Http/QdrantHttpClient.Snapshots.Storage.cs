using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Diagnostics.Helpers;

#if  NETSTANDARD2_0
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
#endif

using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <inheritdoc/>
    public async Task<ListSnapshotsResponse> ListStorageSnapshots(
        CancellationToken cancellationToken,
        string clusterName = null)
    {
        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(ListStorageSnapshots), clusterName);

        var url = "/snapshots";

        var response = await ExecuteRequest<ListSnapshotsResponse>(
            url,
            HttpMethod.Get,
            clusterName,
            cancellationToken,
            retryCount: 0);

        if (response.Result is { Count: > 0 })
        {
            foreach (var snapshot in response.Result)
            {
                snapshot.SnapshotType = SnapshotType.Storage;
            }
        }

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<CreateSnapshotResponse> CreateStorageSnapshot(
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        string clusterName = null)
    {
        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(CreateStorageSnapshot), clusterName);

        var url =
            $"/snapshots?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<CreateSnapshotResponse>(
            url,
            HttpMethod.Post,
            clusterName,
            cancellationToken,
            retryCount: 0);

        response.Result?.SnapshotType = SnapshotType.Storage;

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DownloadSnapshotResponse> DownloadStorageSnapshot(
        string snapshotName,
        CancellationToken cancellationToken,
        string clusterName = null)
    {
        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(DownloadStorageSnapshot), clusterName);

        var url = $"/snapshots/{snapshotName}";

        HttpRequestMessage message = new(HttpMethod.Get, url);

        var response = await DownloadSnapshot(
            clusterName,
            snapshotName,
            message,
            cancellationToken);

        response.Result?.SnapshotType = SnapshotType.Storage;

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> DeleteStorageSnapshot(
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        string clusterName = null)
    {
        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(DeleteStorageSnapshot), clusterName);

        var url = $"/snapshots/{snapshotName}?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<DefaultOperationResponse>(
            url,
            HttpMethod.Delete,
            clusterName,
            cancellationToken,
            retryCount: 0);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }
}
