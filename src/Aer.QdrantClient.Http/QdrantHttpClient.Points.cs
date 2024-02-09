using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable MemberCanBeInternal

namespace Aer.QdrantClient.Http;

public partial class QdrantHttpClient
{
    #region Create \ Update \ Delete operations

    /// <summary>
    /// Delete points by specified ids.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete points from.</param>
    /// <param name="pointIds">The point ids to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The delete operation ordering settings.</param>
    public async Task<PointsOperationResponse> DeletePoints(
        string collectionName,
        IEnumerable<PointId> pointIds,
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        OrderingType? ordering = null)
    {
        var points = new DeletePointsRequest()
        {
            Points = pointIds
        };

        var orderingValue = (ordering ?? OrderingType.Weak).ToString().ToLowerInvariant();

        var url =
            $"/collections/{collectionName}/points/delete?wait={ToUrlQueryString(isWaitForResult)}&ordering={orderingValue}";

        var response = await ExecuteRequest<DeletePointsRequest, PointsOperationResponse>(
            url,
            HttpMethod.Post,
            points,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Perform insert + updates on points. If point with given id already exists - it will be overwritten.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete points from.</param>
    /// <param name="upsertPoints">The point data to upsert.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    public async Task<PointsOperationResponse> UpsertPoints<TPayload>(
        string collectionName,
        UpsertPointsRequest<TPayload> upsertPoints,
        CancellationToken cancellationToken,
        bool isWaitForResult = true)
        where TPayload : class
    {
        if (typeof(TPayload) == typeof(string))
        {
            throw new QdrantInvalidPayloadTypeException(typeof(TPayload).FullName);
        }

        var url = $"/collections/{collectionName}/points?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<UpsertPointsRequest<TPayload>, PointsOperationResponse>(
            url,
            HttpMethod.Put,
            upsertPoints,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Set payload keys values for points.
    /// Sets only the specified keys leaving all other intact.
    /// </summary>
    /// <param name="collectionName">Name of the collection to set payload for.</param>
    /// <param name="setPointsPayload">Set points payload request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
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
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Replace full payload of points with new one.
    /// </summary>
    /// <param name="collectionName">Name of the collection to set payload for.</param>
    /// <param name="overwritePointsPayload">Overwrite points payload request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
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
            HttpMethod.Put,
            overwritePointsPayload,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Delete specified payload keys for points.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete payload for.</param>
    /// <param name="deletePointsPayloadKeys">Delete points payload keys request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
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
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Delete specified payload keys for points.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete payload for.</param>
    /// <param name="clearPointsPayload">Clear points payload request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
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
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Update specified named vectors on points, keep unspecified vectors intact.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete payload for.</param>
    /// <param name="updatePointsVectors">Update points vectors request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    /// <remarks>Named vectors are not supportd yet so single vector for each point will be updated.</remarks>
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
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Delete named vectors from the given points.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete payload for.</param>
    /// <param name="deletePointsVectors">Delete points vectors request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
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
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Apply a series of update operations for points, vectors and payloads.
    /// Operations are executed sequentially in order of appearance in <see cref="BatchUpdatePointsRequest"/>.
    /// </summary>
    /// <param name="collectionName">Name of the collection to apply operations to.</param>
    /// <param name="batchUpdatePointsRequest">The request with operation sequence definition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
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
            cancellationToken);

        return response;
    }

    #endregion

    #region Read \ Count operations

