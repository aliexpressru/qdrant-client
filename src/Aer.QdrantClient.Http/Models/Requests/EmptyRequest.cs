namespace Aer.QdrantClient.Http.Models.Requests;

internal class EmptyRequest
{
	public string RequestMessageBody { get; } = "{}";
	
	public static EmptyRequest Instance { get; } = new ();
}
