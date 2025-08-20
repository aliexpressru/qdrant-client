namespace Aer.QdrantClient.Http.Models.Requests;

/// <summary>
/// Represents the request to set qdrant lock options.
/// </summary>
internal sealed class SetLockOptionsRequest
{
    /// <summary>
    /// The write operations lock reason.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// If set to <c>true</c> write operations are locked, otherwise - write operations are enabled.
    /// </summary>
    public bool Write { get; }

    public SetLockOptionsRequest(bool write, string errorMessage)
    {
        Write = write;
        ErrorMessage = errorMessage;
    }
}
