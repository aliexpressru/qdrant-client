using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Abstractions;

/// <summary>
/// Interface for the Qdrant HTTP API client.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Gets the underlying <see cref="HttpClient"/> instance for the specified collection or cluster name.
    /// </summary>
    /// <param name="collectionOrClusterName">The optional collection or cluster name for client resolution.</param>
#if NETSTANDARD2_0 || NETSTANDARD2_1
    public Task<HttpClient> GetApiClient(string collectionOrClusterName);
#else
    public ValueTask<HttpClient> GetApiClient(string collectionOrClusterName);
#endif

    /// <summary>
    /// Asynchronously wait until the collection status becomes <see cref="QdrantCollectionStatus.Green"/>
    /// and collection optimizer status becomes <see cref="QdrantOptimizerStatus.Ok"/>.
    /// </summary>
    /// <param name="collectionName">The name of the collection to check status for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="pollingInterval">The collection status polling interval. Is not set the default polling interval is 1 second.</param>
    /// <param name="timeout">The timeout after which the collection considered not green and exception is thrown. The default timeout is 30 seconds.</param>
    /// <param name="requiredNumberOfGreenCollectionResponses">The number of green status responses to be received
    /// for collection status to be considered green. To increase the probability that every node has
    /// the same green status - set this parameter to a value greater than the number of nodes.</param>
    /// <param name="isCheckShardTransfersCompleted">
    /// If set to <c>true</c> check that all collection shard transfers are completed.
    /// The collection is not considered ready until all shard transfers are completed.
    /// </param>
    Task EnsureCollectionReady(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? pollingInterval = null,
        TimeSpan? timeout = null,
        uint requiredNumberOfGreenCollectionResponses = 1,
        bool isCheckShardTransfersCompleted = false);

    /// <summary>
    /// Performs a single check of whether the collection status is <see cref="QdrantCollectionStatus.Green"/>
    /// and collection optimizer status is <see cref="QdrantOptimizerStatus.Ok"/>.
    /// Returns <c>true</c> in <see cref="Models.Responses.Base.QdrantResponseBase{TResult}.Result"/> if the collection is ready, <c>false</c> otherwise.
    /// </summary>
    /// <param name="collectionName">The name of the collection to check status for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="requiredNumberOfGreenCollectionResponses">The number of consecutive green status responses required
    /// for the collection to be considered ready. All checks must pass; if any check is not green, returns <c>false</c> immediately.</param>
    /// <param name="isCheckShardTransfersCompleted">
    /// If set to <c>true</c> check that all collection shard transfers are completed.
    /// The collection is not considered ready until all shard transfers are completed.
    /// </param>
    Task<DefaultOperationResponse> CheckCollectionReady(
        string collectionName,
        CancellationToken cancellationToken,
        uint requiredNumberOfGreenCollectionResponses = 1,
        bool isCheckShardTransfersCompleted = false);
}
