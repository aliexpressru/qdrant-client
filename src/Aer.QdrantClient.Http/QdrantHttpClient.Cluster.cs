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
        CancellationToken cancellationToken)
    {
        var url = "/cluster";

        var response = await ExecuteRequest<GetClusterInfoResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> RecoverPeerRaftState(
        CancellationToken cancellationToken)
    {
        var url = "/cluster/recover";

        var response = await ExecuteRequest<DefaultOperationResponse>(
            url,
            HttpMethod.Post,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultOperationResponse> RemovePeer(
        ulong peerId,
        CancellationToken cancellationToken,
        bool isForceDropOperation = false,
        TimeSpan? timeout = null)
    {
        var url = $"/cluster/peer/{peerId}?force={ToUrlQueryString(isForceDropOperation)}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<DefaultOperationResponse>(
            url,
            HttpMethod.Delete,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<GetCollectionClusteringInfoResponse> GetCollectionClusteringInfo(
        string collectionName,
        CancellationToken cancellationToken,
        bool isTranslatePeerIdsToUris = false)
    {
        var url = $"/collections/{collectionName}/cluster";

        var collectionShardingInfo = await ExecuteRequest<GetCollectionClusteringInfoResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        if (isTranslatePeerIdsToUris
            && collectionShardingInfo.Status.IsSuccess
            && collectionShardingInfo.Result is not null)
        {
            var clusterInfo = await GetClusterInfo(cancellationToken);

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
            cancellationToken,
            retryCount: 0);

        return response;
    }
}
