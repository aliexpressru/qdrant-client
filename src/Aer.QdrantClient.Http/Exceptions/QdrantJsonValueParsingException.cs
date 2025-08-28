using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when the qdrant response specific JSON value can't be parsed.
/// </summary>
/// <param name="value">The value that was failed to parse.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantJsonValueParsingException(string value) : Exception($"Unable to parse JSON value {value}");
