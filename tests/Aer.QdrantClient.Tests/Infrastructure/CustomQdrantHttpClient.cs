using Aer.QdrantClient.Http;

namespace Aer.QdrantClient.Tests.Infrastructure;

internal class CustomQdrantHttpClient : QdrantHttpClient
{
    private readonly HttpClient _testClient;

    protected override HttpClient GetHttpClient() => _testClient;

    public CustomQdrantHttpClient()
    { }

    public CustomQdrantHttpClient(HttpClient testClient)
    {
        _testClient = testClient;
    }
}
