using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when trying to read search group identifier value as an incompatible type.
/// </summary>
/// <param name="searchGroupIdConcreteTypeName">Name of the search group identifier concrete type.</param>
/// <param name="targetTypeName">Name of the target type to read search group id as.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantSearchGroupIdConversionException(string searchGroupIdConcreteTypeName, string targetTypeName)
	: Exception($"Can't read search group id from type {searchGroupIdConcreteTypeName} as {targetTypeName}");
