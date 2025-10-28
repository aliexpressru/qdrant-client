using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <inheritdoc/>
    public async Task<GetInstanceDetailsResponse> GetInstanceDetails(CancellationToken cancellationToken)
    {
        var url = "/";

        HttpRequestMessage message = new(HttpMethod.Get, url);

        var response = await ExecuteRequest<GetInstanceDetailsResponse>(
            message,
            cancellationToken);

        return response;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<string> GetPrometheusMetrics(
        CancellationToken cancellationToken,
        bool isAnonymizeMetricsData = true)
    {
        var url = $"/metrics?anonymize={ToUrlQueryString(isAnonymizeMetricsData)}";

        HttpRequestMessage message = new(HttpMethod.Get, url);

        var result = await ExecuteRequest<string>(message, cancellationToken);

        return result;
    }

    /// <inheritdoc/>
    [Obsolete("Lock API is deprecated and going to be removed in v1.16")]
    public async Task<SetLockOptionsResponse> SetLockOptions(
        bool areWritesDisabled,
        string reasonMessage,
        CancellationToken cancellationToken)
    {
        var qdrantVersion = (await GetInstanceDetails(cancellationToken)).ParsedVersion;

        if (qdrantVersion.Minor >= 16)
        {
            throw new NotSupportedException("Qdrant Lock API is deprecated and removed in v1.16");
        }

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

    /// <inheritdoc/>
    [Obsolete("Lock API is deprecated and going to be removed in v1.16")]
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
