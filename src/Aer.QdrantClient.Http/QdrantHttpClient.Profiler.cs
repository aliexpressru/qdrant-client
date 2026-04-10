using Aer.QdrantClient.Http.Diagnostics.Helpers;
using Aer.QdrantClient.Http.Models.Responses;
using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <inheritdoc/>
    [Experimental("QD0003")]
    public async Task<GetSlowRequestsResponse> GetSlowRequests(
        CancellationToken cancellationToken,
        string clusterName = null)
    {
        using var diagnostic = DiagnosticTimer.StartNew(null, nameof(GetSlowRequests), clusterName);

        var url = "/profiler/slow_requests";

        var response = await ExecuteRequest<GetSlowRequestsResponse>(
            url,
            HttpMethod.Get,
            clusterName,
            cancellationToken,
            retryCount: 0);

        if (response.Status.IsSuccess)
        {
            diagnostic.SetSuccess();
        }

        return response;
    }
}
