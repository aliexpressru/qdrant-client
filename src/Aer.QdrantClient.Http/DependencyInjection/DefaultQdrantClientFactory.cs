using Aer.QdrantClient.Http.Abstractions;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Http.DependencyInjection;

/// <summary>
/// Default implementation of <see cref="IQdrantClientFactory"/>.
/// </summary>
internal class DefaultQdrantClientFactory(IHttpClientFactory httpClientFactory) : IQdrantClientFactory
{
    readonly HashSet<string> _unregisteredClientNames = new();

    /// <summary>
    /// Creates a new instance of <see cref="IQdrantHttpClient"/> with the specified client name.
    /// </summary>
    /// <param name="clientName">The name of the client to get.</param>
    public IQdrantHttpClient CreateClient(string clientName)
    {
        // If we have already determined that this client name is not registered, throw immediately
        if (_unregisteredClientNames.Contains(clientName))
        {
            throw new QdrantNamedQdrantClientNotFound(clientName);
        }

        var httpClient = httpClientFactory.CreateClient(clientName);

        if (httpClient.BaseAddress == null)
        {
            // Means that no HttpClient was registered with such name
            _unregisteredClientNames.Add(clientName);
            throw new QdrantNamedQdrantClientNotFound(clientName);
        }

        return new QdrantHttpClient(httpClient);
    }
}
