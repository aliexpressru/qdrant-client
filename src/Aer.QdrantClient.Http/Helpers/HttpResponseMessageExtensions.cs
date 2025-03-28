using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;

namespace Aer.QdrantClient.Http.Helpers;

#if NETSTANDARD2_0
internal static class HttpResponseMessageExtensions
{
	public static HttpResponseMessage SetStatusCode(this HttpResponseMessage httpResponseMessage)
	{
		try
		{
			httpResponseMessage.EnsureSuccessStatusCode();
		}
		catch (HttpRequestException ex) when (ex.SetStatusCode(httpResponseMessage.StatusCode))
		{
			// Intentionally left empty. Will never be reached.
		}

		return httpResponseMessage;
	}
}
#endif
