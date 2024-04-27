using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when the object can't be serialized to Qdrant JSON.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantJsonSerializationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantJsonSerializationException"/> class.
    /// </summary>
    /// <param name="reason">The reason for serialization failure.</param>
    public QdrantJsonSerializationException(string reason) : base($"Unable to searialize JSON. {reason}")
    { }
}
