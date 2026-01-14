using Aer.QdrantClient.Http;

namespace Aer.QdrantClient.Tests.Infrastructure;

internal class CustomQdrantHttpClient : QdrantHttpClient
{
    private readonly HttpClient _testClient;

#if NETSTANDARD2_0 || NETSTANDARD2_1
    public override Task<HttpClient> GetHttpClient(string collectionOrClusterName) => Task.FromResult(_testClient);
#else
    public override ValueTask<HttpClient> GetHttpClient(string collectionOrClusterName) => ValueTask.FromResult(_testClient);
#endif

    public CustomQdrantHttpClient()
    { }

    public CustomQdrantHttpClient(HttpClient testClient)
    {
        _testClient = testClient;
    }
}
