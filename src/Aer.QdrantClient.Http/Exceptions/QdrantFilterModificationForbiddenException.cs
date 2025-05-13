using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens if a filter condition is added when a raw filter string is already set.
/// </summary>
/// <param name="rawFilterString">The raw filter string assigned to Qdrant filter.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantFilterModificationForbiddenException(string rawFilterString)
	: Exception(
		$"Qdrant filter must be built either from a raw filter string or filter conditions. The raw filter string is already set to {rawFilterString}.");
