using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens if trying to construct PointId from invalid number. E.g. negative one.
/// </summary>
/// <param name="invalidPointId">The invalid point identifier.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantInvalidNumericPointIdException(object invalidPointId)
	: Exception($"Invalid numeric point id {invalidPointId}. Numeric point id should be greater than zero.");
