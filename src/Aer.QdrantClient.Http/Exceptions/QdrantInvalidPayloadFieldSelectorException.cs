using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens if trying to pass invalid payload property selector to a filter builder method.
/// </summary>
/// <param name="payloadPropertySelectorExpression">The payload property selector expression.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantInvalidPayloadFieldSelectorException(string payloadPropertySelectorExpression)
	: Exception($"Payload property selector expression {payloadPropertySelectorExpression} is invalid");
