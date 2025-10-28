using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    #region Create \ Update \ Delete operations

    /// <inheritdoc/>
    public async Task<PointsOperationResponse> DeletePoints(
        string collectionName,
        IEnumerable<PointId> pointIds,
        CancellationToken cancellationToken,
        ShardSelector shardSelector = null,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
    {
        var points = new DeletePointsRequest()
        {
            Points = pointIds,
            ShardKey = shardSelector
        };

        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/delete?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<DeletePointsRequest, PointsOperationResponse>(
            url,
            HttpMethod.Post,
            points,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<PointsOperationResponse> DeletePoints(
        string collectionName,
        QdrantFilter filter,
        CancellationToken cancellationToken,
        ShardSelector shardSelector = null,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
    {
        var points = new DeletePointsRequest()
        {
            Filter = filter,
            ShardKey = shardSelector
        };

        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/delete?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<DeletePointsRequest, PointsOperationResponse>(
            url,
            HttpMethod.Post,
            points,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<PointsOperationResponse> UpsertPoints<TPayload>(
        string collectionName,
        UpsertPointsRequest<TPayload> upsertPoints,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
        where TPayload : class
    {
        if (typeof(TPayload) == typeof(string))
        {
            throw new QdrantInvalidPayloadTypeException(typeof(TPayload).FullName);
        }

        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<UpsertPointsRequest<TPayload>, PointsOperationResponse>(
            url,
            HttpMethod.Put,
            upsertPoints,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<PointsOperationResponse> SetPointsPayload<TPayload>(
        string collectionName,
        SetPointsPayloadRequest<TPayload> setPointsPayload,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
        where TPayload : class
    {
        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/payload?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<SetPointsPayloadRequest<TPayload>, PointsOperationResponse>(
            url,
            HttpMethod.Post,
            setPointsPayload,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<PointsOperationResponse> OverwritePointsPayload<TPayload>(
        string collectionName,
        OverwritePointsPayloadRequest<TPayload> overwritePointsPayload,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
        where TPayload : class
    {
        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/payload?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<OverwritePointsPayloadRequest<TPayload>, PointsOperationResponse>(
            url,
            // this is a hack due to update payload part not working when issuing using PUT request
            overwritePointsPayload.Key is not null
                ? HttpMethod.Post
                : HttpMethod.Put,
            overwritePointsPayload,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<PointsOperationResponse> DeletePointsPayloadKeys(
        string collectionName,
        DeletePointsPayloadKeysRequest deletePointsPayloadKeys,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
    {
        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/payload/delete?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<DeletePointsPayloadKeysRequest, PointsOperationResponse>(
            url,
            HttpMethod.Post,
            deletePointsPayloadKeys,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<PointsOperationResponse> ClearPointsPayload(
        string collectionName,
        ClearPointsPayloadRequest clearPointsPayload,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
    {
        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/payload/clear?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<ClearPointsPayloadRequest, PointsOperationResponse>(
            url,
            HttpMethod.Post,
            clearPointsPayload,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<PointsOperationResponse> UpdatePointsVectors(
        string collectionName,
        UpdatePointsVectorsRequest updatePointsVectors,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
    {
        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/vectors?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<UpdatePointsVectorsRequest, PointsOperationResponse>(
            url,
            HttpMethod.Put,
            updatePointsVectors,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<PointsOperationResponse> DeletePointsVectors(
        string collectionName,
        DeletePointsVectorsRequest deletePointsVectors,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
    {
        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/vectors/delete?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<DeletePointsVectorsRequest, PointsOperationResponse>(
            url,
            HttpMethod.Post,
            deletePointsVectors,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <inheritdoc/>
    public async Task<BatchPointsOperationResponse> BatchUpdate(
        string collectionName,
        BatchUpdatePointsRequest batchUpdatePointsRequest,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
    {
        if (batchUpdatePointsRequest.OperationsCount == 0)
        {
            throw new QdrantEmptyBatchRequestException(
                collectionName,
                nameof(BatchUpdate),
                batchUpdatePointsRequest.GetType());
        }

        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/batch?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<BatchUpdatePointsRequest, BatchPointsOperationResponse>(
            url,
            HttpMethod.Post,
            batchUpdatePointsRequest,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    #endregion

    #region Read \ Count operations
    /// <inheritdoc/>
    public async Task<GetPointResponse> GetPoint(
        string collectionName,
        PointId pointId,
        CancellationToken cancellationToken,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var pointIdValue = pointId.ToString(false);
        
        var url = $"/collections/{collectionName}/points/{pointIdValue}";

        var response = await ExecuteRequest<GetPointResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<GetPointsResponse> GetPoints(
        string collectionName,
        IEnumerable<PointId> pointIds,
        PayloadPropertiesSelector withPayload,
        CancellationToken cancellationToken,
        VectorSelector withVector = null,
        ReadPointsConsistency consistency = null,
        ShardSelector shardSelector = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var points = new GetPointsRequest
        {
            Ids = pointIds,
            WithPayload = withPayload,
            WithVector = withVector ?? VectorSelector.None,
            ShardKey = shardSelector,
        };

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points?consistency={consistencyValue}";

        var response = await ExecuteRequest<GetPointsRequest, GetPointsResponse>(
            url,
            HttpMethod.Post,
            points,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<ScrollPointsResponse> ScrollPoints(
        string collectionName,
        QdrantFilter filter,
        PayloadPropertiesSelector withPayload,
        CancellationToken cancellationToken,
        ulong limit = 10,
        PointId offsetPoint = null,
        VectorSelector withVector = null,
        ReadPointsConsistency consistency = null,
        ShardSelector shardSelector = null,
        OrderBySelector orderBySelector = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var scrollRequest = new ScrollPointsRequest()
        {
            Filter = filter,
            Limit = limit,
            Offset = offsetPoint,
            WithPayload = withPayload,
            WithVector = withVector ?? VectorSelector.None,
            ShardKey = shardSelector,
            OrderBy = orderBySelector
        };

        var url = $"/collections/{collectionName}/points/scroll?consistency={consistencyValue}";

        var response = await ExecuteRequest<ScrollPointsRequest, ScrollPointsResponse>(
            url,
            HttpMethod.Post,
            scrollRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<CountPointsResponse> CountPoints(
        string collectionName,
        CountPointsRequest countPointsRequest,
        CancellationToken cancellationToken,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var url = $"/collections/{collectionName}/points/count";

        var response = await ExecuteRequest<CountPointsRequest, CountPointsResponse>(
            url,
            HttpMethod.Post,
            countPointsRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<FacetCountPointsResponse> FacetCountPoints(
        string collectionName,
        FacetCountPointsRequest facetCountPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/facet?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<FacetCountPointsRequest, FacetCountPointsResponse>(
            url,
            HttpMethod.Post,
            facetCountPointsRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;

    }

    #endregion

    #region Search operations

    /// <inheritdoc/>
    public async Task<SearchPointsResponse> SearchPoints(
        string collectionName,
        SearchPointsRequest searchPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/search?consistency={consistencyValue}";

        var response = await ExecuteRequest<SearchPointsRequest, SearchPointsResponse>(
            url,
            HttpMethod.Post,
            searchPointsRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<SearchPointsBatchedResponse> SearchPointsBatched(
        string collectionName,
        SearchPointsBatchedRequest searchPointsBatchedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/search/batch?consistency={consistencyValue}";

        var response = await ExecuteRequest<SearchPointsBatchedRequest, SearchPointsBatchedResponse>(
            url,
            HttpMethod.Post,
            searchPointsBatchedRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<SearchPointsGroupedResponse> SearchPointsGrouped(
        string collectionName,
        SearchPointsGroupedRequest searchPointsGroupedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/search/groups?consistency={consistencyValue}";

        var response = await ExecuteRequest<SearchPointsGroupedRequest, SearchPointsGroupedResponse>(
            url,
            HttpMethod.Post,
            searchPointsGroupedRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<SearchPointsDistanceMatrixPairsResponse> SearchPointsDistanceMatrixPairs(
        string collectionName,
        SearchPointsDistanceMatrixRequest searchPointsDistanceMatrixRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/search/matrix/pairs?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<SearchPointsDistanceMatrixRequest, SearchPointsDistanceMatrixPairsResponse>(
            url,
            HttpMethod.Post,
            searchPointsDistanceMatrixRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<SearchPointsDistanceMatrixOffsetsResponse> SearchPointsDistanceMatrixOffsets(
        string collectionName,
        SearchPointsDistanceMatrixRequest searchPointsDistanceMatrixRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/search/matrix/offsets?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response =
            await ExecuteRequest<SearchPointsDistanceMatrixRequest, SearchPointsDistanceMatrixOffsetsResponse>(
                url,
                HttpMethod.Post,
                searchPointsDistanceMatrixRequest,
                cancellationToken,
                retryCount,
                retryDelay,
                onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<SearchPointsResponse> RecommendPoints(
        string collectionName,
        RecommendPointsRequest recommendPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/recommend?consistency={consistencyValue}";

        var response = await ExecuteRequest<RecommendPointsRequest, SearchPointsResponse>(
            url,
            HttpMethod.Post,
            recommendPointsRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<SearchPointsBatchedResponse> RecommendPointsBatched(
        string collectionName,
        RecommendPointsBatchedRequest recommendPointsBatchedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/recommend/batch?consistency={consistencyValue}";

        var response = await ExecuteRequest<RecommendPointsBatchedRequest, SearchPointsBatchedResponse>(
            url,
            HttpMethod.Post,
            recommendPointsBatchedRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<SearchPointsGroupedResponse> RecommendPointsGrouped(
        string collectionName,
        RecommendPointsGroupedRequest recommendPointsGroupedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/recommend/groups?consistency={consistencyValue}";

        var response = await ExecuteRequest<RecommendPointsGroupedRequest, SearchPointsGroupedResponse>(
            url,
            HttpMethod.Post,
            recommendPointsGroupedRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<SearchPointsResponse> DiscoverPoints(
        string collectionName,
        DiscoverPointsRequest discoverPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/discover?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<DiscoverPointsRequest, SearchPointsResponse>(
            url,
            HttpMethod.Post,
            discoverPointsRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<SearchPointsBatchedResponse> DiscoverPointsBatched(
        string collectionName,
        DiscoverPointsBatchedRequest discoverPointsBatchedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/discover/batch?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<DiscoverPointsBatchedRequest, SearchPointsBatchedResponse>(
            url,
            HttpMethod.Post,
            discoverPointsBatchedRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<QueryPointsResponse> QueryPoints(
        string collectionName,
        QueryPointsRequest queryPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/query?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<QueryPointsRequest, QueryPointsResponse>(
            url,
            HttpMethod.Post,
            queryPointsRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<QueryPointsBatchedResponse> QueryPointsBatched(
        string collectionName,
        QueryPointsBatchedRequest queryPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/query/batch?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<QueryPointsBatchedRequest, QueryPointsBatchedResponse>(
            url,
            HttpMethod.Post,
            queryPointsRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    /// <inheritdoc/>
    public async Task<SearchPointsGroupedResponse> QueryPointsGrouped(
        string collectionName,
        QueryPointsGroupedRequest queryPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        TimeSpan? timeout = null,
        uint retryCount = DEFAULT_RETRY_COUNT,
        TimeSpan? retryDelay = null,
        Action<Exception, TimeSpan, int, uint> onRetry = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/query/groups?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<QueryPointsGroupedRequest, SearchPointsGroupedResponse>(
            url,
            HttpMethod.Post,
            queryPointsRequest,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        return response;
    }

    #endregion
}
