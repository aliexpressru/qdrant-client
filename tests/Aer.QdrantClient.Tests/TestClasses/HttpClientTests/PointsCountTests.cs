using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class PointsCountTests : QdrantTestsBase
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

    [Test]
    public async Task CountPoints_WithoutFilter()
    {
        var vectorCount = 10;

        await PrepareCollection<TestPayload>(
            _qdrantHttpClient,
            TestCollectionName,
            vectorCount: vectorCount);

        var countPointsResult =
            await _qdrantHttpClient.CountPoints(
                TestCollectionName,
                new CountPointsRequest(
                    isCountExactPointsNumber: true
                ),
                CancellationToken.None);

        countPointsResult.Status.IsSuccess.Should().BeTrue();

        countPointsResult.Result.Count.Should().Be((ulong) vectorCount);
    }

    [Test]
    public async Task CountPoints_WithFilter()
    {
        var vectorCount = 10;

        await PrepareCollection<TestPayload>(
            _qdrantHttpClient,
            TestCollectionName,
            vectorCount: vectorCount);

        var countPointsResult =
            await _qdrantHttpClient.CountPoints(
                TestCollectionName,
                new CountPointsRequest(
                    isCountExactPointsNumber: true,
                    filter:
                    Q.Must(
                        Q<TestPayload>.MatchAnyFast(
                            p => p.Integer,
                            0,
                            1,
                            2)
                    )
                ),
                CancellationToken.None);

        countPointsResult.Status.IsSuccess.Should().BeTrue();

        countPointsResult.Result.Count.Should().Be(3);
    }

    [Test]
    public async Task FacetCountPoints_WithoutFilter()
    {
        var vectorCount = 10;

        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName,
            payloadInitializerFunction: i => new TestPayload()
            {
                Integer = i
            },
            vectorCount: vectorCount);

        // faceting enabled only for indexed fields

        (await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                Q<TestPayload>.GetPayloadFieldName(p => p.Integer),
                PayloadIndexedFieldType.Integer,
                CancellationToken.None,
                isWaitForResult: true)
            ).EnsureSuccess();

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var countPointsResult =
            await _qdrantHttpClient.FacetCountPoints(
                TestCollectionName,
                new FacetCountPointsRequest(
                    Q<TestPayload>.GetPayloadFieldName(p => p.Integer),
                    limit: 10,
                    exact: true
                ),
                CancellationToken.None);

        countPointsResult.Status.IsSuccess.Should().BeTrue();
        countPointsResult.Result.Hits.Length.Should().Be(vectorCount);

        foreach (var fieldFacet in countPointsResult.Result.Hits)
        {
            fieldFacet.ValueAs<int>().Should().BeOneOf(Enumerable.Range(0, 10));
            fieldFacet.Count.Should().Be(1);
        }
    }

    [Test]
    public async Task FacetCountPoints_WithFilter()
    {
        var vectorCount = 10;

        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName,
            payloadInitializerFunction: i => new TestPayload()
            {
                Integer = i
            },
            vectorCount: vectorCount);

        // faceting enabled only for indexed fields

        (await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                Q<TestPayload>.GetPayloadFieldName(p => p.Integer),
                PayloadIndexedFieldType.Integer,
                CancellationToken.None,
                isWaitForResult: true)
            ).EnsureSuccess();

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var countPointsResult =
            await _qdrantHttpClient.FacetCountPoints(
                TestCollectionName,
                new FacetCountPointsRequest(
                    Q<TestPayload>.GetPayloadFieldName(p => p.Integer),
                    filter: Q<TestPayload>.BeInRange(p=>p.Integer, greaterThanOrEqual: 4),
                    limit: 10,
                    exact: true
                ),
                CancellationToken.None);

        countPointsResult.Status.IsSuccess.Should().BeTrue();
        countPointsResult.Result.Hits.Length.Should().Be(vectorCount-4); // 0, 1, 2, 3 are less than 4

        foreach (var fieldFacet in countPointsResult.Result.Hits)
        {
            fieldFacet.ValueAs<int>().Should().BeOneOf(Enumerable.Range(4, 10));
            fieldFacet.Count.Should().Be(1);
        }
    }
}
