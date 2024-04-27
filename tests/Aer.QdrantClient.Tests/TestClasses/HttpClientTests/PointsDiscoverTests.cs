using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class PointsDiscoverTests : QdrantTestsBase
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
    public async Task DiscoverPoints_CollectionDoesNotExist()
    {
        var discoverPointInNonexistentCollectionResult
            = await _qdrantHttpClient.DiscoverPoints(
                TestCollectionName,
                DiscoverPointsRequest.ByVectorExamples(
                    new[]
                    {
                        new KeyValuePair<float[], float[]>(
                            CreateTestFloatVector(10),
                            CreateTestFloatVector(10)
                        )
                    },
                    limit: 10,
                    target: CreateTestFloatVector(10)),
                CancellationToken.None);

        discoverPointInNonexistentCollectionResult.Status.IsSuccess.Should().BeFalse();
        discoverPointInNonexistentCollectionResult.Status.Error.Should()
            .Contain(TestCollectionName)
            .And.Contain("doesn't exist");
    }

    [Test]
    public async Task DiscoverPoints_PointDoesNotExist()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var discoverNonexistentPointResult
            = await _qdrantHttpClient.DiscoverPoints(
                TestCollectionName,
                DiscoverPointsRequest.ByVectorExamples(
                    new[]
                    {
                        new KeyValuePair<float[], float[]>(
                            CreateTestFloatVector(10),
                            CreateTestFloatVector(10)
                        )
                    },
                    limit: 10,
                    target: CreateTestFloatVector(10)),
                CancellationToken.None);

        discoverNonexistentPointResult.Status.IsSuccess.Should().BeTrue();

        discoverNonexistentPointResult.Result.Length.Should().Be(0);
    }

    [Test]
    public async Task DiscoverPoints_OnePoint_ById()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var vector1Vector2Vector = CreateTestFloatVector(vectorSize);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>(){
            new(
                PointId.Integer(1),
                vector1Vector2Vector,
                new TestPayload()
                {
                    Integer = 1,
                    Text = "1"
                }),
            new(
                PointId.Integer(2), // same vector but with different id, this one is expected to be recommended
                vector1Vector2Vector,
                new TestPayload()
                {
                    Integer = 2,
                    Text = "2"
                }),
            new(
                PointId.Integer(3),
                CreateTestFloatVector(vectorSize),
                new TestPayload()
                {
                    Integer = 3,
                    Text = "3"
                }
            )
        };

        await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = upsertPoints
            },
            CancellationToken.None);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var positiveExamplePointId = upsertPoints[0].Id;
        var vectorToAvoidPointId = upsertPoints.Last().Id;

        var recommendedPoints = await _qdrantHttpClient.DiscoverPoints(
            TestCollectionName,
            DiscoverPointsRequest.ByPointIds(
                positiveNegativeContextPairs: new[]{
                    new KeyValuePair<PointId, PointId>(positiveExamplePointId, vectorToAvoidPointId)
                },
                limit: 1,
                target: PointId.Integer(1),
                withVector: true,
                withPayload: true),
            CancellationToken.None);

        recommendedPoints.Status.IsSuccess.Should().BeTrue();
        recommendedPoints.Result.Length.Should().Be(1);
        recommendedPoints.Result.First().Id.Should().Be(upsertPoints[1].Id);

        recommendedPoints.Result.Should()
            .AllSatisfy(p => p.Vector.Should().NotBeNull())
            .And.AllSatisfy(p => p.Payload.Should().NotBeNull());
    }

    [Test]
    public async Task DiscoverPoints_OnePoint_ByExample()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var vector1Vector2Vector = CreateTestFloatVector(vectorSize);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
        {
            new(
                PointId.Integer(1),
                vector1Vector2Vector,
                new TestPayload()
                {
                    Integer = 1,
                    Text = "1"
                }),
            new(
                PointId.Integer(2), // same vector but with different id, this one an previous one should be recommended
                vector1Vector2Vector,
                new TestPayload()
                {
                    Integer = 2,
                    Text = "2"
                }),
            new(
                PointId.Integer(3),
                CreateTestFloatVector(vectorSize),
                new TestPayload()
                {
                    Integer = 3,
                    Text = "3"
                }
            )
        };

        await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = upsertPoints
            },
            CancellationToken.None);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var positiveExampleVector = vector1Vector2Vector;
        var vectorToAvoid = upsertPoints.Last().Vector.Default;

        var request = DiscoverPointsRequest.ByVectorExamples(
            positiveNegativeContextPairs: new[]
            {
                new KeyValuePair<float[], float[]>(positiveExampleVector, vectorToAvoid)
            },
            limit: 2,
            target: vector1Vector2Vector,
            withVector: true,
            withPayload: true);

        var recommendedPoints = await _qdrantHttpClient.DiscoverPoints(
            TestCollectionName,
            request,
            CancellationToken.None);

        recommendedPoints.Status.IsSuccess.Should().BeTrue();
        recommendedPoints.Result.Length.Should().Be(2); // recommend two first upsert points

        var orderedResults = recommendedPoints.Result
            .OrderBy(p => p.Id.AsInteger()).ToList();

        orderedResults[0].Id.Should().Be(upsertPoints[0].Id);
        orderedResults[1].Id.Should().Be(upsertPoints[1].Id);

        recommendedPoints.Result.Should()
            .AllSatisfy(p => p.Vector.Should().NotBeNull())
            .And.AllSatisfy(p => p.Payload.Should().NotBeNull());
    }

    [Test]
    public async Task DiscoverPoints_ByIds()
    {
        var vectorSize = 10U;
        var vectorCount = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var singleVector = CreateConstantTestVector(1.1f, vectorSize);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    singleVector,
                    new TestPayload()
                    {
                        Integer = i + 1,
                        Text = (i + 1).ToString()
                    }
                )
            );
        }

        await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = upsertPoints
            },
            CancellationToken.None);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var targetPointId = upsertPoints[0].Id;

        var positiveExamplePointId = upsertPoints[1].Id;
        var negativeExamplePointId = upsertPoints[2].Id;

        var recommendedPoints = await _qdrantHttpClient.DiscoverPoints(
            TestCollectionName,
            DiscoverPointsRequest.ByPointIds(
                positiveNegativeContextPairs: new[]
                    {new KeyValuePair<PointId, PointId>(positiveExamplePointId, negativeExamplePointId)},
                limit: (uint) vectorCount,
                target: targetPointId,
                withVector: true,
                withPayload: true),
            CancellationToken.None);

        recommendedPoints.Status.IsSuccess.Should().BeTrue();
        recommendedPoints.Result.Length.Should().Be(vectorCount-3); // all vectors except the three example ones
        recommendedPoints.Result.Should().AllSatisfy(
            p =>
                p.Id.Should().NotBe(targetPointId)
                    .And.NotBe(negativeExamplePointId)
        );

        recommendedPoints.Result.Should()
            .AllSatisfy(p => p.Vector.Should().NotBeNull())
            .And.AllSatisfy(p => p.Payload.Should().NotBeNull());
    }

    [Test]
    public async Task DiscoverPoints_ByExamples_WithoutTarget()
    {
        var vectorSize = 10U;
        var vectorCount = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var positiveVector = CreateConstantTestVector(1.1f, vectorSize);
        var negtiveVector = CreateConstantTestVector(2.2f, vectorSize);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            var vector = i < 5
                ? positiveVector
                : negtiveVector;

            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    vector,
                    new TestPayload()
                    {
                        Integer = i + 1,
                        Text = (i + 1).ToString()
                    }
                )
            );
        }

        await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = upsertPoints
            },
            CancellationToken.None);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var recommendedPoints = await _qdrantHttpClient.DiscoverPoints(
            TestCollectionName,
            DiscoverPointsRequest.ByVectorExamples(
                positiveNegativeContextPairs: new[]{new KeyValuePair<float[], float[]>(positiveVector, negtiveVector)},
                limit: (uint) vectorCount,
                withVector: true,
                withPayload: true),
            CancellationToken.None);

        recommendedPoints.Status.IsSuccess.Should().BeTrue();
        recommendedPoints.Result.Length.Should().Be(vectorCount); // all points since we supplied all of the existing vectors

        recommendedPoints.Result.Should()
            .AllSatisfy(p => p.Vector.Should().NotBeNull())
            .And.AllSatisfy(p => p.Payload.Should().NotBeNull());
    }

    [Test]
    public async Task DiscoverPointsBatched_ByExamples()
    {
        var vectorSize = 10U;
        var vectorCount = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var positiveVector = CreateConstantTestVector(1.1f, vectorSize);
        var negativeVector = CreateConstantTestVector(2.2f, vectorSize);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            var vector = i < 5
                ? positiveVector
                : negativeVector;

            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    vector,
                    new TestPayload()
                    {
                        Integer = i + 1,
                        Text = (i + 1).ToString()
                    }
                )
            );
        }

        await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = upsertPoints
            },
            CancellationToken.None);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var request1 = DiscoverPointsRequest.ByVectorExamples(
            positiveNegativeContextPairs: new[] {new KeyValuePair<float[], float[]>(positiveVector, negativeVector)},
            (uint) vectorCount,
            withVector: true,
            withPayload: true);

        var request2 = DiscoverPointsRequest.ByVectorExamples(
            positiveNegativeContextPairs: new[] {new KeyValuePair<float[], float[]>(negativeVector, positiveVector)},
            (uint) vectorCount,
            withVector: true,
            withPayload: true);

        var recommendedPoints = await _qdrantHttpClient.DiscoverPointsBatched(
            TestCollectionName,
            new DiscoverPointsBatchedRequest(request1, request2),
            CancellationToken.None);

        recommendedPoints.Status.IsSuccess.Should().BeTrue();

        recommendedPoints.Result.Length.Should().Be(2); // two requests in a batch

        foreach (var pointsForRequestInBatch in recommendedPoints.Result)
        {
            pointsForRequestInBatch.Length.Should()
                .Be(vectorCount); // all points since we supplied all of the existing vectors minus one filtered out

            pointsForRequestInBatch.Should()
                .AllSatisfy(p => p.Vector.Should().NotBeNull())
                .And.AllSatisfy(p => p.Payload.Should().NotBeNull());
        }
    }
}
