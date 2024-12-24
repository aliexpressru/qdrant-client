using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
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
                new DiscoverPointsRequest(
                    [
                        new KeyValuePair<PointIdOrQueryVector, PointIdOrQueryVector>(
                            CreateTestVector(10),
                            CreateTestVector(10)
                        )
                    ],
                    limit: 10,
                    target: CreateTestVector(10)),
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
                new DiscoverPointsRequest(
                    [
                        new KeyValuePair<PointIdOrQueryVector, PointIdOrQueryVector>(
                            CreateTestVector(10),
                            CreateTestVector(10)
                        )
                    ],
                    limit: 10,
                    target: CreateTestVector(10)),
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
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var vector1Vector2Vector = CreateTestVector(vectorSize);

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
                PointId.Integer(2), // same vector but with different id, this one is expected to be recommended
                vector1Vector2Vector,
                new TestPayload()
                {
                    Integer = 2,
                    Text = "2"
                }),
            new(
                PointId.Integer(3),
                CreateTestVector(vectorSize),
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

        var discoveredPoints = await _qdrantHttpClient.DiscoverPoints(
            TestCollectionName,
            new DiscoverPointsRequest(
                positiveNegativeContextPairs:
                [
                    new KeyValuePair<PointIdOrQueryVector, PointIdOrQueryVector>(positiveExamplePointId, vectorToAvoidPointId)
                ],
                limit: 1,
                target: PointId.Integer(1),
                withVector: true,
                withPayload: true),
            CancellationToken.None);

        discoveredPoints.Status.IsSuccess.Should().BeTrue();
        discoveredPoints.Result.Length.Should().Be(1);
        discoveredPoints.Result.First().Id.Should().Be(upsertPoints[1].Id);

        discoveredPoints.Result.Should()
            .AllSatisfy(p => p.Vector.Should().NotBeNull())
            .And.AllSatisfy(p => p.Payload.Should().NotBeNull());
    }

    [Test]
    public async Task DiscoverPoints_OnePoint_ByExample()
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

        var vector1Vector2Vector = CreateTestVector(vectorSize);

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
                PointId.Integer(2), // same vector but with different id, this one and a previous one should be recommended
                vector1Vector2Vector,
                new TestPayload()
                {
                    Integer = 2,
                    Text = "2"
                }),
            new(
                PointId.Integer(3),
                CreateTestVector(vectorSize),
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
        var vectorToAvoid = upsertPoints.Last().Vector.AsDenseVector().VectorValues;

        var request = new DiscoverPointsRequest(
            positiveNegativeContextPairs:
            [
                new KeyValuePair<PointIdOrQueryVector, PointIdOrQueryVector>(positiveExampleVector, vectorToAvoid)
            ],
            limit: 2,
            target: vector1Vector2Vector,
            withVector: true,
            withPayload: true);

        var discoveredPoints = await _qdrantHttpClient.DiscoverPoints(
            TestCollectionName,
            request,
            CancellationToken.None);

        discoveredPoints.Status.IsSuccess.Should().BeTrue();
        discoveredPoints.Result.Length.Should().Be(2); // recommend two first upsert points

        var orderedResults = discoveredPoints.Result
            .OrderBy(p => p.Id.AsInteger()).ToList();

        orderedResults[0].Id.Should().Be(upsertPoints[0].Id);
        orderedResults[1].Id.Should().Be(upsertPoints[1].Id);

        discoveredPoints.Result.Should()
            .AllSatisfy(p => p.Vector.Should().NotBeNull())
            .And.AllSatisfy(p => p.Payload.Should().NotBeNull());
    }

    [Test]
    public async Task DiscoverPoints_OnePoint_ByExample_SparseVector()
    {
        var vectorSize = 10U;

        Dictionary<string, SparseVectorConfiguration> sparseVectors = new()
        {
            [VectorBase.DefaultVectorName] = new(onDisk: true, fullScanThreshold: 1000),
            ["Vector_2"] = new(onDisk: true), // default sparse vector configuration
        };

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(sparseVectorsConfiguration: sparseVectors)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var vector1Vector2Vector = CreateTestSparseVector(vectorSize, 2);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
        {
            new(
                PointId.Integer(1),
                vector: new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        [VectorBase.DefaultVectorName] = vector1Vector2Vector,
                        ["Vector_2"] = CreateTestSparseVector(vectorSize, 5),
                    }
                },
                new TestPayload()
                {
                    Integer = 1,
                    Text = "1"
                }),
            new(
                PointId.Integer(2), // same vector but with different id, this one and a previous one should be recommended
                vector: new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        [VectorBase.DefaultVectorName] = vector1Vector2Vector,
                        ["Vector_2"] = CreateTestSparseVector(vectorSize, 5),
                    }
                },
                new TestPayload()
                {
                    Integer = 2,
                    Text = "2"
                }),
            new(
                PointId.Integer(3),
                vector: new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        [VectorBase.DefaultVectorName] = CreateTestSparseVector(vectorSize, 5),
                        ["Vector_2"] = CreateTestSparseVector(vectorSize, 5)
                    }
                },
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
        var vectorToAvoid = upsertPoints.Last().Vector.AsSparseVector();

        var request = new DiscoverPointsRequest(
            positiveNegativeContextPairs:
            [
                new KeyValuePair<PointIdOrQueryVector, PointIdOrQueryVector>(positiveExampleVector, vectorToAvoid)
            ],
            limit: 2,
            target: vector1Vector2Vector,
            withVector: true,
            withPayload: true){
            Using = VectorBase.DefaultVectorName
        };

        var discoveredPoints = await _qdrantHttpClient.DiscoverPoints(
            TestCollectionName,
            request,
            CancellationToken.None);

        discoveredPoints.Status.IsSuccess.Should().BeTrue();
        discoveredPoints.Result.Length.Should().Be(2); // recommend two first upsert points

        discoveredPoints.Result.Should()
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
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true)
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

        var discoveredPoints = await _qdrantHttpClient.DiscoverPoints(
            TestCollectionName,
            new DiscoverPointsRequest(
                positiveNegativeContextPairs:
                [
                    new KeyValuePair<PointIdOrQueryVector, PointIdOrQueryVector>(positiveExamplePointId, negativeExamplePointId)
                ],
                limit: (uint) vectorCount,
                target: targetPointId,
                withVector: true,
                withPayload: true),
            CancellationToken.None);

        discoveredPoints.Status.IsSuccess.Should().BeTrue();
        discoveredPoints.Result.Length.Should().Be(vectorCount-3); // all vectors except the three example ones
        discoveredPoints.Result.Should().AllSatisfy(
            p =>
                p.Id.Should().NotBe(targetPointId)
                    .And.NotBe(negativeExamplePointId)
        );

        discoveredPoints.Result.Should()
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
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true)
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

        var discoveredPoints = await _qdrantHttpClient.DiscoverPoints(
            TestCollectionName,
            new DiscoverPointsRequest(
                positiveNegativeContextPairs:
                [
                    new KeyValuePair<PointIdOrQueryVector, PointIdOrQueryVector>(positiveVector, negativeVector)
                ],
                limit: (uint) vectorCount,
                withVector: true,
                withPayload: true),
            CancellationToken.None);

        discoveredPoints.Status.IsSuccess.Should().BeTrue();
        discoveredPoints.Result.Length.Should().Be(vectorCount); // all points since we supplied all the existing vectors

        discoveredPoints.Result.Should()
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
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true)
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

        var request1 = new DiscoverPointsRequest(
            positiveNegativeContextPairs:
            [
                new KeyValuePair<PointIdOrQueryVector, PointIdOrQueryVector>(positiveVector, negativeVector)
            ],
            (uint) vectorCount,
            withVector: true,
            withPayload: true);

        var request2 = new DiscoverPointsRequest(
            positiveNegativeContextPairs:
            [
                new KeyValuePair<PointIdOrQueryVector, PointIdOrQueryVector>(negativeVector, positiveVector)
            ],
            (uint) vectorCount,
            withVector: true,
            withPayload: true);

        var discoveredPoints = await _qdrantHttpClient.DiscoverPointsBatched(
            TestCollectionName,
            new DiscoverPointsBatchedRequest(request1, request2),
            CancellationToken.None);

        discoveredPoints.Status.IsSuccess.Should().BeTrue();

        discoveredPoints.Result.Length.Should().Be(2); // two requests in a batch

        foreach (var pointsForRequestInBatch in discoveredPoints.Result)
        {
            pointsForRequestInBatch.Length.Should()
                .Be(vectorCount); // all points since we supplied all the existing vectors minus one filtered out

            pointsForRequestInBatch.Should()
                .AllSatisfy(p => p.Vector.Should().NotBeNull())
                .And.AllSatisfy(p => p.Payload.Should().NotBeNull());
        }
    }
}
