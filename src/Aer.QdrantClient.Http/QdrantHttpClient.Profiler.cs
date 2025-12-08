using Aer.QdrantClient.Http.Models.Responses;
using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http;

[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public partial class QdrantHttpClient
{
    /// <inheritdoc/>
    [Experimental("QD0003")]
    public async Task<GetSlowRequestsResponse> GetSlowRequests(CancellationToken cancellationToken)
    {
        var url = "/profiler/slow_requests";

        var response = await ExecuteRequest<GetSlowRequestsResponse>(
            url,
            HttpMethod.Get,
            cancellationToken,
            retryCount: 0);

        return response;
    }
}
