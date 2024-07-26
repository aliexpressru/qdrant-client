using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class ServiceMethodsTests : QdrantTestsBase
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

    [TestCase(true)]
    [TestCase(false)]
    public async Task TestGetTelemetryData(bool isAnonymized)
    {
        var telemetry = await _qdrantHttpClient.GetTelemetry(
            CancellationToken.None,
            detailsLevel: 3,
            isAnonymizeTelemetryData: isAnonymized);

        telemetry.Status.IsSuccess.Should().BeTrue();
        telemetry.Result.Should().NotBeNull();
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task TestPrometheusMetrics(bool isAnonymized)
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var prometheusMetrics = await _qdrantHttpClient.GetPrometheusMetrics(
            CancellationToken.None,
            isAnonymizeMetricsData: isAnonymized);

        prometheusMetrics.Should().NotBeNull();

        prometheusMetrics.Should().Contain("collections_total 1");
    }

    [Test]
    public async Task TestCollectionReady_CollectionDoesNotExist()
    {
        var act = () => _qdrantHttpClient.EnsureCollectionReady(
                TestCollectionName,
                CancellationToken.None);

        await act.Should().ThrowAsync<QdrantUnsuccessfulResponseStatusException>()
            .Where(e => e.Message.Contains("not found", StringComparison.InvariantCultureIgnoreCase));
    }

    [Test]
    public async Task TestCollectionReady_InvalidTimeout()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var act = () => _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            pollingInterval: TimeSpan.FromMinutes(1),
            timeout: TimeSpan.FromSeconds(1));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(e => e.Message.Contains("should be greater than"));
    }

    [Test]
    public async Task TestCollectionReady_OneSuccessfulResponse()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var act = () => _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            pollingInterval: TimeSpan.FromMilliseconds(100),
            timeout: TimeSpan.FromSeconds(30),
            requiredNumberOfGreenCollectionResponses: 1); // default value

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task TestCollectionReady_SeveralSuccessfulResponses()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var act = () => _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            pollingInterval: TimeSpan.FromMilliseconds(100),
            timeout: TimeSpan.FromSeconds(30),
            requiredNumberOfGreenCollectionResponses: 3);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task TestStorageLock()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var lockReason = "Writes disabled";

        var setLockOptionsResult =
            await _qdrantHttpClient.SetLockOptions(areWritesDisabled: true, lockReason, CancellationToken.None);

        setLockOptionsResult.Status.IsSuccess.Should().BeTrue();
        setLockOptionsResult.Result.Write.Should().BeFalse();

        var upsertPointsAct = async ()=> await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points =
                [
                    new UpsertPointsRequest<TestPayload>.UpsertPoint(67, CreateTestVector(10), 67)
                ]
            },
            CancellationToken.None);

        // the fact that locked collection throws unauthorized status code is freaking me out!

        await upsertPointsAct.Should().ThrowAsync<QdrantUnauthorizedAccessException>()
                .Where(e => e.Message.Contains(lockReason));

        var setNewLockOptionsResult =
            await _qdrantHttpClient.SetLockOptions(areWritesDisabled: false, lockReason, CancellationToken.None);

        setNewLockOptionsResult.Status.IsSuccess.Should().BeTrue();

        // returns previous lock options
        setLockOptionsResult.Result.Write.Should().BeFalse();

        await upsertPointsAct.Should().NotThrowAsync();
    }

    [Test]
    [Experimental("QD0001")]
    public async Task TestReportIssues()
    {
        var issuesReportResult =
            await _qdrantHttpClient.ReportIssues(CancellationToken.None);

        issuesReportResult.Status.IsSuccess.Should().BeTrue();
        issuesReportResult.Result.Issues.Should().BeEmpty();

        var issuesClearResult =
            await _qdrantHttpClient.ClearIssues(CancellationToken.None);

        issuesClearResult.Status.IsSuccess.Should().BeTrue();
        issuesClearResult.Result.Should().BeTrue();
    }
}
