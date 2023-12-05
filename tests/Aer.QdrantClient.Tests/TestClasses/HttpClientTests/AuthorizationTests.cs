using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class AuthorizationTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize(isDisableAuthorization: true);
    }

    [Test]
    public async Task TestUnauthorizedAction()
    {
        if (IsCiEnvironment)
        {
            // CI environment has non-secure Qdrant instance so this test is irrelevant
            return;
        }

        _qdrantHttpClient = ServiceProvider.GetRequiredService<QdrantHttpClient>();

        var unauthorizedAct = async ()=> await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                100,
                isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        await unauthorizedAct.Should().ThrowAsync<QdrantUnauthorizedAccessException>();
    }
}
