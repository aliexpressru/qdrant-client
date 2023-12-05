// ReSharper disable MemberCanBeInternal

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens if trying to pass invalid payload proerty selctor to a filter builder method.
/// </summary>
public class QdrantInvalidPayloadFieldSelectorException : Exception
{

    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantInvalidPayloadFieldSelectorException"/> class.
    /// </summary>
    /// <param name="payloadPropertySelectorExpression">The payload property selector expression.</param>
    public QdrantInvalidPayloadFieldSelectorException(string payloadPropertySelectorExpression) : base($"Payload property selector expression {payloadPropertySelectorExpression} is invalid")
    { }
}
