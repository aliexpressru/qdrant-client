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
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
            Time = sw.Elapsed.TotalSeconds
        };

        return ret;
    }

    /// <inheritdoc/>
    public void StartCreatingCollectionPayloadIndexes(
        string collectionName,
        ICollection<CollectionPayloadIndexDefinition> payloadIndexes) =>
        Task.Run(async () =>
            {
                try
                {
                    if (payloadIndexes is null or { Count: 0 })
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
            }
        );
}
