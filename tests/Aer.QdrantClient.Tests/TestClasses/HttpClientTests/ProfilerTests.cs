using Aer.QdrantClient.Http;
using Aer.QdrantClient.Tests.Base;
using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

#if !DEBUG
[Ignore("Testing non-documented API only locally")]
#endif
internal class ProfilerTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;

    [OneTimeSetUp]
    public void Setup()
    {
        OnlyIfVersionAfterOrEqual("1.16.0", "Profiler API is only supported from v1.16");

        Initialize();

        _qdrantHttpClient = ServiceProvider.GetRequiredService<QdrantHttpClient>();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage();
    }

    [Test]
    [Experimental("Undocumented_profiler_slow_requests")] // Testing undocumented profiler/slow_requests API.
    public async Task GetSlowRequests()
    {
        var vectorCount = 100000;

        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName,
            vectorCount: vectorCount);

        var getSlowRequestsResult =
            await _qdrantHttpClient.GetSlowRequests(CancellationToken.None);

        getSlowRequestsResult.Status.IsSuccess.Should().BeTrue();
        getSlowRequestsResult.Result.Requests.Should().NotBeNull();

        getSlowRequestsResult.Result.Requests.Length.Should().BeGreaterThanOrEqualTo(1);

        var slowRequest = getSlowRequestsResult.Result.Requests.First();

        slowRequest.CollectionName.Should().Be(TestCollectionName);
        slowRequest.Duration.Should().BeGreaterThan(0);
        slowRequest.Datetime.Should().BeBefore(DateTime.UtcNow);
        slowRequest.RequestName.Should().NotBeNullOrEmpty();
        slowRequest.ApproxCount.Should().BeGreaterThan(0);
        slowRequest.RequestBody.Should().NotBeNull();
    }
}
