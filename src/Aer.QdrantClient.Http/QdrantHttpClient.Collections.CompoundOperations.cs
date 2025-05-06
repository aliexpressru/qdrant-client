using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    internal const uint DEFAULT_COLLECTION_INDEXING_THRESHOLD = 10000;

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
    public async Task<GetCollectionInfoResponse> GetCollectionInfo(
        string collectionName,
        bool isCountExactPointsNumber,
        CancellationToken cancellationToken,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var getCollectionInfoResponse = await GetCollectionInfo(
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        if (!getCollectionInfoResponse.Status.IsSuccess)
        {
            return getCollectionInfoResponse;
        }

        if (isCountExactPointsNumber)
        {
            var countPointsResponse = (await CountPoints(
                collectionName,
                new CountPointsRequest(
                    isCountExactPointsNumber: true,
                    filter: QdrantFilter.Empty),
                cancellationToken)).EnsureSuccess();

            getCollectionInfoResponse.Result.PointsCount = countPointsResponse.Count;
        }

        return getCollectionInfoResponse;
    }

    /// <summary>
    /// Get the detailed information about all existing collections.
    /// </summary>
    /// <param name="isCountExactPointsNumber">Is set to <c>true</c> counts collection points for all collections.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    public async Task<ListCollectionInfoResponse> ListCollectionInfo(
        bool isCountExactPointsNumber,
        CancellationToken cancellationToken,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var listCollectionsResponse =
            (await ListCollections(cancellationToken)).EnsureSuccess();

        Dictionary<string, GetCollectionInfoResponse.CollectionInfo> collectionInfos =
            new(listCollectionsResponse.Collections.Length);

        foreach (var collectionNameInfo in listCollectionsResponse.Collections)
        {
            var getCollectionInfoResponse = (await GetCollectionInfo(
                    collectionNameInfo.Name,
                    isCountExactPointsNumber,
                    cancellationToken,
                    retryCount,
                    retryDelay,
                    onRetry)
                ).EnsureSuccess();

            collectionInfos.Add(collectionNameInfo.Name, getCollectionInfoResponse);
        }

        sw.Stop();

        var ret = new ListCollectionInfoResponse()
        {
            Result = collectionInfos,
            Status = QdrantStatus.Success(),
            Time = sw.Elapsed.TotalMinutes
        };

        return ret;
    }

    /// <summary>
    /// Create HNSW index and many payload indexes if they are defined in a fire-and-forget manner.
    /// Pass logger when constructing <see cref="QdrantHttpClient"/> to see any errors that may happen during this operation.
    /// </summary>
    /// <param name="collectionName">Collection name to create indexes for.</param>
    /// <param name="payloadIndexes">Collection payload index definitions that describe payload indexes to create after the HNSW index creation has been successfully issued.</param>
    /// <param name="collectionIndexingThreshold">
    /// Optional Maximum size (in KiloBytes) of vectors allowed for plain index.
    /// If not set the default value of 10000 is used.
    /// </param>
    public void StartCreatingCollectionIndexes(
        string collectionName,
        ICollection<CollectionPayloadIndexDefinition> payloadIndexes,
        uint? collectionIndexingThreshold = null)
    {
        Task.Run(async () => {
            try
            {
                var collectionParametersUpdateResponse = await UpdateCollectionParameters(
                    collectionName,
                    new UpdateCollectionParametersRequest()
                    {
                        OptimizersConfig = new OptimizersConfiguration()
                        {
                            IndexingThreshold = collectionIndexingThreshold ?? DEFAULT_COLLECTION_INDEXING_THRESHOLD
                        }
                    },
                    CancellationToken.None,
                    timeout: _defaultOperationTimeout);

                if (!collectionParametersUpdateResponse.Status.IsSuccess)
                {
                    _logger.LogError(
                        "Failed to start collection {CollectionName} HNSW index creation. Details: {ErrorMessage}",
                        collectionName,
                        collectionParametersUpdateResponse.Status.GetErrorMessage());

                    return;
                }

                // Then - create many payload indexes if they are defined

                if (payloadIndexes is null or {Count: 0})
                {
                    return;
                }

                foreach (var payloadIndexDefinition in payloadIndexes)
                {
                    var createPayloadIndexResponse = await CreatePayloadIndex(
                        collectionName,
                        payloadIndexDefinition.PayloadIndexedFieldName,
                        payloadIndexDefinition.PayloadIndexedFieldSchema,
                        CancellationToken.None,
                        onDisk: payloadIndexDefinition.OnDisk,
                        isTenant: payloadIndexDefinition.IsTenant,
                        isPrincipal: payloadIndexDefinition.IsPrincipal,
                        isWaitForResult: false);

                    if (!createPayloadIndexResponse.Status.IsSuccess)
                    {
                        _logger.LogError(
                            "Failed to create payload index {PayloadIndex} for collection {CollectionName}. Details: {ErrorMessage}",
                            payloadIndexDefinition.ToString(),
                            collectionName,
                            createPayloadIndexResponse.Status.GetErrorMessage());

                        return;
                    }
                }

                _logger.LogInformation(
                    "Successfully started collection {CollectionName} HNSW and payload indexes [{PayloadIndexDefinitions}] creation",
                    collectionName,
                    string.Join(", ", payloadIndexes.Select(x => x.ToString()))
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception during collection {CollectionName} indexes creation start: {ExceptionMessage}",
                    collectionName,
                    ex.Message);
            }
        });
    }
}
