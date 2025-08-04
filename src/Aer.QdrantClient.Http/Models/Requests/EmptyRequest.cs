namespace Aer.QdrantClient.Http.Models.Requests;

/// <summary>
/// Represents an empty request.
/// This is used when no specific request body is needed.
/// </summary>
internal sealed class EmptyRequest
{
	public string RequestMessageBody { get; } = "{}";
	
	public static EmptyRequest Instance { get; } = new ();
}
