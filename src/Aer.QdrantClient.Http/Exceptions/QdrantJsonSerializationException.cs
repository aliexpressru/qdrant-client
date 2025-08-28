using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when the object can't be serialized to qdrant JSON.
/// </summary>
/// <param name="reason">The reason for serialization failure.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantJsonSerializationException(string reason) : Exception($"Unable to serialize JSON. {reason}");
