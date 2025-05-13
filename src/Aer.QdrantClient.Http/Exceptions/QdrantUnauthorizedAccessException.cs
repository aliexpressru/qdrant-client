using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when trying to access secure qdrant engine API without authorization.
/// </summary>
/// <param name="unauthorizedReason">The Qdrant engine unauthorized access reason message.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantUnauthorizedAccessException(string unauthorizedReason)
	: Exception(
		$"Qdrant secure backend detected unauthorized access. Reason: '{unauthorizedReason}'. Check configuration.");
