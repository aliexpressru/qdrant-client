using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when trying to read search group identifier value as an incompatible type.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class QdrantSearchGroupIdConversionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantSearchGroupIdConversionException"/> class.
    /// </summary>
    /// <param name="searchGroupIdConcreteTypeName">Name of the search group identifier concrete type.</param>
    /// <param name="targetTypeName">Name of the target type to read search group id as.</param>
    public QdrantSearchGroupIdConversionException(string searchGroupIdConcreteTypeName, string targetTypeName)
        : base($"Can't read search group id from type {searchGroupIdConcreteTypeName} as {targetTypeName}")
    { }
}
