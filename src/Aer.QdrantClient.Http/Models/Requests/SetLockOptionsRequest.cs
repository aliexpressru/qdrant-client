namespace Aer.QdrantClient.Http.Models.Requests;

/// <summary>
/// Represents the request to set qdrant lock options.
/// </summary>
internal sealed class SetLockOptionsRequest(bool write, string errorMessage)
{
    /// <summary>
    /// The write operations lock reason.
    /// </summary>
    public string ErrorMessage { get; } = errorMessage;

    /// <summary>
    /// If set to <c>true</c> write operations are locked, otherwise - write operations are enabled.
    /// </summary>
    public bool Write { get; } = write;
}
