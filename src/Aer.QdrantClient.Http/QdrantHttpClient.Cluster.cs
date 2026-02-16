using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <inheritdoc/>
    public async Task<GetClusterInfoResponse> GetClusterInfo(
        CancellationToken cancellationToken,
        string clusterName = null)
    {
        var url = "/cluster";

        var response = await ExecuteRequest<GetClusterInfoResponse>(
            url,
            HttpMethod.Get,
            clusterName,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> RecoverPeerRaftState(
        CancellationToken cancellationToken,
        string clusterName = null)
    {
        var url = "/cluster/recover";

        var response = await ExecuteRequest<DefaultOperationResponse>(
            url,
            HttpMethod.Post,
            clusterName,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> RemovePeer(
        ulong peerId,
        CancellationToken cancellationToken,
        bool isForceDropOperation = false,
        TimeSpan? timeout = null,
        string clusterName = null)
    {
        var url = $"/cluster/peer/{peerId}?force={ToUrlQueryString(isForceDropOperation)}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<DefaultOperationResponse>(
            url,
            HttpMethod.Delete,
            clusterName,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<GetCollectionClusteringInfoResponse> GetCollectionClusteringInfo(
        string collectionName,
        CancellationToken cancellationToken,
        bool isTranslatePeerIdsToUris = false,
        string clusterName = null)
    {
        var url = $"/collections/{collectionName}/cluster";

        var collectionShardingInfo = await ExecuteRequest<GetCollectionClusteringInfoResponse>(
            url,
            HttpMethod.Get,
            clusterName,
            cancellationToken,
            retryCount: 0);

        if (isTranslatePeerIdsToUris
            && collectionShardingInfo.Status.IsSuccess
            && collectionShardingInfo.Result is not null)
        {
            var clusterInfo = await GetClusterInfo(cancellationToken, clusterName);

            collectionShardingInfo.Result.PeerUri =
                clusterInfo.Result.ParsedPeers[collectionShardingInfo.Result.PeerId].Uri;

            if (collectionShardingInfo.Result.RemoteShards is not null)
            {
                foreach (var shard in collectionShardingInfo.Result.RemoteShards)
                {
                    var shardPeer = shard.PeerId;
                    var shardPeerUri = clusterInfo.Result.ParsedPeers[shardPeer].Uri;

                    shard.PeerUri = shardPeerUri;
                }
            }

            if (collectionShardingInfo.Result.ReshardingOperations is not null)
            {
                foreach (var reshardingOperation in collectionShardingInfo.Result.ReshardingOperations)
                {
                    var shardPeer = reshardingOperation.PeerId;
                    var shardPeerUri = clusterInfo.Result.ParsedPeers[shardPeer].Uri;

                    reshardingOperation.PeerUri = shardPeerUri;
                }
            }
        }

        // Collect shards by peers info

        if (collectionShardingInfo.Status.IsSuccess
            && collectionShardingInfo.Result is not null)
        {
            var shardsByPeers = new Dictionary<ulong, List<uint>>(
                collectionShardingInfo.Result.RemoteShards.Length
                + collectionShardingInfo.Result.LocalShards.Length
            );

            var answeringPeerId = collectionShardingInfo.Result.PeerId;

            shardsByPeers.Add(answeringPeerId, new List<uint>(collectionShardingInfo.Result.LocalShards.Length));

            foreach (var shard in collectionShardingInfo.Result.LocalShards)
            {
                var shardId = shard.ShardId;

                shardsByPeers[answeringPeerId].Add(shardId);
            }

            foreach (var shard in collectionShardingInfo.Result.RemoteShards)
            {
                var shardPeer = shard.PeerId;

#pragma warning disable CA1854 // Justification: false positive
                if (!shardsByPeers.ContainsKey(shardPeer))
                {
                    shardsByPeers.Add(shardPeer, []);
                }
#pragma warning restore CA1854

                var shardId = shard.ShardId;

                shardsByPeers[shardPeer].Add(shardId);
            }

            collectionShardingInfo.Result.ShardsByPeers = shardsByPeers;
        }

        return collectionShardingInfo;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> UpdateCollectionClusteringSetup(
        string collectionName,
        UpdateCollectionClusteringSetupRequest updateOperation,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null)
    {
        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var url = $"/collections/{collectionName}/cluster?timeout={timeoutValue}";

        var response = await ExecuteRequest<UpdateCollectionClusteringSetupRequest, DefaultOperationResponse>(
            url,
            HttpMethod.Post,
            updateOperation,
            collectionName,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> CreateShardKey(
        string collectionName,
        ShardKey shardKey,
        CancellationToken cancellationToken,
        uint? shardsNumber = null,
        uint? replicationFactor = null,
        ulong[] placement = null,
        TimeSpan? timeout = null,
        ShardState? initialState = null)
    {
        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var createShardKeyRequest = new CreateShardKeyRequest(
            shardKey,
            shardsNumber,
            replicationFactor,
            placement,
            initialState);

        var url = $"/collections/{collectionName}/shards?timeout={timeoutValue}";

        var response = await ExecuteRequest<CreateShardKeyRequest, DefaultOperationResponse>(
            url,
            HttpMethod.Put,
            createShardKeyRequest,
            collectionName,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> DeleteShardKey(
        string collectionName,
        ShardKey shardKey,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null)
    {
        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var deleteShardKeyRequest = new DeleteShardKeyRequest(shardKey);

        var url = $"/collections/{collectionName}/shards/delete?timeout={timeoutValue}";

        var response = await ExecuteRequest<DeleteShardKeyRequest, DefaultOperationResponse>(
            url,
            HttpMethod.Post,
            deleteShardKeyRequest,
            collectionName,
            cancellationToken,
            retryCount: 0);

        return response;
    }
}
