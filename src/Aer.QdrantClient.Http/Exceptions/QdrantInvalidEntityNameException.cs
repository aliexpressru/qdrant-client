namespace Aer.QdrantClient.Http.Exceptions;

/// <summary>
/// Occurs when the attempt to access or create a collection with invalid name is made.
/// </summary>
public class QdrantInvalidEntityNameException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="QdrantInvalidEntityNameException"/>.
    /// </summary>
    /// <param name="invalidCollectionName">The entity name that is deemed invalid.</param>
    /// <param name="reason">The reason why the entity name is deemed invalid.</param>
    public QdrantInvalidEntityNameException(string invalidCollectionName, string reason)
        : base($"The qdrant entity name {invalidCollectionName} is invalid. Reason : {reason}")
    { }
}
