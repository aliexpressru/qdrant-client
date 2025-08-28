using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Occurs when Qdrant cluster is in invalid state.
/// </summary>
/// <param name="reason">The invalid state reason.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantInvalidClusterStateException(string reason) : Exception($"Qdrant cluster seems to be in invalid state: {reason}");
