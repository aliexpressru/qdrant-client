using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when the qdrant response specific JSON value can't be parsed.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantJsonValueParsingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantJsonValueParsingException"/> class.
    /// </summary>
    /// <param name="value">The value that was failed to parse.</param>
    public QdrantJsonValueParsingException(string value) : base($"Unable to parse JSON value {value}")
    { }
}
