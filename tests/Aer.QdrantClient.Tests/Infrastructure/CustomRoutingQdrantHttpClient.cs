using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Tests.Infrastructure;

/// <summary>
/// The custom implementation of the <see cref="QdrantHttpClient"/> for tests.
/// This implementation exists to test routing by using different HttpClient instances for different collections/clusters.
/// </summary>
internal class CustomRoutingQdrantHttpClient : QdrantHttpClient
{
    private readonly Dictionary<string, HttpClient> _clientsByClusters;

#if NETSTANDARD2_0 || NETSTANDARD2_1
    public override Task<HttpClient> GetHttpClient(string collectionOrClusterName)
    {
        var configuredClient = _clientsByClusters.GetValueOrDefault(collectionOrClusterName);

        if (configuredClient is null)
        {
            throw new QdrantClientUninitializedException();
        }

        return Task.FromResult(configuredClient);
    }
#else
    public override ValueTask<HttpClient> GetApiClient(string collectionOrClusterName)
    {
        if (collectionOrClusterName is null)
        {
            throw new QdrantClientUninitializedException();
        }

        var configuredClient = _clientsByClusters.GetValueOrDefault(collectionOrClusterName);

        if (configuredClient is null || configuredClient.BaseAddress is null)
        {
            throw new QdrantClientUninitializedException();
        }

        return ValueTask.FromResult(configuredClient);
    }
#endif

    public CustomRoutingQdrantHttpClient()
    { }

    public CustomRoutingQdrantHttpClient(Dictionary<string, HttpClient> clientsByCollections)
    {
        _clientsByClusters = clientsByCollections;
    }
}
