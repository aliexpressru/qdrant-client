using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when trying to read point identifier value as an incompatible type.
/// </summary>
/// <param name="pointIdConcreteTypeName">Name of the point identifier concrete type.</param>
/// <param name="targetTypeName">Name of the target type to read point id as.</param>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantPointIdConversionException(string pointIdConcreteTypeName, string targetTypeName)
	: Exception($"Can't read point id from type {pointIdConcreteTypeName} as {targetTypeName}");
