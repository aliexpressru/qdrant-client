using Aer.QdrantClient.Http.Diagnostics.Helpers;
using Aer.QdrantClient.Http.Diagnostics.Tracing;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Responses;
using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <inheritdoc/>
    public async Task<GetInstanceDetailsResponse> GetInstanceDetails(CancellationToken cancellationToken, string clusterName = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(GetInstanceDetails),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(GetInstanceDetails), clusterName);

        var url = "/";

        HttpRequestMessage message = new(HttpMethod.Get, url);

        var response = await ExecuteRequest<GetInstanceDetailsResponse>(
            message,
            clusterName,
            cancellationToken);

        tracingScope.SetResult(true);

        diagnostic.SetSuccess();

        return response;
    }

    /// <inheritdoc/>
    public async Task<GetTelemetryResponse> GetTelemetry(
        CancellationToken cancellationToken,
        uint detailsLevel = 3,
        bool isAnonymizeTelemetryData = true,
        string clusterName = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(GetTelemetry),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(GetTelemetry), clusterName);

        var url = $"/telemetry?details_level={detailsLevel}&anonymize={ToUrlQueryString(isAnonymizeTelemetryData)}";

        var response = await ExecuteRequest<GetTelemetryResponse>(
            url,
            HttpMethod.Get,
            clusterName,
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
    public async Task<string> GetPrometheusMetrics(
        CancellationToken cancellationToken,
        bool isAnonymizeMetricsData = true,
        string clusterName = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(GetPrometheusMetrics),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(GetPrometheusMetrics), clusterName);

        var url = $"/metrics?anonymize={ToUrlQueryString(isAnonymizeMetricsData)}";

        HttpRequestMessage message = new(HttpMethod.Get, url);

        var response = await ExecuteRequest<string>(message, clusterName, cancellationToken);

        tracingScope.SetResult(true);

        diagnostic.SetSuccess();

        return response;
    }

    /// <inheritdoc/>
    [Obsolete("Lock API is deprecated and going to be removed in v1.16")]
    public async Task<SetLockOptionsResponse> SetLockOptions(
        bool areWritesDisabled,
        string reasonMessage,
        CancellationToken cancellationToken,
        string clusterName = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(SetLockOptions),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(SetLockOptions), clusterName);

        var qdrantVersion = (await GetInstanceDetails(cancellationToken)).ParsedVersion;

        if (qdrantVersion.Minor >= 16)
        {
            var ex = new NotSupportedException("Qdrant Lock API is deprecated and removed in v1.16");

            tracingScope.SetError(ex);

            throw ex;
        }

        var url = "/locks";

        var setLockOptionsRequest = new SetLockOptionsRequest(
            write: areWritesDisabled,
            errorMessage: reasonMessage);

        var response = await ExecuteRequest<SetLockOptionsRequest, SetLockOptionsResponse>(
            url,
            HttpMethod.Post,
            setLockOptionsRequest,
            clusterName,
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
    [Obsolete("Lock API is deprecated and going to be removed in v1.16")]
    public async Task<SetLockOptionsResponse> GetLockOptions(CancellationToken cancellationToken, string clusterName = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(GetLockOptions),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(GetLockOptions), clusterName);

        var url = "/locks";

        var response = await ExecuteRequest<SetLockOptionsResponse>(
            url,
            HttpMethod.Get,
            clusterName,
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
    [Experimental("QD0001")]
    public async Task<ReportIssuesResponse> ReportIssues(CancellationToken cancellationToken, string clusterName = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(ReportIssues),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(ReportIssues), clusterName);

        var url = "/issues";

        var response = await ExecuteRequest<ReportIssuesResponse>(
            url,
            HttpMethod.Get,
            clusterName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

        return response;
    }

    /// <inheritdoc/>
    [Experimental("QD0002")]
    public async Task<ClearReportedIssuesResponse> ClearIssues(CancellationToken cancellationToken, string clusterName = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(ClearIssues),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(ClearIssues), clusterName);

        var url = "/issues";

        var response = await ExecuteRequest<ClearReportedIssuesResponse>(
            url,
            HttpMethod.Delete,
            clusterName,
            cancellationToken,
            retryCount: 0);

        tracingScope.SetResult(response);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }
}
