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
        uint detailsLevel,
        bool isAnonymizeTelemetryData);

    /// <summary>
    /// Collect metrics data including app info, collections info, cluster info and statistics in Prometheus format.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isAnonymizeMetricsData">If set tot <c>true</c>, anonymize the collected metrics result.</param>
    Task<string> GetPrometheusMetrics(
        CancellationToken cancellationToken,
        bool isAnonymizeMetricsData);

}
