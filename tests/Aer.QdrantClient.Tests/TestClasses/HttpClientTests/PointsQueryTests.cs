using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;
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
}
