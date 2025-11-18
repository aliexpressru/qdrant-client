namespace Aer.QdrantClient.Http.Abstractions;

/// <summary>
/// Represents a factory for creating <see cref="IQdrantHttpClient"/> instances.
/// </summary>
public interface IQdrantClientFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="IQdrantHttpClient"/> with the specified client name.
    /// For client to be created, it must be previously registered in the dependency
    /// injection container with the same name it is going to be requested.
    /// </summary>
    /// <param name="clientName">The name of the client to get.</param>
    IQdrantHttpClient CreateClient(string clientName);
}
