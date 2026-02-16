using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http.Abstractions;

public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Gets the slow requests profiler data.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    /// <remarks>This is an experimental undocumented API - use at your own risk.</remarks>
    Task<GetSlowRequestsResponse> GetSlowRequests(CancellationToken cancellationToken, string clusterName = null);
}
