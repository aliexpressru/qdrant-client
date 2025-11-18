using Aer.QdrantClient.Http.Abstractions;

namespace Aer.QdrantClient.Http.DependencyInjection;

/// <summary>
/// Represents a factory for creating <see cref="QdrantClient"/> instances.
/// </summary>
public class QdrantClientFactory(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
{
    /// <summary>
    /// Creates a new instance of <see cref="IQdrantHttpClient"/> with the specified client name.
    /// </summary>
    /// <param name="clientName">The name of the client to get.</param>
    public IQdrantHttpClient CreateClient(string clientName)
    {
        var httpClient = httpClientFactory.CreateClient(clientName);
        
        return new QdrantHttpClient(httpClient);
    }
}
