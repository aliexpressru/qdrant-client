using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <summary>
    /// Get information about the current state and composition of the cluster (shards).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
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

    /// <summary>
    /// Tries to recover current peer Raft state.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
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

    /// <summary>
    /// Removes the specified peer (shard) from the cluster.
    /// </summary>
    /// <param name="peerId">The identifier of the peer to drop.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isForceDropOperation">If <c>true</c> - removes peer even if it has shards/replicas on it.</param>
    /// <param name="timeout">The operation timeout. If not set the default value of 30 seconds used.</param>
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

    /// <summary>
    /// Get clustering (sharding) information for a collection.
    /// </summary>
    /// <param name="collectionName">Collection name to get sharding info for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isTranslatePeerIdsToUris">If set to <c>true</c>, enriches collection cluster info response with peer URI values.</param>
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

    /// <summary>
    /// Update collection clustering (sharding) setup.
    /// </summary>
    /// <param name="collectionName">Collection name to update sharding info for.</param>
    /// <param name="updateOperation">The required collection clustering setup update operation model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">The operation timeout. If not set the default value of 30 seconds used.</param>
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

    /// <summary>
    /// Creates collection shards with specified shard key.
    /// </summary>
    /// <param name="collectionName">Collection name to create shard key for.</param>
    /// <param name="shardKey">The shard key for shards to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="shardsNumber">How many shards to create for this key. If not specified, will use the default value from config.</param>
    /// <param name="replicationFactor">How many replicas to create for each shard. If not specified, will use the default value from config.</param>
    /// <param name="placement">
    /// Placement of shards for this key - array of peer ids, that can be used to place shards for this key.
    /// If not specified, will be randomly placed among all peers.
    /// </param>
    /// <param name="timeout">The operation timeout. If not set the default value of 30 seconds used.</param>
    public async Task<DefaultOperationResponse> CreateShardKey(
        string collectionName,
        ShardKey shardKey,
        CancellationToken cancellationToken,
        uint? shardsNumber = null,
        uint? replicationFactor = null,
        ulong[] placement = null,
        TimeSpan? timeout = null)
    {
        var timeoutValue = GetTimeoutValueOrDefault(timeout);

        var createShardKeyRequest = new CreateShardKeyRequest(shardKey, shardsNumber, replicationFactor, placement);

        var url = $"/collections/{collectionName}/shards?timeout={timeoutValue}";

        var response = await ExecuteRequest<CreateShardKeyRequest, DefaultOperationResponse>(
            url,
            HttpMethod.Put,
            createShardKeyRequest,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Deletes collection shards with specified shard key.
    /// </summary>
    /// <param name="collectionName">Collection name to delete shard key for.</param>
    /// <param name="shardKey">The shard key for shards to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">The operation timeout. If not set the default value of 30 seconds used.</param>
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
