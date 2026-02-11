using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http.Abstractions;

public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Get the detailed information about specified existing collection.
    /// </summary>
    /// <param name="collectionName">Collection name to get info for.</param>
    /// <param name="isCountExactPointsNumber">If set to <c>true</c> counts the exact number of points in collection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    /// <param name="clusterName">
    /// The optional cluster name for multi-cluster client scenarios.
    /// If set it will be used for multi-cluster routing instead of <paramref name="collectionName"/>.
    /// </param>
    Task<GetCollectionInfoResponse> GetCollectionInfo(
        string collectionName,
        bool isCountExactPointsNumber,
        CancellationToken cancellationToken,
        uint retryCount = 3,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null,
        string clusterName = null);

    /// <summary>
    /// Get the detailed information about all existing collections.
    /// </summary>
    /// <param name="isCountExactPointsNumber">If set to <c>true</c> counts collection points for all collections.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<ListCollectionInfoResponse> ListCollectionInfo(
        bool isCountExactPointsNumber,
        CancellationToken cancellationToken,
        uint retryCount = 3,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null,
        string clusterName = null);

    /// <summary>
    /// Create HNSW index and many payload indexes if they are defined in a fire-and-forget manner.
    /// Pass logger when constructing <see cref="QdrantHttpClient"/> to see any errors that may happen during this operation.
    /// </summary>
    /// <param name="collectionName">Collection name to create indexes for.</param>
    /// <param name="payloadIndexes">Collection payload index definitions that describe payload indexes to create after the HNSW index creation has been successfully issued.</param>
    public void StartCreatingCollectionPayloadIndexes(
        string collectionName,
        ICollection<CollectionPayloadIndexDefinition> payloadIndexes);
}
