using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class CollectionsCompoundOperationsTests : QdrantTestsBase
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
    public async Task TestListCollectionInfo()
    {
        var vectorSize = 10U;
        var vectorCount = 10;

        var collectionCreationResult1 = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                ReplicationFactor = 1
            },
            CancellationToken.None);

        var collectionCreationResult2 = await _qdrantHttpClient.CreateCollection(
            TestCollectionName2,
            new CreateCollectionRequest(VectorDistanceMetric.Cosine, vectorSize, isServeVectorsFromDisk: false)
            {
                OnDiskPayload = true,
                ReplicationFactor = 3
            },
            CancellationToken.None);

        collectionCreationResult1.EnsureSuccess();
        collectionCreationResult2.EnsureSuccess();

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    CreateTestFloat32Vector(vectorSize),
                    i
                )
            );
        }

        var upsertPointsResult1
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = upsertPoints
                },
                CancellationToken.None,
                isWaitForResult: true,
                ordering: OrderingType.Strong);

        var upsertPointsResult2
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = upsertPoints
                },
                CancellationToken.None,
                isWaitForResult: true,
                ordering: OrderingType.Strong);

        upsertPointsResult1.EnsureSuccess();
        upsertPointsResult2.EnsureSuccess();

        List<Task> collectionReadyTasks = [
            _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None),
            _qdrantHttpClient.EnsureCollectionReady(TestCollectionName2, CancellationToken.None)
        ];

        await Task.WhenAll(collectionReadyTasks);

        // call both compound collection info operations

        var listCollectionInfoResult =
            await _qdrantHttpClient.ListCollectionInfo(isCountExactPointsNumber: true, CancellationToken.None);

        var firstCollectionInfoResult = await _qdrantHttpClient.GetCollectionInfo(
            TestCollectionName,
            isCountExactPointsNumber: true,
            CancellationToken.None);

        var secondCollectionInfoResult = await _qdrantHttpClient.GetCollectionInfo(
            TestCollectionName2,
            isCountExactPointsNumber: true,
            CancellationToken.None);

        firstCollectionInfoResult.Status.IsSuccess.Should().BeTrue();
        secondCollectionInfoResult.Status.IsSuccess.Should().BeTrue();

        listCollectionInfoResult.Status.IsSuccess.Should().BeTrue();

        listCollectionInfoResult.Result.Should().HaveCount(2);
        listCollectionInfoResult.Result.Should().ContainKey(TestCollectionName);
        listCollectionInfoResult.Result.Should().ContainKey(TestCollectionName2);

        listCollectionInfoResult.Result[TestCollectionName].Should().BeEquivalentTo(firstCollectionInfoResult.Result);
        listCollectionInfoResult.Result[TestCollectionName2].Should().BeEquivalentTo(secondCollectionInfoResult.Result);
    }
}
