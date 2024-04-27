using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens if trying to construct SearchGroupId from invalid number. E.g. negative one.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantInvalidNumericSearchGroupIdException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantInvalidNumericSearchGroupIdException"/> class.
    /// </summary>
    /// <param name="invalidPointId">The invalid point identifier.</param>
    public QdrantInvalidNumericSearchGroupIdException(object invalidPointId)
        : base($"Invalid numeric search group id {invalidPointId}. Numeric search group id should be greater than zero.")
    { }
}
