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
    /// A compound operation that deletes all existing storage snapshots.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    public async Task<DefaultOperationResponse> DeleteAllStorageSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult = true)
    {
        var storageSnapshots = await ListStorageSnapshots(cancellationToken);
        storageSnapshots.EnsureSuccess();

        foreach (var storageSnapshot in storageSnapshots.Result)
        {
            var deleteSnapshotResult = await DeleteStorageSnapshot(
                storageSnapshot.Name,
                cancellationToken,
                isWaitForResult);
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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    public async Task<DefaultOperationResponse> DeleteAllCollectionSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult = true)
    {
        var listAllCollectionsResponse = await ListCollections(cancellationToken);
        listAllCollectionsResponse.EnsureSuccess();

        var allCollectionNames = listAllCollectionsResponse.Result.Collections.Select(cn => cn.Name).ToHashSet();

        foreach (var collectionName in allCollectionNames)
        {
            var listCollectionSnapshotsResponse = await ListCollectionSnapshots(collectionName, cancellationToken);

            if (!listCollectionSnapshotsResponse.Status.IsSuccess)
            {
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
        }

        return new DefaultOperationResponse()
        {
            Result = true,
            Status = new QdrantStatus(QdrantOperationStatusType.Ok)
        };
    }
    
    /// <summary>
    /// A compound operation that deletes all existing collection shard snapshots.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    public async Task<DefaultOperationResponse> DeleteAllCollectionShardSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult = true)
    {
        var listAllCollectionsResponse = await ListCollections(cancellationToken);
        listAllCollectionsResponse.EnsureSuccess();

        var allCollectionNames = listAllCollectionsResponse.Result.Collections.Select(cn => cn.Name).ToHashSet();

        foreach (var collectionName in allCollectionNames)
        {
            var collectionClusteringInfo = await GetCollectionClusteringInfo(collectionName, cancellationToken);

            if (!collectionClusteringInfo.Status.IsSuccess)
            {
                return new DefaultOperationResponse()
                {
                    Result = false,
                    Status = collectionClusteringInfo.Status
                };
            }
#if NETSTANDARD2_0
            HashSet<uint> shardIds = new();
#else
            HashSet<uint> shardIds = new (
                collectionClusteringInfo.Result.LocalShards.Length
                + collectionClusteringInfo.Result.RemoteShards.Length);
#endif
            if (collectionClusteringInfo.Result.LocalShards is {Length: > 0} localShards)
            {
                foreach (var shardInfo in localShards)
                {
                    shardIds.Add(shardInfo.ShardId);
                }
            }

            if (collectionClusteringInfo.Result.RemoteShards is {Length: > 0} remoteShards)
            {
                foreach (var shardInfo in remoteShards)
                {
                    shardIds.Add(shardInfo.ShardId);
                }
            }
            
            foreach(var shardId in shardIds)
            {
                var listCollectionSnapshotsResponse = await ListShardSnapshots(collectionName, shardId, cancellationToken);

                if (!listCollectionSnapshotsResponse.Status.IsSuccess)
                {
                    return new DefaultOperationResponse()
                    {
                        Result = false,
                        Status = listCollectionSnapshotsResponse.Status
                    };
                }
                
                foreach(var shardSnapshot in listCollectionSnapshotsResponse.Result)
                {
                    var deleteCollectionShardSnapshotResponse =
                        await DeleteShardSnapshot(
                            collectionName,
                            shardId,
                            shardSnapshot.Name,
                            cancellationToken,
                            isWaitForResult);

                    if (!deleteCollectionShardSnapshotResponse.Status.IsSuccess)
                    {
                        return new DefaultOperationResponse()
                        {
                            Result = false,
                            Status = deleteCollectionShardSnapshotResponse.Status
                        };
                    }
                }
            }
        }

        return new DefaultOperationResponse()
        {
            Result = true,
            Status = new QdrantStatus(QdrantOperationStatusType.Ok)
        };
    }
}
