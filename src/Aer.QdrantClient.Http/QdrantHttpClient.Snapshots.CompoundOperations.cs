using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Diagnostics.Helpers;
using Aer.QdrantClient.Http.Diagnostics.Tracing;

#if NETSTANDARD2_0
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
#endif

using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <inheritdoc/>
    public async Task<ListSnapshotsResponse> ListAllSnapshots(
        CancellationToken cancellationToken,
        bool includeStorageSnapshots = false,
        string clusterName = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(ListAllSnapshots),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(ListAllSnapshots), clusterName);

        List<SnapshotInfo> allSnapshots = [];

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
                var earlyRet = new ListSnapshotsResponse(collectionClusteringInfo)
                {
                    Result = null
                };

                tracingScope.SetResult(earlyRet);

                return earlyRet;
            }

            // Local shard snapshots

#if NETSTANDARD2_0
            HashSet<uint> localShardIds = [];
#else
            HashSet<uint> localShardIds = new(collectionClusteringInfo.Result.LocalShards.Length);
#endif
            // Listing remote shards snapshots is forbidden in Qdrant API
            if (collectionClusteringInfo.Result.LocalShards is { Length: > 0 } localShards)
            {
                foreach (var shardInfo in localShards)
                {
                    localShardIds.Add(shardInfo.ShardId);
                }
            }

            foreach (var localShardId in localShardIds)
            {
                var listShardSnapshotsResponse = await ListShardSnapshots(collectionName, localShardId, cancellationToken);

                if (!listShardSnapshotsResponse.Status.IsSuccess)
                {
                    var earlyRet = new ListSnapshotsResponse(listShardSnapshotsResponse)
                    {
                        Result = null
                    };

                    tracingScope.SetResult(earlyRet);

                    return earlyRet;
                }

                foreach (var shardSnapshot in listShardSnapshotsResponse.Result)
                {
                    shardSnapshot.SnapshotType = SnapshotType.Shard;
                    allSnapshots.Add(shardSnapshot);
                }
            }
        }

        // Collect storage snapshots

        if (includeStorageSnapshots)
        {
            var listStorageSnapshotsResponse = await ListStorageSnapshots(cancellationToken);

            if (!listStorageSnapshotsResponse.Status.IsSuccess)
            {
                var earlyRet = new ListSnapshotsResponse(listStorageSnapshotsResponse)
                {
                    Result = null
                };

                tracingScope.SetResult(earlyRet);

                return earlyRet;
            }

            foreach (var storageSnapshot in listStorageSnapshotsResponse.Result)
            {
                storageSnapshot.SnapshotType = SnapshotType.Storage;
                allSnapshots.Add(storageSnapshot);
            }
        }

        sw.Stop();

        var ret = new ListSnapshotsResponse()
        {
            Result = allSnapshots,
            Status = QdrantStatus.Success(),
            Time = sw.Elapsed.TotalSeconds
        };

        tracingScope.SetResult(ret);

        diagnostic.SetSuccess();

        return ret;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> DeleteAllStorageSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        string clusterName = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(DeleteAllStorageSnapshots),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(DeleteAllStorageSnapshots), clusterName);

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

        var ret = new DefaultOperationResponse()
        {
            Result = true,
            Status = new QdrantStatus(QdrantOperationStatusType.Ok)
        };

        tracingScope.SetResult(ret);

        diagnostic.SetSuccess();

        return ret;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> DeleteAllCollectionSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        string clusterName = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(DeleteAllCollectionSnapshots),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(DeleteAllCollectionSnapshots), clusterName);

        var listAllCollectionsResponse = await ListCollections(cancellationToken);
        listAllCollectionsResponse.EnsureSuccess();

        var allCollectionNames = listAllCollectionsResponse.Result.Collections.Select(cn => cn.Name).ToHashSet();

        foreach (var collectionName in allCollectionNames)
        {
            var listCollectionSnapshotsResponse = await ListCollectionSnapshots(collectionName, cancellationToken);

            if (!listCollectionSnapshotsResponse.Status.IsSuccess)
            {
                var earlyRet = new DefaultOperationResponse(listCollectionSnapshotsResponse)
                {
                    Result = false
                };

                tracingScope.SetResult(earlyRet);

                return earlyRet;
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
                    var earlyRet = new DefaultOperationResponse(deleteCollectionSnapshotResponse)
                    {
                        Result = false
                    };

                    tracingScope.SetResult(earlyRet);

                    return earlyRet;
                }
            }
        }

        var ret = new DefaultOperationResponse()
        {
            Result = true,
            Status = new QdrantStatus(QdrantOperationStatusType.Ok)
        };

        tracingScope.SetResult(ret);

        diagnostic.SetSuccess();

        return ret;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> DeleteAllCollectionShardSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        string clusterName = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(DeleteAllCollectionShardSnapshots),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(DeleteAllCollectionShardSnapshots), clusterName);

        var listAllCollectionsResponse = await ListCollections(cancellationToken);
        listAllCollectionsResponse.EnsureSuccess();

        var allCollectionNames = listAllCollectionsResponse.Result.Collections.Select(cn => cn.Name).ToHashSet();

        foreach (var collectionName in allCollectionNames)
        {
            var collectionClusteringInfo = await GetCollectionClusteringInfo(collectionName, cancellationToken);

            if (!collectionClusteringInfo.Status.IsSuccess)
            {
                var earlyRet = new DefaultOperationResponse(collectionClusteringInfo)
                {
                    Result = false
                };

                tracingScope.SetResult(earlyRet);

                return earlyRet;
            }

#if NETSTANDARD2_0
            HashSet<uint> localSardIds = [];
#else
            HashSet<uint> localSardIds = new(collectionClusteringInfo.Result.LocalShards.Length);
#endif
            // Listing remote shards snapshots is forbidden in Qdrant API

            if (collectionClusteringInfo.Result.LocalShards is { Length: > 0 } localShards)
            {
                foreach (var shardInfo in localShards)
                {
                    localSardIds.Add(shardInfo.ShardId);
                }
            }

            foreach (var localShardId in localSardIds)
            {
                var listCollectionSnapshotsResponse = await ListShardSnapshots(collectionName, localShardId, cancellationToken);

                if (!listCollectionSnapshotsResponse.Status.IsSuccess)
                {
                    var earlyRet = new DefaultOperationResponse()
                    {
                        Result = false,
                        Status = listCollectionSnapshotsResponse.Status
                    };

                    tracingScope.SetResult(earlyRet);

                    return earlyRet;
                }

                foreach (var localShardSnapshot in listCollectionSnapshotsResponse.Result)
                {
                    var deleteCollectionShardSnapshotResponse =
                        await DeleteShardSnapshot(
                            collectionName,
                            localShardId,
                            localShardSnapshot.Name,
                            cancellationToken,
                            isWaitForResult);

                    if (!deleteCollectionShardSnapshotResponse.Status.IsSuccess)
                    {
                        var earlyRet = new DefaultOperationResponse(deleteCollectionShardSnapshotResponse)
                        {
                            Result = false
                        };

                        tracingScope.SetResult(earlyRet);

                        return earlyRet;
                    }
                }
            }
        }

        var ret = new DefaultOperationResponse()
        {
            Result = true,
            Status = new QdrantStatus(QdrantOperationStatusType.Ok)
        };

        tracingScope.SetResult(ret);

        diagnostic.SetSuccess();

        return ret;
    }
}
