using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <summary>
    /// Gets the Qdrant instance details.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<GetInstanceDetailsResponse> GetInstanceDetails(CancellationToken cancellationToken)
    {
        var url = "/";
        
        HttpRequestMessage message = new(HttpMethod.Get, url);

        var response = await ExecuteRequest<GetInstanceDetailsResponse>(
            message,
            cancellationToken);

        return response;
    }
    
    /// <summary>
    /// Get the Qdrant telemetry information.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="detailsLevel">Defines how detailed the telemetry data is.</param>
    /// <param name="isAnonymizeTelemetryData">If set tot <c>true</c>, anonymize the collected telemetry result.</param>
    public async Task<GetTelemetryResponse> GetTelemetry(
        CancellationToken cancellationToken,
        uint detailsLevel = 3,
        bool isAnonymizeTelemetryData = true)
    {
        var url = $"/telemetry?details_level={detailsLevel}&anonymize={ToUrlQueryString(isAnonymizeTelemetryData)}";

        var response = await ExecuteRequest<GetTelemetryResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Collect metrics data including app info, collections info, cluster info and statistics in Prometheus format.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isAnonymizeMetricsData">If set tot <c>true</c>, anonymize the collected metrics result.</param>
    public async Task<string> GetPrometheusMetrics(
        CancellationToken cancellationToken,
        bool isAnonymizeMetricsData = true)
    {
        var url = $"/metrics?anonymize={ToUrlQueryString(isAnonymizeMetricsData)}";

        HttpRequestMessage message = new(HttpMethod.Get, url);

        var result = await ExecuteRequest<string>(message, cancellationToken);

        return result;
    }

    /// <summary>
    /// Set write lock options to disable any writes.
    /// If write is locked, all write operations and collection creation are forbidden.
    /// However, deletion operations or updates are not forbidden under the write lock.
    /// Returns previous lock options.
    /// </summary>
    /// <param name="areWritesDisabled">If set to <c>true</c>, qdrant doesn’t allow
    /// creating new collections or adding new data to the existing storage.
    /// </param>
    /// <param name="reasonMessage">The reason why the current lock options are set.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    /// This feature enables administrators to prevent a qdrant process from using more disk space
    /// while permitting users to search and delete unnecessary data.
    /// </remarks>
    public async Task<SetLockOptionsResponse> SetLockOptions(
        bool areWritesDisabled,
        string reasonMessage,
        CancellationToken cancellationToken)
    {
        var url = "/locks";

        var setLockOptionsRequest = new SetLockOptionsRequest(
            write: areWritesDisabled,
            errorMessage: reasonMessage);

        var response = await ExecuteRequest<SetLockOptionsRequest, SetLockOptionsResponse>(
            url,
            HttpMethod.Post,
            setLockOptionsRequest,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Get lock options. If write is locked, all write operations and collection creation are forbidden.
    /// However, deletion operations or updates are not forbidden under the write lock.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<SetLockOptionsResponse> GetLockOptions(CancellationToken cancellationToken)
    {
        var url = "/locks";

        var response = await ExecuteRequest<SetLockOptionsResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Retrieves a report of performance issues and configuration suggestions.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [Experimental("QD0001")]
    public async Task<ReportIssuesResponse> ReportIssues(CancellationToken cancellationToken)
    {
        var url = "/issues";

        var response = await ExecuteRequest<ReportIssuesResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        return response;
    }

    /// <summary>
    /// Removes all issues reported so far.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [Experimental("QD0002")]
    public async Task<ClearReportedIssuesResponse> ClearIssues(CancellationToken cancellationToken)
    {
        var url = "/issues";

        var response = await ExecuteRequest<ClearReportedIssuesResponse>(
            url,
            HttpMethod.Delete,
            cancellationToken,
            retryCount: 0);

        return response;
    }
}
