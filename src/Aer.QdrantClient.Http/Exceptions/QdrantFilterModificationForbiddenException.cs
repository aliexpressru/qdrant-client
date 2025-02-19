using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens if a filter condition is added when a raw filter string is already set.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantFilterModificationForbiddenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantFilterModificationForbiddenException"/> class.
    /// <param name="rawFilterString">The raw filter string assigned to Qdrant filter.</param>
    /// </summary>
    public QdrantFilterModificationForbiddenException(string rawFilterString)
    : base($"Qdrant filter must be built either from a raw filter string or filter conditions. The raw filter string is already set to {rawFilterString}.")
    { }
}
