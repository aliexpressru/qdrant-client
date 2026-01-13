using Aer.QdrantClient.Http;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Tests.Infrastructure;

internal class ThrowingQdrantHttpClient : QdrantHttpClient
{
    readonly HttpClient _throwingHttpClient;

    protected override HttpClient GetHttpClient() => _throwingHttpClient;

    public ThrowingQdrantHttpClient(HttpClient apiClient, ILogger logger = null) : base(apiClient, logger)
    {
        _throwingHttpClient = new ThrowingHttpClient(apiClient);
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
