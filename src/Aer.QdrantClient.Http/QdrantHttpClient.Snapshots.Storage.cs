using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Requests;

#if  NETSTANDARD2_0
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
#endif

using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <inheritdoc/>
    public async Task<ListSnapshotsResponse> ListStorageSnapshots(
        CancellationToken cancellationToken)
    {
        var url = "/snapshots";

        var response = await ExecuteRequest<ListSnapshotsResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);
        
        if (response.Result is { Count: > 0 })
        {
            foreach (var snapshot in response.Result)
            {
                snapshot.SnapshotType = SnapshotType.Storage;
            }
        }

        return response;
    }

    /// <inheritdoc/>
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

        if (response.Result is not null)
        {
            response.Result.SnapshotType = SnapshotType.Storage;
        }

        return response;
    }

    /// <inheritdoc/>
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
        
        if (result.Result is not null)
        {
            result.Result.SnapshotType = SnapshotType.Storage;
        }

        return result;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> RecoverStorageFromSnapshot(
        string snapshotName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null)
    {
        var localSnapshotUri = new Uri($"file:///qdrant/snapshots/{snapshotName}");
    
        return await RecoverStorageFromSnapshot(
            localSnapshotUri,
            cancellationToken,
            isWaitForResult,
            snapshotPriority,
            snapshotChecksum);
    }
    
    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> RecoverStorageFromSnapshot(
        Uri snapshotLocationUri,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null)
    {
        // Full storage snapshot is a .tar file with each collection having its own snapshot inside.
        // Alongside it the config.json maps snapshots to collections.

        /*
        {
          "collections_mapping": {
            "test_collection": "test_collection-7273830188020032-2025-09-19-15-30-41.snapshot"
          },
          "collections_aliases": {}
        }
        */
        
        // We need to unpack tar and apply snapshots to collections according to config.json

        throw new NotImplementedException(
            "This method is not available as a direct Qdrant API call and not implemented yet.");
        
        var url =
            $"/snapshots/recover?wait={ToUrlQueryString(isWaitForResult)}";
    
        var request = new RecoverEntityFromSnapshotRequest(snapshotLocationUri, snapshotPriority, snapshotChecksum);
        
        var response = await ExecuteRequest<RecoverEntityFromSnapshotRequest, DefaultOperationResponse>(
            url,
            HttpMethod.Put,
            request,
            cancellationToken,
            retryCount: 0);
    
        return response;
    }
    
    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> RecoverStorageFromUploadedSnapshot(
        Stream snapshotContent,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null,
        string snapshotChecksum = null
    )
    {
        // Full storage snapshot is a .tar file with each collection having its own snapshot inside.
        // Alongside it the config.json maps snapshots to collections.

        /*
        {
          "collections_mapping": {
            "test_collection": "test_collection-7273830188020032-2025-09-19-15-30-41.snapshot"
          },
          "collections_aliases": {}
        }
        */

        // We need to unpack tar and apply snapshots to collections according to config.json

        throw new NotImplementedException("This method is not available as a direct Qdrant API call and not implemented yet.");
        
        var url =
            $"/snapshots/upload?wait={ToUrlQueryString(isWaitForResult)}";
    
        if (snapshotPriority.HasValue)
        {
            url += $"&priority={ToUrlQueryString(snapshotPriority.Value)}";
        }
    
        if (!string.IsNullOrEmpty(snapshotChecksum))
        { 
            url+= $"&checksum={snapshotChecksum}";
        }
    
        var result = await RecoverFromUploadedSnapshot(
            url,
            snapshotContent,
            cancellationToken);
    
        return result;
    }
}
