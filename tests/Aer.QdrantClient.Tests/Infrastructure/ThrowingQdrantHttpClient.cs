using Aer.QdrantClient.Http;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Tests.Infrastructure;

internal class ThrowingQdrantHttpClient : QdrantHttpClient
{
	public ThrowingQdrantHttpClient(HttpClient apiClient, ILogger logger = null) : base(apiClient, logger)
	{ 
		ApiClient = new ThrowingHttpClient(apiClient);
	}

	public ThrowingQdrantHttpClient(
		string host,
		int port = 6334,
		bool useHttps = false,
		string apiKey = null,
		TimeSpan? httpClientTimeout = null,
		ILogger logger = null,
		bool disableTracing = false) : base(host, port, useHttps, apiKey, httpClientTimeout, logger, disableTracing)
	{
		ApiClient = new ThrowingHttpClient(ApiClient);
	}

	public ThrowingQdrantHttpClient(
		Uri httpAddress,
		string apiKey = null,
		TimeSpan? httpClientTimeout = null,
		ILogger logger = null,
		bool disableTracing = false) : base(httpAddress, apiKey, httpClientTimeout, logger, disableTracing)
	{
		ApiClient = new ThrowingHttpClient(ApiClient);
	}
	
	public void ThrowOnce()
	{
		((ThrowingHttpClient)ApiClient).ThrowOnce();
	}
	
	public void BadRequestOnce()
	{
		((ThrowingHttpClient)ApiClient).BadRequestOnce();
	}
}
