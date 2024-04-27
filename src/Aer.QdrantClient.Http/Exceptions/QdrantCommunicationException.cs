using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens if communicating with Qdrant back-end failed.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantCommunicationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantCommunicationException"/> class.
    /// </summary>
    /// <param name="method">The Qdrant method.</param>
    /// <param name="url">The Qdrant api URL.</param>
    /// <param name="statusCode">The Qdrant api response status code.</param>
    /// <param name="reasonPhrase">The Qdrant fail reason phrase.</param>
    /// <param name="jsonContent">The Qdrant fail raw response content json.</param>
    public QdrantCommunicationException(
        string method,
        string url,
        HttpStatusCode statusCode,
        string reasonPhrase,
        string jsonContent) : base($"Qdrant backend {method} {url} response status code {statusCode} does not indicate success.\nReason: {reasonPhrase}.\nContent: {jsonContent}")
    { }
}
