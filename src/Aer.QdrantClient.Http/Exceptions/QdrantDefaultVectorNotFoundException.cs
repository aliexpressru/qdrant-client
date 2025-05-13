using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Occurs when the attempt is made to get default vector that doesn't exist for a point.
/// </summary>
/// <param name="defaultVectorName">The name of the default vector that is not found for point.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantDefaultVectorNotFoundException(string defaultVectorName)
	: Exception($"Can't find default vector with name {defaultVectorName}");
