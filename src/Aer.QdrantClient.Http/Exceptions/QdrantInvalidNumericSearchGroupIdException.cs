using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens if trying to construct SearchGroupId from invalid number. E.g. negative one.
/// </summary>
/// <param name="invalidGroupId">The invalid group identifier.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantInvalidNumericSearchGroupIdException(object invalidGroupId)
	: Exception(
		$"Invalid numeric search group id {invalidGroupId}. Numeric search group id should be greater than zero.");
