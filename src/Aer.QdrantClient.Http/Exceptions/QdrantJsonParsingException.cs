using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when the qdrant response JSON can't be parsed.
/// </summary>
/// <param name="reason">The reason for parsing failure.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantJsonParsingException(string reason) : Exception($"Unable to parse JSON. {reason}");
