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
    /// <summary>
    /// Get the detailed information about specified existing collection.
    /// </summary>
    /// <param name="collectionName">Collection name to get info for.</param>
    /// <param name="isCountExactPointsNumber">If set to <c>true</c> counts the exact number of points in collection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<GetCollectionInfoResponse> GetCollectionInfo(
        string collectionName,
        bool isCountExactPointsNumber,
        CancellationToken cancellationToken)
    {
        var getCollectionInfoResponse = await GetCollectionInfo(collectionName, cancellationToken);

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
    public async Task<ListCollectionInfoResponse> ListCollectionInfo(
        bool isCountExactPointsNumber,
        CancellationToken cancellationToken)
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
                cancellationToken)
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
    /// <param name="collectionIndexingThreshold">
    /// Maximum size (in KiloBytes) of vectors allowed for plain index.
    /// To disable vector indexing, set to <c>0</c>.
    /// Note: 1kB = 1 vector of size 256.
    /// </param>
    /// <param name="payloadIndexes">Collection payload index definitions that describe payload indexes to create after the HNSW index creation has been successfully issued.</param>
    public void StartCreatingCollectionIndexes(
        string collectionName,
        uint collectionIndexingThreshold,
        ICollection<CollectionPayloadIndexDefinition> payloadIndexes)
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
                            IndexingThreshold = collectionIndexingThreshold
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

                foreach (var payloadIndex in payloadIndexes)
                {
                    var createPayloadIndexResponse = await CreatePayloadIndex(
                        collectionName,
                        payloadIndex.PayloadIndexedFieldName,
                        payloadIndex.PayloadIndexedFieldSchema,
                        CancellationToken.None,
                        onDisk: payloadIndex.OnDisk,
                        isTenant: payloadIndex.IsTenant,
                        isPrincipal: payloadIndex.IsPrincipal,
                        isWaitForResult: false);

                    if (!createPayloadIndexResponse.Status.IsSuccess)
                    {
                        _logger.LogError(
                            "Failed to create payload index {PayloadIndex} for collection {CollectionName}. Details: {ErrorMessage}",
                            payloadIndex.ToString(),
                            collectionName,
                            createPayloadIndexResponse.Status.GetErrorMessage());

                        return;
                    }
                }
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
