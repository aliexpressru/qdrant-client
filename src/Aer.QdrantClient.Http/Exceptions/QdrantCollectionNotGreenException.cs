using Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable MemberCanBeInternal

namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Represents an exception which happens when waiting time for the collection to become green exceeded the specified timeout value.
/// </summary>
public class QdrantCollectionNotGreenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantCollectionNotGreenException"/> class.
    /// </summary>
    /// <param name="collectionName">The collection to wait to become green.</param>
    /// <param name="waitForCollectionGreenTimeout">The time to wait for collection to become green.</param>
    public QdrantCollectionNotGreenException(
        string collectionName,
        TimeSpan waitForCollectionGreenTimeout)
        : base($"The collection {collectionName} is not {QdrantCollectionStatus.Green} for the timeout duration {waitForCollectionGreenTimeout:g}")
    { }
}
