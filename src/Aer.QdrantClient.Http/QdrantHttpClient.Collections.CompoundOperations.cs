using Aer.QdrantClient.Http.Diagnostics.Helpers;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
        Action<Exception, TimeSpan, int, uint> onRetry = null,
        string clusterName = null)
    {
        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(GetCollectionInfo), clusterName);

        var response = await GetCollectionInfo(
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry,
            clusterName);

        if (!response.Status.IsSuccess)
        {
            return response;
        }

        if (isCountExactPointsNumber)
        {
            var countPointsResponse = (await CountPoints(
                collectionName,
                new CountPointsRequest(
                    isCountExactPointsNumber: true,
                    filter: QdrantFilter.Empty),
                cancellationToken)).EnsureSuccess();

            response.Result.PointsCount = countPointsResponse.Count;
        }

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<ListCollectionInfoResponse> ListCollectionInfo(
        bool isCountExactPointsNumber,
        CancellationToken cancellationToken,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null,
        string clusterName = null)
    {
        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(ListCollectionInfo), clusterName);

        Stopwatch sw = Stopwatch.StartNew();

        var listCollectionsResponse =
            (await ListCollections(cancellationToken, clusterName)).EnsureSuccess();

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
                    onRetry,
                    clusterName)
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

        if (ret.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
                    using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(StartCreatingCollectionPayloadIndexes), null);

                    if (payloadIndexes is null or { Count: 0 })
                    {
                        diagnostic.SetSuccess();

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
                            if (Logger.IsEnabled(LogLevel.Error) == true)
                            {
                                Logger.LogError(
                                    "Failed to create payload index {PayloadIndex} for collection {CollectionName}. Details: {ErrorMessage}",
                                    payloadIndexDefinition.ToString(),
                                    collectionName,
                                    createPayloadIndexResponse.Status.GetErrorMessage()
                                );
                            }

                            return;
                        }
                    }

                    if (Logger.IsEnabled(LogLevel.Information) == true)
                    {
                        Logger.LogInformation(
                            "Successfully started collection {CollectionName} HNSW and payload indexes [{PayloadIndexDefinitions}] creation",
                            collectionName,
                            string.Join(", ", payloadIndexes.Select(x => x.ToString()))
                        );
                    }

                    diagnostic.SetSuccess();
                }
                catch (Exception ex)
                {
                    if (Logger.IsEnabled(LogLevel.Error) == true)
                    {
                        Logger.LogError("Exception during collection {CollectionName} indexes creation start: {ExceptionMessage}",
                        collectionName,
                        ex.Message);
                    }
                }
            }
        );
}
