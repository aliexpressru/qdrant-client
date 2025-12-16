using Aer.QdrantClient.Http;

namespace Aer.QdrantClient.Tests.Infrastructure;

internal class CustomQdrantHttpClient : QdrantHttpClient
{
    public CustomQdrantHttpClient()
    {

    }

    public CustomQdrantHttpClient(HttpClient testClient)
    {
        ApiClient = testClient;
    }
}
