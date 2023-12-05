// ReSharper disable MemberCanBeInternal

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when invalid qdrant filter encounered.
/// </summary>
public class QdrantInvalidFilterException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantInvalidFilterException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public QdrantInvalidFilterException(string message) : base(message)
    { }
}
