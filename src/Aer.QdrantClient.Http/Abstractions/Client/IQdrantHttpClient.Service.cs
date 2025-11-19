using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http.Abstractions;

public partial interface IQdrantHttpClient
{
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
    [Obsolete("Lock API is deprecated and going to be removed in v1.16")]
    Task<SetLockOptionsResponse> SetLockOptions(
        bool areWritesDisabled,
        string reasonMessage,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get lock options. If write is locked, all write operations and collection creation are forbidden.
    /// However, deletion operations or updates are not forbidden under the write lock.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [Obsolete("Lock API is deprecated and going to be removed in v1.16")]
    Task<SetLockOptionsResponse> GetLockOptions(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a report of performance issues and configuration suggestions.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [Experimental("QD0001")]
    Task<ReportIssuesResponse> ReportIssues(CancellationToken cancellationToken);

    /// <summary>
    /// Removes all issues reported so far.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [Experimental("QD0002")]
    Task<ClearReportedIssuesResponse> ClearIssues(CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets the Qdrant instance details.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<GetInstanceDetailsResponse> GetInstanceDetails(CancellationToken cancellationToken);

    /// <summary>
    /// Get the Qdrant telemetry information.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="detailsLevel">Defines how detailed the telemetry data is.</param>
    /// <param name="isAnonymizeTelemetryData">If set tot <c>true</c>, anonymize the collected telemetry result.</param>
    Task<GetTelemetryResponse> GetTelemetry(
        CancellationToken cancellationToken,
        uint detailsLevel = 3,
        bool isAnonymizeTelemetryData = true);

    /// <summary>
    /// Collect metrics data including app info, collections info, cluster info and statistics in Prometheus format.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isAnonymizeMetricsData">If set tot <c>true</c>, anonymize the collected metrics result.</param>
    Task<string> GetPrometheusMetrics(
        CancellationToken cancellationToken,
        bool isAnonymizeMetricsData = true);
}
