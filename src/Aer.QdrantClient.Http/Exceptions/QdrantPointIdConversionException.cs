// ReSharper disable MemberCanBeInternal

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when trying to read point identifier value as an incompatible type.
/// </summary>
public class QdrantPointIdConversionException : Exception
{

    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantPointIdConversionException"/> class.
    /// </summary>
    /// <param name="pointIdConcreteTypeName">Name of the point identifier concrete type.</param>
    /// <param name="targetTypeName">Name of the target type to read point id as.</param>
    public QdrantPointIdConversionException(string pointIdConcreteTypeName, string targetTypeName) : base($"Can't read point id from type {pointIdConcreteTypeName} as {targetTypeName}")
    { }
}
