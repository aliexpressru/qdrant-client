using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Diagnostics.Helpers;
using Aer.QdrantClient.Http.Diagnostics.Tracing;
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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(DeletePoints),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DeletePoints), null);

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
            collectionName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(DeletePoints),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DeletePoints), null);

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
            collectionName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<PointsOperationResponse> UpsertPoints(
        string collectionName,
        UpsertPointsRequest upsertPoints,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(UpsertPoints),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(UpsertPoints), null);

        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<UpsertPointsRequest, PointsOperationResponse>(
            url,
            HttpMethod.Put,
            upsertPoints,
            collectionName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<PointsOperationResponse> SetPointsPayload(
        string collectionName,
        SetPointsPayloadRequest setPointsPayload,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(SetPointsPayload),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(SetPointsPayload), null);

        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/payload?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<SetPointsPayloadRequest, PointsOperationResponse>(
            url,
            HttpMethod.Post,
            setPointsPayload,
            collectionName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<PointsOperationResponse> OverwritePointsPayload(
        string collectionName,
        OverwritePointsPayloadRequest overwritePointsPayload,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(OverwritePointsPayload),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(OverwritePointsPayload), null);

        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/payload?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<OverwritePointsPayloadRequest, PointsOperationResponse>(
            url,
            // this is a hack due to update payload part not working when issuing using PUT request
            overwritePointsPayload.Key is not null
                ? HttpMethod.Post
                : HttpMethod.Put,
            overwritePointsPayload,
            collectionName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(DeletePointsPayloadKeys),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DeletePointsPayloadKeys), null);

        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/payload/delete?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<DeletePointsPayloadKeysRequest, PointsOperationResponse>(
            url,
            HttpMethod.Post,
            deletePointsPayloadKeys,
            collectionName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(ClearPointsPayload),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(ClearPointsPayload), null);

        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/payload/clear?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<ClearPointsPayloadRequest, PointsOperationResponse>(
            url,
            HttpMethod.Post,
            clearPointsPayload,
            collectionName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(UpdatePointsVectors),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(UpdatePointsVectors), null);

        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/vectors?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<UpdatePointsVectorsRequest, PointsOperationResponse>(
            url,
            HttpMethod.Put,
            updatePointsVectors,
            collectionName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(DeletePointsVectors),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DeletePointsVectors), null);

        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/vectors/delete?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<DeletePointsVectorsRequest, PointsOperationResponse>(
            url,
            HttpMethod.Post,
            deletePointsVectors,
            collectionName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(BatchUpdate),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(BatchUpdate), null);

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
            collectionName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(GetPoint),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(GetPoint), null);

        var pointIdValue = pointId.ToString(false);

        var url = $"/collections/{collectionName}/points/{pointIdValue}";

        var response = await ExecuteRequest<GetPointResponse>(
            url,
            HttpMethod.Get,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(GetPoints),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(GetPoints), null);

        var getPointsRequest = new GetPointsRequest
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
            getPointsRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(ScrollPoints),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(ScrollPoints), null);

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
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(CountPoints),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(CountPoints), null);

        var url = $"/collections/{collectionName}/points/count";

        var response = await ExecuteRequest<CountPointsRequest, CountPointsResponse>(
            url,
            HttpMethod.Post,
            countPointsRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(FacetCountPoints),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(FacetCountPoints), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/facet?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<FacetCountPointsRequest, FacetCountPointsResponse>(
            url,
            HttpMethod.Post,
            facetCountPointsRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(SearchPoints),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(SearchPoints), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/search?consistency={consistencyValue}";

        var response = await ExecuteRequest<SearchPointsRequest, SearchPointsResponse>(
            url,
            HttpMethod.Post,
            searchPointsRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(SearchPointsBatched),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(SearchPointsBatched), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/search/batch?consistency={consistencyValue}";

        var response = await ExecuteRequest<SearchPointsBatchedRequest, SearchPointsBatchedResponse>(
            url,
            HttpMethod.Post,
            searchPointsBatchedRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(SearchPointsGrouped),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(SearchPointsGrouped), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/search/groups?consistency={consistencyValue}";

        var response = await ExecuteRequest<SearchPointsGroupedRequest, SearchPointsGroupedResponse>(
            url,
            HttpMethod.Post,
            searchPointsGroupedRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(SearchPointsDistanceMatrixPairs),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(SearchPointsDistanceMatrixPairs), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/search/matrix/pairs?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<SearchPointsDistanceMatrixRequest, SearchPointsDistanceMatrixPairsResponse>(
            url,
            HttpMethod.Post,
            searchPointsDistanceMatrixRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(SearchPointsDistanceMatrixOffsets),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(SearchPointsDistanceMatrixOffsets), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/search/matrix/offsets?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response =
            await ExecuteRequest<SearchPointsDistanceMatrixRequest, SearchPointsDistanceMatrixOffsetsResponse>(
                url,
                HttpMethod.Post,
                searchPointsDistanceMatrixRequest,
                collectionName,
                cancellationToken,
                retryCount,
                retryDelay,
                onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(RecommendPoints),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(RecommendPoints), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/recommend?consistency={consistencyValue}";

        var response = await ExecuteRequest<RecommendPointsRequest, SearchPointsResponse>(
            url,
            HttpMethod.Post,
            recommendPointsRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(RecommendPointsBatched),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(RecommendPointsBatched), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/recommend/batch?consistency={consistencyValue}";

        var response = await ExecuteRequest<RecommendPointsBatchedRequest, SearchPointsBatchedResponse>(
            url,
            HttpMethod.Post,
            recommendPointsBatchedRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(RecommendPointsGrouped),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(RecommendPointsGrouped), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/recommend/groups?consistency={consistencyValue}";

        var response = await ExecuteRequest<RecommendPointsGroupedRequest, SearchPointsGroupedResponse>(
            url,
            HttpMethod.Post,
            recommendPointsGroupedRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(DiscoverPoints),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DiscoverPoints), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/discover?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<DiscoverPointsRequest, SearchPointsResponse>(
            url,
            HttpMethod.Post,
            discoverPointsRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(DiscoverPointsBatched),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(DiscoverPointsBatched), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/discover/batch?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<DiscoverPointsBatchedRequest, SearchPointsBatchedResponse>(
            url,
            HttpMethod.Post,
            discoverPointsBatchedRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(QueryPoints),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(QueryPoints), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/query?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<QueryPointsRequest, QueryPointsResponse>(
            url,
            HttpMethod.Post,
            queryPointsRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(QueryPointsBatched),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(QueryPointsBatched), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/query/batch?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<QueryPointsBatchedRequest, QueryPointsBatchedResponse>(
            url,
            HttpMethod.Post,
            queryPointsRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

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
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(QueryPointsGrouped),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(collectionName, nameof(QueryPointsGrouped), null);

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/query/groups?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<QueryPointsGroupedRequest, SearchPointsGroupedResponse>(
            url,
            HttpMethod.Post,
            queryPointsRequest,
            collectionName,
            cancellationToken,
            retryCount,
            retryDelay,
            onRetry);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }

    #endregion
}
