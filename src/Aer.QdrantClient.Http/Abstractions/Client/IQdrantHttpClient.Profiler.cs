using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http.Abstractions;

public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Gets the slow requests profiler data.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>This is an experimental undocumented API.</remarks>
    Task<GetSlowRequestsResponse> GetSlowRequests(CancellationToken cancellationToken);
}
