using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class PointsQueryTests : QdrantTestsBase
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
    public async Task PrefetchWithoutQuery()
    {
        await PrepareCollection<TestPayload>(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: 10);

        var nearestPointsResponse = await _qdrantHttpClient.QueryPoints(
            TestCollectionName,
            new QueryPointsRequest()
            {
                Prefetch =
                [
                    new PrefetchPoints()
                    {
                        Filter = Q<TestPayload>.BeInRange(
                            p => p.Integer,
                            greaterThanOrEqual: 0,
                            lessThanOrEqual: 2)
                    }
                ],
                WithPayload = true,
                WithVector = true,
                Limit = 10
            },
            CancellationToken.None);

        nearestPointsResponse.Status.IsSuccess.Should().BeFalse();
        nearestPointsResponse.Status.GetErrorMessage().Should().Contain(
            "A query is needed to merge the prefetches. Can't have prefetches without defining a query.");
    }

    [Test]
    public async Task FindNearestPoints()
    {
        var vectorCount = 10;

        var (_, upsertPointsByPointIds, _) =
            await PrepareCollection<TestPayload>(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: vectorCount);

        var nearestPointsByPointIdResponse = await _qdrantHttpClient.QueryPoints(
            TestCollectionName,
            new QueryPointsRequest()
            {
                Query = PointsQuery.CreateFindNearestPointsQuery(upsertPointsByPointIds.First().Value.Id),
                WithPayload = true,
                WithVector = true,
                Limit = 2
            },
            CancellationToken.None);

        var nearestPointsByVectorResponse = await _qdrantHttpClient.QueryPoints(
            TestCollectionName,
            new QueryPointsRequest()
            {
                Query = PointsQuery.CreateFindNearestPointsQuery(upsertPointsByPointIds.First().Value.Vector),
                WithPayload = true,
                WithVector = true,
                Limit = 2
            },
            CancellationToken.None);

        nearestPointsByPointIdResponse.Status.IsSuccess.Should().BeTrue();
        nearestPointsByPointIdResponse.Result.Points.Length.Should().Be(2);

        nearestPointsByVectorResponse.Status.IsSuccess.Should().BeTrue();
        nearestPointsByVectorResponse.Result.Points.Length.Should().Be(2);
    }

    [Test]
    public async Task FindNearestPoints_WithPrefetch()
    {
        var vectorCount = 10;

        var (_, upsertPointsByPointIds, _) =
            await PrepareCollection<TestPayload>(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: vectorCount);

        var nearestPointsResponse = await _qdrantHttpClient.QueryPoints(
            TestCollectionName,
            new QueryPointsRequest()
            {
                Prefetch =
                [
                    new PrefetchPoints()
                    {
                        Filter = Q<TestPayload>.BeInRange(
                            p => p.Integer,
                            greaterThanOrEqual: 0,
                            lessThanOrEqual: 2)
                    }
                ],
                Query = PointsQuery.CreateFindNearestPointsQuery(upsertPointsByPointIds.First().Value.Id),
                WithPayload = true,
                WithVector = true,
                Limit = 10
            },
            CancellationToken.None);

        nearestPointsResponse.Status.IsSuccess.Should().BeTrue();
        // less than limit since prefetch should eliminate all points but 3
        nearestPointsResponse.Result.Points.Length.Should().Be(2);

        nearestPointsResponse.Result.Points.Should().AllSatisfy(
            p =>
                p.Payload.As<TestPayload>().Integer.Should().BeInRange(0, 2)
        );
    }

    [Test]
    public async Task Fusion()
    {
        var vectorCount = 10;

        var (_, upsertPointsByPointIds, _) =
            await PrepareCollection<TestPayload>(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: vectorCount);

        var nearestPointsResponse = await _qdrantHttpClient.QueryPoints(
            TestCollectionName,
            new QueryPointsRequest()
            {
                Prefetch =
                [
                    new PrefetchPoints()
                    {
                        Filter = Q<TestPayload>.BeInRange(
                            p => p.Integer,
                            greaterThanOrEqual: 0,
                            lessThanOrEqual: 2),
                        Limit = 10
                    },
                    new PrefetchPoints()
                    {
                        Query = PointsQuery.CreateFindNearestPointsQuery(upsertPointsByPointIds.First().Value.Id),
                        Limit = 5
                    }
                ],
                Query = PointsQuery.CreateFusionQuery(),
                WithPayload = true,
                WithVector = true
            },
            CancellationToken.None);

        nearestPointsResponse.Status.IsSuccess.Should().BeTrue();
        // first prefetch returns 3 points
        // second prefetch returns up to 5 points
        nearestPointsResponse.Result.Points.Length.Should().BeInRange(5, 8);
    }

    [Test]
    public async Task OrderByPoints()
    {
        var vectorCount = 10;

        await PrepareCollection<TestPayload>(
            _qdrantHttpClient,
            TestCollectionName,
            vectorCount: vectorCount);

        // order by does not work without indexes
        await _qdrantHttpClient.CreatePayloadIndex(
            TestCollectionName,
            "integer",
            PayloadIndexedFieldType.Integer,
            CancellationToken.None,
            isWaitForResult: true);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var orderedPointsResponse = await _qdrantHttpClient.QueryPoints(
            TestCollectionName,
            new QueryPointsRequest()
            {
                Prefetch =
                [
                    new PrefetchPoints()
                    {
                        Query = PointsQuery.CreateOrderByQuery(
                            OrderBySelector<TestPayload>.Desc(p => p.Integer)),
                        Limit = 2
                    }
                ],
                Query = PointsQuery.CreateOrderByQuery(
                    OrderBySelector<TestPayload>.Asc(p => p.Integer)),
                WithPayload = true,
                WithVector = true
            },
            CancellationToken.None);

        orderedPointsResponse.Status.IsSuccess.Should().BeTrue();
        orderedPointsResponse.Result.Points.Length.Should().Be(2);

        var orderedPoints = orderedPointsResponse.Result.Points.OrderBy(p => p.Score).ToList();

        var firstPoint = orderedPoints.First();
        var secondPoint = orderedPoints.Skip(1).First();

        firstPoint.Payload.As<TestPayload>().Integer!.Value
            .Should().BeLessThan(secondPoint.Payload.As<TestPayload>().Integer!.Value);
    }
}
