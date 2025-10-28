using System.Diagnostics;
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
    /// <inheritdoc/>
    public async Task<ListSnapshotsResponse> ListAllSnapshots(CancellationToken cancellationToken)
    {
        List<SnapshotInfo> allSnapshots = new();

        Stopwatch sw = Stopwatch.StartNew();

        var listAllCollectionsResponse = await ListCollections(cancellationToken);
        listAllCollectionsResponse.EnsureSuccess();

        var allCollectionNames = listAllCollectionsResponse.Result.Collections.Select(cn => cn.Name).ToHashSet();

        // Collect collection and shards snapshots
        foreach (var collectionName in allCollectionNames)
        {
            // Collection snapshots

            var listCollectionSnapshotsResponse = await ListCollectionSnapshots(collectionName, cancellationToken);

            foreach (var collectionSnapshot in listCollectionSnapshotsResponse.Result)
            {
                collectionSnapshot.SnapshotType = SnapshotType.Collection;
                allSnapshots.Add(collectionSnapshot);
            }

            var collectionClusteringInfo = await GetCollectionClusteringInfo(collectionName, cancellationToken);

            if (!collectionClusteringInfo.Status.IsSuccess)
            {
                return new ListSnapshotsResponse(collectionClusteringInfo)
                {
                    Result = null
                };
            }

            // Shard snapshots

#if NETSTANDARD2_0
            HashSet<uint> shardIds = new();
#else
            HashSet<uint> shardIds = new(
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

            foreach (var shardId in shardIds)
            {
                var listShardSnapshotsResponse = await ListShardSnapshots(collectionName, shardId, cancellationToken);

                if (!listShardSnapshotsResponse.Status.IsSuccess)
                {
                    return new ListSnapshotsResponse(listShardSnapshotsResponse)
                    {
                        Result = null
                    };
                }

                foreach (var shardSnapshot in listShardSnapshotsResponse.Result)
                {
                    shardSnapshot.SnapshotType = SnapshotType.Shard;
                    allSnapshots.Add(shardSnapshot);
                }
            }
        }

        // Collect storage snapshots

        var listStorageSnapshotsResponse = await ListStorageSnapshots(cancellationToken);

        if (!listStorageSnapshotsResponse.Status.IsSuccess)
        {
            return new ListSnapshotsResponse(listStorageSnapshotsResponse)
            {
                Result = null
            };
        }

        foreach (var storageSnapshot in listStorageSnapshotsResponse.Result)
        {
            storageSnapshot.SnapshotType = SnapshotType.Storage;
            allSnapshots.Add(storageSnapshot);
        }

        sw.Stop();

        return new ListSnapshotsResponse()
        {
            Result = allSnapshots,
            Status = QdrantStatus.Success(),
            Time = sw.Elapsed.TotalSeconds
        };
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
                return new DefaultOperationResponse(listCollectionSnapshotsResponse)
                {
                    Result = false
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
                    return new DefaultOperationResponse(deleteCollectionSnapshotResponse)
                    {
                        Result = false
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
    
    /// <inheritdoc/>
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
                return new DefaultOperationResponse(collectionClusteringInfo)
                {
                    Result = false
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
                        return new DefaultOperationResponse(deleteCollectionShardSnapshotResponse)
                        {
                            Result = false
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
