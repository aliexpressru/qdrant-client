using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http.Abstractions;

public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Create collection.
    /// </summary>
    /// <param name="collectionName">Collection name. Must be maximum 255 characters long.</param>
    /// <param name="request">The collection creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<DefaultOperationResponse> CreateCollection(
        string collectionName,
        CreateCollectionRequest request,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = 3,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null);

    /// <summary>
    /// Update parameters of the existing collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to update.</param>
    /// <param name="request">Collection parameters to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout in seconds. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<DefaultOperationResponse> UpdateCollectionParameters(
        string collectionName,
        UpdateCollectionParametersRequest request,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = 3,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null);

    /// <summary>
    /// Trigger optimizers on existing collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout in seconds. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    /// <remarks>Issues the empty update collection parameters request to start optimizers for grey collections. https://qdrant.tech/documentation/concepts/collections/#grey-collection-status</remarks>
    Task<DefaultOperationResponse> TriggerOptimizers(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = 3,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null);

    /// <summary>
    /// Get the detailed information about specified existing collection.
    /// </summary>
    /// <param name="collectionName">Collection name to get info for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<GetCollectionInfoResponse> GetCollectionInfo(
        string collectionName,
        CancellationToken cancellationToken,
        uint retryCount = 3,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null);

    /// <summary>
    /// Get the names of all the existing collections.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<ListCollectionsResponse> ListCollections(CancellationToken cancellationToken);

    /// <summary>
    /// Delete collection by name.
    /// </summary>
    /// <param name="collectionName">The name of the collection to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<DefaultOperationResponse> DeleteCollection(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = 3,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null);

    /// <summary>
    /// Get list of all existing collections aliases.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<ListCollectionAliasesResponse> ListAllAliases(
        CancellationToken cancellationToken,
        uint retryCount = 3,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null);

    /// <summary>
    /// Get list of all aliases for a specified collection.
    /// </summary>
    /// <param name="collectionName">The name of the collection to list aliases for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<ListCollectionAliasesResponse> ListCollectionAliases(
        string collectionName,
        CancellationToken cancellationToken,
        uint retryCount = 3,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null);

    /// <summary>
    /// Execute multiple collection aliases update operations in one batch.
    /// </summary>
    /// <param name="updateCollectionAliasesRequest">The request with update aliases operations batch.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<DefaultOperationResponse> UpdateCollectionsAliases(
        UpdateCollectionAliasesRequest updateCollectionAliasesRequest,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null,
        uint retryCount = 3,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null);

    /// <summary>
    /// Checks whether collection with specified name exists.
    /// </summary>
    /// <param name="collectionName">The name of the collection to check existence for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<CheckCollectionExistsResponse> CheckCollectionExists(
        string collectionName,
        CancellationToken cancellationToken);
}
