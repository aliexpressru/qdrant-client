using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Http;

/// <summary>
/// Interface for Qdrant HTTP API client.
/// </summary>
public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Delete points by specified ids.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete points from.</param>
    /// <param name="pointIds">The point ids to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="shardSelector">The shard selector. If set, performs operation only on specified shard(s).</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The delete operation ordering settings.</param>
    Task<PointsOperationResponse> DeletePoints(
        string collectionName,
        IEnumerable<PointId> pointIds,
        CancellationToken cancellationToken,
        ShardSelector shardSelector,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Delete points by specified filters.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete points from.</param>
    /// <param name="filter">The filter to find points to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="shardSelector">The shard selector. If set, performs operation only on specified shard(s).</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The delete operation ordering settings.</param>
    Task<PointsOperationResponse> DeletePoints(
        string collectionName,
        QdrantFilter filter,
        CancellationToken cancellationToken,
        ShardSelector shardSelector,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Delete specified payload keys for points.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete payload for.</param>
    /// <param name="deletePointsPayloadKeys">Delete points payload keys request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    Task<PointsOperationResponse> DeletePointsPayloadKeys(
        string collectionName,
        DeletePointsPayloadKeysRequest deletePointsPayloadKeys,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Delete specified payload keys for points.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete payload for.</param>
    /// <param name="clearPointsPayload">Clear points payload request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    Task<PointsOperationResponse> ClearPointsPayload(
        string collectionName,
        ClearPointsPayloadRequest clearPointsPayload,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Update specified named vectors on points, keep unspecified vectors intact.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete payload for.</param>
    /// <param name="updatePointsVectors">Update points vectors request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    Task<PointsOperationResponse> UpdatePointsVectors(
        string collectionName,
        UpdatePointsVectorsRequest updatePointsVectors,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Delete named vectors from the given points.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete payload for.</param>
    /// <param name="deletePointsVectors">Delete points vectors request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    Task<PointsOperationResponse> DeletePointsVectors(
        string collectionName,
        DeletePointsVectorsRequest deletePointsVectors,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Apply a series of update operations for points, vectors and payloads.
    /// Operations are executed sequentially in order of appearance in <see cref="BatchUpdatePointsRequest"/>.
    /// </summary>
    /// <param name="collectionName">Name of the collection to apply operations to.</param>
    /// <param name="batchUpdatePointsRequest">The request with operation sequence definition.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    Task<BatchPointsOperationResponse> BatchUpdate(
        string collectionName,
        BatchUpdatePointsRequest batchUpdatePointsRequest,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering);

    /// <summary>
    /// Retrieve full information of single point by id.
    /// </summary>
    /// <param name="collectionName">Name of the collection to retrieve point from.</param>
    /// <param name="pointId">The identifier of the point to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<GetPointResponse> GetPoint(
        string collectionName,
        PointId pointId,
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieve multiple points by specified ids.
    /// </summary>
    /// <param name="collectionName">Name of the collection to retrieve from.</param>
    /// <param name="pointIds">The point ids to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="withPayload">Whether the whole payload or only selected payload properties should be returned with the response.</param>
    /// <param name="withVector">Whether the vector, all named vectors or only selected named vectors should be returned with the response.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<GetPointsResponse> GetPoints(
        string collectionName,
        IEnumerable<PointId> pointIds,
        PayloadPropertiesSelector withPayload,
        CancellationToken cancellationToken,
        VectorSelector withVector,
        ReadPointsConsistency consistency,
        ShardSelector shardSelector,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

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
    /// <param name="shardSelector">
    /// The shard selector. If set performs operation on specified shard(s).
    /// If not set - performs operation on all shards.
    /// </param>
    /// <param name="orderBySelector">
    /// The ordering field and direction selector.
    /// You can pass a string payload field name value which would be interpreted as order by the specified field in ascending order.
    /// </param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<ScrollPointsResponse> ScrollPoints(
        string collectionName,
        QdrantFilter filter,
        PayloadPropertiesSelector withPayload,
        CancellationToken cancellationToken,
        ulong limit,
        PointId offsetPoint,
        VectorSelector withVector,
        ReadPointsConsistency consistency,
        ShardSelector shardSelector,
        OrderBySelector orderBySelector,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Count points which matches given filtering condition.
    /// </summary>
    /// <param name="collectionName">Name of the collection to count points in.</param>
    /// <param name="countPointsRequest">The count points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<CountPointsResponse> CountPoints(
        string collectionName,
        CountPointsRequest countPointsRequest,
        CancellationToken cancellationToken,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieves facets for the specified payload field.
    /// </summary>
    /// <param name="collectionName">Name of the collection to facet count points in.</param>
    /// <param name="facetCountPointsRequest">The facet count points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<FacetCountPointsResponse> FacetCountPoints(
        string collectionName,
        FacetCountPointsRequest facetCountPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieve the closest points based on vector similarity and given filtering conditions.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="searchPointsRequest">The search points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsResponse> SearchPoints(
        string collectionName,
        SearchPointsRequest searchPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieve the closest points based on vector similarity and given filtering conditions.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="searchPointsBatchedRequest">The search points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsBatchedResponse> SearchPointsBatched(
        string collectionName,
        SearchPointsBatchedRequest searchPointsBatchedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieve the closest points based on vector similarity and given filtering conditions, grouped by a given payload field.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="searchPointsGroupedRequest">The search points grouped request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsGroupedResponse> SearchPointsGrouped(
        string collectionName,
        SearchPointsGroupedRequest searchPointsGroupedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieves sparse matrix of pairwise distances between points sampled from the collection. Output is a list of pairs of points and their distances.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="searchPointsDistanceMatrixRequest">The search points distance matrix request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsDistanceMatrixPairsResponse> SearchPointsDistanceMatrixPairs(
        string collectionName,
        SearchPointsDistanceMatrixRequest searchPointsDistanceMatrixRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Retrieves sparse matrix of pairwise distances between points sampled from the collection. Output is a form of row and column offsets and list of distances.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="searchPointsDistanceMatrixRequest">The search points distance matrix request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsDistanceMatrixOffsetsResponse> SearchPointsDistanceMatrixOffsets(
        string collectionName,
        SearchPointsDistanceMatrixRequest searchPointsDistanceMatrixRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Look for the points which are closer to stored positive examples and at the same time further to negative examples.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="recommendPointsRequest">The recommend points request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsResponse> RecommendPoints(
        string collectionName,
        RecommendPointsRequest recommendPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Look for the points which are closer to stored positive examples and at the same time further to negative examples.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="recommendPointsBatchedRequest">The recommend points batched request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsBatchedResponse> RecommendPointsBatched(
        string collectionName,
        RecommendPointsBatchedRequest recommendPointsBatchedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Look for the points which are closer to stored positive examples
    /// and at the same time further to negative examples, grouped by a given payload field.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="recommendPointsGroupedRequest">The recommend points grouped request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsGroupedResponse> RecommendPointsGrouped(
        string collectionName,
        RecommendPointsGroupedRequest recommendPointsGroupedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

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
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsResponse> DiscoverPoints(
        string collectionName,
        DiscoverPointsRequest discoverPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Look for points based on target and/or positive and negative example pairs, in batch.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="discoverPointsBatchedRequest">The discover points batched request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsBatchedResponse> DiscoverPointsBatched(
        string collectionName,
        DiscoverPointsBatchedRequest discoverPointsBatchedRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Universally query points. This endpoint covers all capabilities of search, recommend, discover, filters.
    /// But also enables hybrid and multi-stage queries.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="queryPointsRequest">The universal query API request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<QueryPointsResponse> QueryPoints(
        string collectionName,
        QueryPointsRequest queryPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Universally query points in batch. This endpoint covers all capabilities of search, recommend, discover, filters.
    /// But also enables hybrid and multi-stage queries.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="queryPointsRequest">The universal query API request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<QueryPointsBatchedResponse> QueryPointsBatched(
        string collectionName,
        QueryPointsBatchedRequest queryPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Universally query points and group results by a specified payload field.
    /// This endpoint covers all capabilities of search, recommend, discover, filters.
    /// But also enables hybrid and multi-stage queries.
    /// </summary>
    /// <param name="collectionName">Name of the collection to search in.</param>
    /// <param name="queryPointsRequest">The universal query API request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="consistency">The consistency settings.</param>
    /// <param name="timeout">Wait for operation commit timeout. If timeout is reached - request will return with service error.</param>
    /// <param name="retryCount">Operation retry count. Set to <c>null</c> to disable retry.</param>
    /// <param name="retryDelay">Operation retry delay. Set to <c>null</c> to retry immediately.</param>
    /// <param name="onRetry">
    /// The action to be called on operation retry.
    /// Parameters : Exception that happened during operation execution, delay before the next retry, retry number and max retry count.
    /// </param>
    Task<SearchPointsGroupedResponse> QueryPointsGrouped(
        string collectionName,
        QueryPointsGroupedRequest queryPointsRequest,
        CancellationToken cancellationToken,
        ReadPointsConsistency consistency,
        TimeSpan? timeout,
        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

}
