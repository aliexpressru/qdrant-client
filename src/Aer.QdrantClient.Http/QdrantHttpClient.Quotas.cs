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
    public async Task<SetQuotasResponse> SetQuotas(
        bool enabled, 
        uint? maxResidentMemoryPercent, 
        uint? maxDiskUsagePercent, 
        uint? releaseMarginPercent,
        CancellationToken cancellationToken,
        string clusterName = null,
        bool isWaitForResult = true)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(SetQuotas),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(SetQuotas), clusterName);

        var setQuotasRequest = new SetQuotasRequest
        {
            Enabled = enabled,
            MaxResidentMemoryPercent = maxResidentMemoryPercent,
            MaxDiskUsagePercent = maxDiskUsagePercent,
            ReleaseMarginPercent = releaseMarginPercent,
            IsWaitForResult = isWaitForResult,
        };
        
        var url = $"/quotas?wait={ToUrlQueryString(isWaitForResult)}";

        var response = await ExecuteRequest<SetQuotasRequest, SetQuotasResponse>(
            url,
            HttpMethod.Put,
            setQuotasRequest,
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
    public async Task<GetQuotasResponse> GetQuotas(CancellationToken cancellationToken, string clusterName = null)
    {
        using var tracingScope = QdrantHttpClientTracing.CreateRequestScope(
            _tracer,
            nameof(GetQuotas),
            _enableTracing,
            Logger);

        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(GetQuotas), clusterName);
        
        var url = $"/quotas";

        var response = await ExecuteRequest<GetQuotasResponse>(
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
}
