using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <summary>
    /// Get information about the current state and composition of the cluster (shards).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<GetClusterInfoResponse> GetClusterInfo(
        CancellationToken cancellationToken,
        bool getAsRawString = false)
    {
        var url = "/cluster";

        if (getAsRawString)
        {
            try
            {
                HttpRequestMessage message = new(HttpMethod.Get, url);
                var rawClusterStatusString = await ExecuteRequestPlain(url, message, cancellationToken);

                return new GetClusterInfoResponse()
                {
                    RawClusterStatusString = rawClusterStatusString,
                    Status = new QdrantStatus(QdrantOperationStatusType.Ok)
                };
            }
            catch (Exception ex)
            {
                return new GetClusterInfoResponse()
                {
                    Status = new QdrantStatus(QdrantOperationStatusType.Error)
                    {
                        Error = ex.Message,
                        Exception = ex
                    }
                };
            }
        }

        var response = await ExecuteRequest<GetClusterInfoResponse>(url, HttpMethod.Get, cancellationToken);

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
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Removes the specified peer (shard) from the cluster.
    /// </summary>
    /// <param name="peerId">The identifier of the peer to drop.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isForceDropOperation">If <c>true</c> - removes peer even if it has shards/replicas on it.</param>
    public async Task<DefaultOperationResponse> RemovePeer(
        ulong peerId,
        CancellationToken cancellationToken,
        bool isForceDropOperation = false)
    {
        var url = $"/cluster/peer/{peerId}?force={ToUrlQueryString(isForceDropOperation)}";

        var response = await ExecuteRequest<DefaultOperationResponse>(url, HttpMethod.Delete, cancellationToken);

        return response;
    }

    /// <summary>
    /// Get clustering (sharding) information for a collection.
    /// </summary>
    /// <param name="collectionName">Collection name to get sharding info for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<GetCollectionClusteringInfoResponse> GetCollectionClusteringInfo(
        string collectionName,
        CancellationToken cancellationToken)
    {
        var url = $"/collections/{collectionName}/cluster";

        var response = await ExecuteRequest<GetCollectionClusteringInfoResponse>(url, HttpMethod.Get, cancellationToken);

        return response;
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
            cancellationToken);

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

        string url = $"/collections/{collectionName}/shards?timeout={timeoutValue}";

        var response = await ExecuteRequest<CreateShardKeyRequest, DefaultOperationResponse>(
            url,
            HttpMethod.Put,
            createShardKeyRequest,
            cancellationToken);

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

        string url = $"/collections/{collectionName}/shards/delete?timeout={timeoutValue}";

        var response = await ExecuteRequest<DeleteShardKeyRequest, DefaultOperationResponse>(
            url,
            HttpMethod.Post,
            deleteShardKeyRequest,
            cancellationToken);

        return response;
    }
}
