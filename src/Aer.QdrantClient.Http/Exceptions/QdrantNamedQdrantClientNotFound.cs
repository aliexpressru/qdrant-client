namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Occurs when a Qdrant HTTP client with the specified name is not found in the dependency injection container.
/// </summary>
/// <param name="clientName">The name of the client that was not found.</param>
public class QdrantNamedQdrantClientNotFound(string clientName)
    : Exception($"No Qdrant HTTP client registered with the name '{clientName}'");
