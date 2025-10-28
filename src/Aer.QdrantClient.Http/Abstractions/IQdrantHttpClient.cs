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
    Task<GetPeerResponse>
        GetPeerInfoByUriSubstring(
            string clusterNodeUriSubstring,
            CancellationToken cancellationToken);

    Task<PayloadIndexOperationResponse> CreatePayloadIndex(
        string collectionName,
        string payloadFieldName,
        PayloadIndexedFieldType payloadFieldType,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        bool onDisk,

        bool? isTenant,
        bool? isPrincipal,

        bool? isLookupEnabled,
        bool? isRangeEnabled,

        uint retryCount,
        TimeSpan? retryDelay,
        Action<Exception, TimeSpan, int, uint> onRetry);

    /// <summary>
    /// Perform insert + updates on points. If point with given id already exists - it will be overwritten.
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete points from.</param>
    /// <param name="upsertPoints">The point data to upsert.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The upsert operation ordering settings.</param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    Task<PointsOperationResponse> UpsertPoints<TPayload>(
        string collectionName,
        UpsertPointsRequest<TPayload> upsertPoints,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering)
        where TPayload : class;

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
    Task<PointsOperationResponse> SetPointsPayload<TPayload>(
        string collectionName,
        SetPointsPayloadRequest<TPayload> setPointsPayload,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering)
        where TPayload : class;

    /// <summary>
    /// Replace full payload of points with new one.
    /// </summary>
    /// <param name="collectionName">Name of the collection to set payload for.</param>
    /// <param name="overwritePointsPayload">Overwrite points payload request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen.</param>
    /// <param name="ordering">The batch update operation ordering settings.</param>
    /// <typeparam name="TPayload">The type of the point payload.</typeparam>
    Task<PointsOperationResponse> OverwritePointsPayload<TPayload>(
        string collectionName,
        OverwritePointsPayloadRequest<TPayload> overwritePointsPayload,
        CancellationToken cancellationToken,
        bool isWaitForResult,
        OrderingType? ordering)
        where TPayload : class;

    Task<SetLockOptionsResponse> SetLockOptions(
        bool areWritesDisabled,
        string reasonMessage,
        CancellationToken cancellationToken);

    Task<SetLockOptionsResponse> GetLockOptions(CancellationToken cancellationToken);

    Task<ReportIssuesResponse> ReportIssues(CancellationToken cancellationToken);

    Task<ClearReportedIssuesResponse> ClearIssues(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously wait until the collection status becomes <see cref="QdrantCollectionStatus.Green"/>
    /// and collection optimizer status becomes <see cref="QdrantOptimizerStatus.Ok"/>.
    /// </summary>
    /// <param name="collectionName">The name of the collection to check status for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="pollingInterval">The collection status polling interval. Is not set the default polling interval is 1 second.</param>
    /// <param name="timeout">The timeout after which the collection considered not green and exception is thrown. The default timeout is 30 seconds.</param>
    /// <param name="requiredNumberOfGreenCollectionResponses">The number of green status responses to be received
    /// for collection status to be considered green. To increase the probability that every node has
    /// the same green status - set this parameter to a value greater than the number of nodes.</param>
    /// <param name="isCheckShardTransfersCompleted">
    /// If set to <c>true</c> check that all collection shard transfers are completed.
    /// The collection is not considered ready until all shard transfers are completed.
    /// </param>
    Task EnsureCollectionReady(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? pollingInterval,
        TimeSpan? timeout,
        uint requiredNumberOfGreenCollectionResponses,
        bool isCheckShardTransfersCompleted);

}
