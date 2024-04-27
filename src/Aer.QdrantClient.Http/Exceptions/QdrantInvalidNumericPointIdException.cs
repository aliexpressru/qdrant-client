using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens if trying to construct PointId from invalid number. E.g. negative one.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantInvalidNumericPointIdException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantInvalidNumericPointIdException"/> class.
    /// </summary>
    /// <param name="invalidPointId">The invalid point identifier.</param>
    public QdrantInvalidNumericPointIdException(object invalidPointId)
        : base($"Invalid numeric point id {invalidPointId}. Numeric point id should be greater than zero.")
    { }
}
