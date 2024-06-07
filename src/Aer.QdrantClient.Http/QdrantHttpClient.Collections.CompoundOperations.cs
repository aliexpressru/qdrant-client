using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

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
    /// <param name="isCountExactPointsNumber"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
}