    /// <summary>
    /// Retrieve full information of single point by id.
    /// </summary>
    /// <param name="collectionName">Name of the collection to retrieve point from.</param>
    /// <param name="pointId">The identifier of the point to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<GetPointResponse> GetPoint(
        string collectionName,
        PointId pointId,
        CancellationToken cancellationToken)
    {
        string url = $"/collections/{collectionName}/points/{pointId}";

        var response = await ExecuteRequest<GetPointResponse>(
            url,
            HttpMethod.Get,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Retrieve multiple points by specified ids.
    /// </summary>
    /// <param name="collectionName">Name of the collection to retrieve from.</param>
    /// <param name="pointIds">The point ids to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="consistency">The consistency settings.</param>
    public async Task<GetPointsResponse> GetPoints(
        string collectionName,
        IEnumerable<PointId> pointIds,
        PayloadPropertiesSelector withPayload,
        CancellationToken cancellationToken,
        VectorSelector withVector = null,
        ReadPointsConsistency consistency = null)
    {
        var points = new GetPointsRequest
        {
            Ids = pointIds,
            WithPayload = withPayload,
            WithVector = withVector ?? VectorSelector.None
        };

        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        string url = $"/collections/{collectionName}/points?consistency={consistencyValue}";

        var response = await ExecuteRequest<GetPointsRequest, GetPointsResponse>(
            url,
            HttpMethod.Post,
            points,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Scroll request - paginate over all points which matches given filtering condition.
    /// </summary>
    /// <param name="collectionName">Name of the collection to retrieve from.</param>
    /// <param name="filter">Look only for points which satisfies this conditions. If not provided - all points.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="limit">Page size. Default: 10.</param>
    /// <param name="offsetPoint">Start ID to read points from.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="consistency">The consistency settings.</param>
    public async Task<ScrollPointsResponse> ScrollPoints(
        string collectionName,
        QdrantFilter filter,
        PayloadPropertiesSelector withPayload,
        CancellationToken cancellationToken,
        ulong limit = 10,
        PointId offsetPoint = null,
        VectorSelector withVector = null,
        ReadPointsConsistency consistency = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var scrollRequest = new ScrollPointsRequest()
        {
            Filter = filter,
            Limit = limit,
            Offset = offsetPoint,
            WithPayload = withPayload,
            WithVector = withVector ?? VectorSelector.None
        };

        string url = $"/collections/{collectionName}/points/scroll?consistency={consistencyValue}";

        var response = await ExecuteRequest<ScrollPointsRequest, ScrollPointsResponse>(
            url,
            HttpMethod.Post,
            scrollRequest,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Count points which matches given filtering condition.
    /// </summary>
    /// <param name="collectionName">Name of the collection to count points in.</param>
    /// <param name="countPointsRequest">The count points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<CountPointsResponse> CountPoints(
        string collectionName,
        CountPointsRequest countPointsRequest,
        CancellationToken cancellationToken)
    {
        var url = $"/collections/{collectionName}/points/count";

        var response = await ExecuteRequest<CountPointsRequest, CountPointsResponse>(
            url,
            HttpMethod.Post,
            countPointsRequest,
            cancellationToken);

        return response;
    }

    #endregion

    #region Search operations

    /// <summary>
    /// Retrieve closest points based on vector similarity and given filtering conditions.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="searchPointsRequest">The search points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    public async Task<SearchPointsResponse> SearchPoints(
        string collectionName,
        SearchPointsRequest searchPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/search?consistency={consistencyValue}";

        var response = await ExecuteRequest<SearchPointsRequest, SearchPointsResponse>(
            url,
            HttpMethod.Post,
            searchPointsRequest,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Retrieve closest points based on vector similarity and given filtering conditions.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="searchPointsBatchedRequest">The search points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    public async Task<SearchPointsBatchedResponse> SearchPointsBatched(
        string collectionName,
        SearchPointsBatchedRequest searchPointsBatchedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/search/batch?consistency={consistencyValue}";

        var response = await ExecuteRequest<SearchPointsBatchedRequest, SearchPointsBatchedResponse>(
            url,
            HttpMethod.Post,
            searchPointsBatchedRequest,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Retrieve closest points based on vector similarity and given filtering conditions, grouped by a given payload field.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="searchPointsGroupedRequest">The search points grouped request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    public async Task<SearchPointsGroupedResponse> SearchPointsGrouped(
        string collectionName,
        SearchPointsGroupedRequest searchPointsGroupedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/search/groups?consistency={consistencyValue}";

        var response = await ExecuteRequest<SearchPointsGroupedRequest, SearchPointsGroupedResponse>(
            url,
            HttpMethod.Post,
            searchPointsGroupedRequest,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Look for the points which are closer to stored positive examples and at the same time further to negative examples.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="recommendPointsRequest">The recommend points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    public async Task<SearchPointsResponse> RecommendPoints(
        string collectionName,
        RecommendPointsByRequest recommendPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/recommend?consistency={consistencyValue}";

        var response = await ExecuteRequest<RecommendPointsByRequest, SearchPointsResponse>(
            url,
            HttpMethod.Post,
            recommendPointsRequest,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Look for the points which are closer to stored positive examples and at the same time further to negative examples.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="recommendPointsBatchedRequest">The recommend points batched request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    public async Task<SearchPointsBatchedResponse> RecommendPointsBatched(
        string collectionName,
        RecommendPointsBatchedRequest recommendPointsBatchedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/recommend/batch?consistency={consistencyValue}";

        var response = await ExecuteRequest<RecommendPointsBatchedRequest, SearchPointsBatchedResponse>(
            url,
            HttpMethod.Post,
            recommendPointsBatchedRequest,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Look for the points which are closer to stored positive examples
    /// and at the same time further to negative examples, grouped by a given payload field.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="recommendPointsGroupedRequest">The recommend points grouped request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    public async Task<SearchPointsGroupedResponse> RecommendPointsGrouped(
        string collectionName,
        RecommendPointsByGroupedRequest recommendPointsGroupedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/recommend/groups?consistency={consistencyValue}";

        var response = await ExecuteRequest<RecommendPointsByGroupedRequest, SearchPointsGroupedResponse>(
            url,
            HttpMethod.Post,
            recommendPointsGroupedRequest,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Use context and a target to find the most similar points to the target, constrained by the context.
    /// When using only the context (without a target), a special search - called context search - is performed
    /// where pairs of points are used to generate a loss that guides the search towards the zone where
    /// most positive examples overlap. This means that the score minimizes the scenario of finding a point
    /// closer to a negative than to a positive part of a pair. Since the score of a context relates to loss,
    /// the maximum score a point can get is <c>0.0</c>, and it becomes normal that many points can have a score of <c>0.0</c>.
    /// <br/>
    /// When using target (with or without context), the score behaves a little different:
    /// The integer part of the score represents the rank with respect to the context, while the decimal part
    /// of the score relates to the distance to the target. The context part of the score for each pair
    /// is calculated <c>+1</c> if the point is closer to a positive than to a negative part of a pair,
    /// and <c>-1</c> otherwise.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="discoverPointsRequest">The discover points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    public async Task<SearchPointsResponse> DiscoverPoints(
        string collectionName,
        DiscoverPointsByRequest discoverPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        TimeSpan? timeout = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url = $"/collections/{collectionName}/points/discover?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<DiscoverPointsByRequest, SearchPointsResponse>(
            url,
            HttpMethod.Post,
            discoverPointsRequest,
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Look for points based on target and/or positive and negative example pairs, in batch.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="discoverPointsBatchedRequest">The discover points batched request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    public async Task<SearchPointsBatchedResponse> DiscoverPointsBatched(
        string collectionName,
        DiscoverPointsBatchedRequest discoverPointsBatchedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency = null,
        TimeSpan? timeout = null)
    {
        var consistencyValue = (consistency ?? ReadPointsConsistency.Default).ToQueryParameterValue();

        var url =
            $"/collections/{collectionName}/points/discover/batch?consistency={consistencyValue}&timeout={GetTimeoutValueOrDefault(timeout)}";

        var response = await ExecuteRequest<DiscoverPointsBatchedRequest, SearchPointsBatchedResponse>(
            url,
            HttpMethod.Post,
            discoverPointsBatchedRequest,
            cancellationToken);

        return response;
    }

    #endregion
}
