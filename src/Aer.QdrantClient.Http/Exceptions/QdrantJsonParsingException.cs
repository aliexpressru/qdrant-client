using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when the qdrant response JSON can't be parsed.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantJsonParsingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantJsonValueParsingException"/> class.
    /// </summary>
    /// <param name="reason">The reason for deserialization failure.</param>
    public QdrantJsonParsingException(string reason) : base($"Unable to parse JSON. {reason}")
    { }
}
