// ReSharper disable MemberCanBeInternal

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when the Qdrant payload is found to be of invalid type.
/// </summary>
public class QdrantInvalidPayloadTypeException : Exception
{

    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantInvalidPayloadTypeException"/> class.
    /// </summary>
    /// <param name="payloadTypeName">Name of the payload type.</param>
    public QdrantInvalidPayloadTypeException(string payloadTypeName)
        : base($"Payload of type {payloadTypeName} is not supported. Use complex type or a Dictionary<string, object>.")
    { }
}
