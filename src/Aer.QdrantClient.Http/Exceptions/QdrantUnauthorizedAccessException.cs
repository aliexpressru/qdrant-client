// ReSharper disable MemberCanBeInternal

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when trying to access secure Qdrant engine API without authorization.
/// </summary>
public class QdrantUnauthorizedAccessException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantUnauthorizedAccessException"/> class.
    /// </summary>
    /// <param name="unauthorizedReason">The Qdrant engine unauthorized access reason message.</param>
    public QdrantUnauthorizedAccessException(string unauthorizedReason) : base(
        $"Qdrant secure backend detected unauthorized access. Reason: '{unauthorizedReason}'. Check configuration.")
    { }
}
