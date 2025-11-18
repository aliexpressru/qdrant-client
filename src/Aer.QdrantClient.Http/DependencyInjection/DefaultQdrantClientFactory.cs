using Aer.QdrantClient.Http.Abstractions;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Http.DependencyInjection;

/// <summary>
/// Default implementation of <see cref="IQdrantClientFactory"/>.
/// </summary>
internal class DefaultQdrantClientFactory(IHttpClientFactory httpClientFactory) : IQdrantClientFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="IQdrantHttpClient"/> with the specified client name.
    /// </summary>
    /// <param name="clientName">The name of the client to get.</param>
    public IQdrantHttpClient CreateClient(string clientName)
    {
        var httpClient = httpClientFactory.CreateClient(clientName);

        if (httpClient.BaseAddress == null)
        {
            // Means that no HttpClient was registered with such name
            throw new QdrantNamedQdrantClientNotFound(clientName);
        }

        return new QdrantHttpClient(httpClient);
    }
}
