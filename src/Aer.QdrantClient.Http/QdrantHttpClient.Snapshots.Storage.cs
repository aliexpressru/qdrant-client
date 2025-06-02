using System.Diagnostics.CodeAnalysis;

#if  NETSTANDARD2_0
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
#endif

using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <summary>
    /// Get list of snapshots of the whole storage
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<ListSnapshotsResponse> ListStorageSnapshots(
        CancellationToken cancellationToken)
    {
        var url = "/snapshots";

        var response = await ExecuteRequest<ListSnapshotsResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Create new snapshot of the whole storage.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    public async Task<CreateSnapshotResponse> CreateStorageSnapshot(
        CancellationToken cancellationToken,
        bool isWaitForResult = true)
    {
        var url =
            $"/snapshots?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<CreateSnapshotResponse>(
            url,
            HttpMethod.Post,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Delete snapshot of the whole storage.
    /// </summary>
    /// <param name="snapshotName">Name of the snapshot to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    public async Task<DefaultOperationResponse> DeleteStorageSnapshot(
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true
    )
    {
        var url = $"/snapshots/{snapshotName}?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<DefaultOperationResponse>(
            url,
            HttpMethod.Delete,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Download specified snapshot of the whole storage as a file stream.
    /// </summary>
    /// <param name="snapshotName">Name of the snapshot to download.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<DownloadSnapshotResponse> DownloadStorageSnapshot(
        string snapshotName,
        CancellationToken cancellationToken)
    {
        var url = $"/snapshots/{snapshotName}";

        HttpRequestMessage message = new(HttpMethod.Get, url);

        var result = await DownloadSnapshot(
            snapshotName,
            message,
            cancellationToken);

        return result;
    }
}
