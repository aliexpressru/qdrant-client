using Aer.QdrantClient.Http;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Tests.Infrastructure;

internal class ThrowingQdrantHttpClient : QdrantHttpClient
{
    readonly HttpClient _throwingHttpClient;

#if NETSTANDARD2_0 || NETSTANDARD2_1
    public override Task<HttpClient> GetHttpClient(string _) => Task.FromResult(_throwingHttpClient);
#else
    public override ValueTask<HttpClient> GetApiClient(string _) => ValueTask.FromResult(_throwingHttpClient);
#endif

    public ThrowingQdrantHttpClient(HttpClient apiClient, ILogger logger = null) : base(apiClient, logger)
    {
        _throwingHttpClient = new ThrowingHttpClient(apiClient);
    }

#pragma warning disable CA2012 // Justification: Need to support synchronous calls in tests, can't await ValueTasks directly.
    public void ThrowOnce()
    {
        HttpClient httpClient;

        var getHttpClientTask = GetApiClient(null);
        if (getHttpClientTask.IsCompleted)
        {
            httpClient = getHttpClientTask.Result;
        }
        else
        {
            httpClient = getHttpClientTask.GetAwaiter().GetResult();
        }

        ((ThrowingHttpClient)httpClient).ThrowOnce();
    }

    public void BadRequestOnce()
    {
        HttpClient httpClient;

        var getHttpClientTask = GetApiClient(null);
        if (getHttpClientTask.IsCompleted)
        {
            httpClient = getHttpClientTask.Result;
        }
        else
        {
            httpClient = getHttpClientTask.GetAwaiter().GetResult();
        }

        ((ThrowingHttpClient)httpClient).BadRequestOnce();
    }
#pragma warning restore CA2012
}
