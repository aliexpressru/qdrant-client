using System.Diagnostics.CodeAnalysis;

#if  NETSTANDARD2_0
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
#endif

using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <summary>
    /// A compound operation that deletes all existing snapshots.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    public async Task<DefaultOperationResponse> DeleteAllCollectionSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult = true)
    {
        var storageSnapshots = await ListStorageSnapshots(cancellationToken);
        storageSnapshots.EnsureSuccess();

        foreach (var storageSnapshot in storageSnapshots.Result)
        { 
            var deleteSnapshotResult = await DeleteStorageSnapshot(storageSnapshot.Name, cancellationToken, isWaitForResult);
            deleteSnapshotResult.EnsureSuccess();
        }

        return new DefaultOperationResponse()
        {
            Result = true,
            Status = new QdrantStatus(QdrantOperationStatusType.Ok)
        };
    }
    
    /// <summary>
    /// A compound operation that deletes all existing collection snapshots.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete all snapshots for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If true, wait for changes to actually happen. If false - let changes happen in background.</param>
    public async Task<DefaultOperationResponse> DeleteAllCollectionSnapshots(
        string collectionName,
        CancellationToken cancellationToken,
        bool isWaitForResult = true)
    {
        var listAllCollectionsResponse = await ListCollections(cancellationToken);
        listAllCollectionsResponse.EnsureSuccess();

        var allCollectionNames = listAllCollectionsResponse.Result.Collections.Select(cn => cn.Name).ToHashSet();

        if (!allCollectionNames.Contains(collectionName))
        {
            // means collection does not exist
            // just return success since it does not have any visible snapshots
            return new DefaultOperationResponse()
            {
                Result = true,
                Status = new QdrantStatus(QdrantOperationStatusType.Ok)
            };
        }

        var listCollectionSnapshotsResponse = await ListCollectionSnapshots(collectionName, cancellationToken);

        if (!listCollectionSnapshotsResponse.Status.IsSuccess)
        {
            // return original error

            return new DefaultOperationResponse()
            {
                Result = false,
                Status = listCollectionSnapshotsResponse.Status
            };
        }

        foreach (var collectionSnapshot in listCollectionSnapshotsResponse.Result)
        {
            var deleteCollectionSnapshotResponse =
                await DeleteCollectionSnapshot(
                    collectionName,
                    collectionSnapshot.Name,
                    cancellationToken,
                    isWaitForResult);

            if (!deleteCollectionSnapshotResponse.Status.IsSuccess)
            {
                return new DefaultOperationResponse()
                {
                    Result = false,
                    Status = deleteCollectionSnapshotResponse.Status
                };
            }
        }

        return new DefaultOperationResponse()
        {
            Result = true,
            Status = new QdrantStatus(QdrantOperationStatusType.Ok)
        };
    }
}
