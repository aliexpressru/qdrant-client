using System.Net;

namespace Aer.QdrantClient.Http.Helpers;

#if NETSTANDARD2_1
internal static class HttpRequestExceptionExtensions
{
	private const string StatusCodeKeyName = "StatusCode";

	public static bool SetStatusCode(this HttpRequestException httpRequestException, HttpStatusCode httpStatusCode)
	{
		httpRequestException.Data[StatusCodeKeyName] = httpStatusCode;

		return false;
	}

	public static HttpStatusCode? GetStatusCode(this HttpRequestException httpRequestException)
	{
		return (HttpStatusCode?) httpRequestException.Data[StatusCodeKeyName];
	}
}
#endif
