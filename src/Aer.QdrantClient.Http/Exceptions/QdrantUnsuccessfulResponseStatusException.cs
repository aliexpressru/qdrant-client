using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens the qdrant response status does not indicate success.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantUnsuccessfulResponseStatusException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantUnsuccessfulResponseStatusException"/> class.
    /// </summary>
    /// <param name="qdrantResponseType">The type of the qdrant response.</param>
    /// <param name="status">The status of the qdrant response.</param>
    public QdrantUnsuccessfulResponseStatusException(Type qdrantResponseType, QdrantStatus status)
        : base($"Qdrant response {qdrantResponseType} status {status} does not indicate success")
    { }
}
