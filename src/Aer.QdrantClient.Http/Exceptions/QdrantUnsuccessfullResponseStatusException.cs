using Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable MemberCanBeInternal

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens the qdrant response status does not indicate success.
/// </summary>
public class QdrantUnsuccessfullResponseStatusException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantUnsuccessfullResponseStatusException"/> class.
    /// </summary>
    /// <param name="qdrantResponseType">The type of the qdrant response.</param>
    /// <param name="status">The status of the qdrant response.</param>
    public QdrantUnsuccessfullResponseStatusException(Type qdrantResponseType, QdrantStatus status)
        : base($"Qdrant response {qdrantResponseType} status {status} does not indicate success")
    { }
}
