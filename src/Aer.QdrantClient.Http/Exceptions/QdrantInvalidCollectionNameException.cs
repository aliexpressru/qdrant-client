namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Occurs when the attempt to access or create a collection with invalid name is made.
/// </summary>
public class QdrantInvalidCollectionNameException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="QdrantInvalidCollectionNameException"/>.
    /// </summary>
    /// <param name="invalidCollectionName">The collection name that is deemed invalid.</param>
    /// <param name="reason">The reason why the collection name is deemed invalid.</param>
    public QdrantInvalidCollectionNameException(string invalidCollectionName, string reason)
        : base($"The collection name {invalidCollectionName} is invalid. Reason : {reason}")
    { }
}
