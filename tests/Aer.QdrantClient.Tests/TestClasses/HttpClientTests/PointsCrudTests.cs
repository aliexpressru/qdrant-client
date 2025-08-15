using Aer.QdrantClient.Http;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal partial class PointsCrudTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();

        _qdrantHttpClient = ServiceProvider.GetRequiredService<QdrantHttpClient>();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage();
    }
}
