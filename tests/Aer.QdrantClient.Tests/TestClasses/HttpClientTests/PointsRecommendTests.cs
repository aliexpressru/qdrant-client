using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Helpers;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class PointsRecommendTests : QdrantTestsBase
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
    public async Task RecommendPoints_CollectionDoesNotExist()
    {
        var recommendPointInNonexistentCollectionResult
            = await _qdrantHttpClient.RecommendPoints(
                TestCollectionName,
                RecommendPointsRequest.ByVectorExamples(new[] {CreateTestVector(10)}, 10),
                CancellationToken.None);

        recommendPointInNonexistentCollectionResult.Status.IsSuccess.Should().BeFalse();
        recommendPointInNonexistentCollectionResult.Status.Error.Should()
            .Contain(TestCollectionName)
            .And.Contain("doesn't exist");
    }

    [Test]
    public async Task RecommendPoints_PointDoesNotExist()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var recommendNonexistentPointResult
            = await _qdrantHttpClient.RecommendPoints(
                TestCollectionName,
                RecommendPointsRequest.ByVectorExamples(new[] {CreateTestVector(10)}, 10),
                CancellationToken.None);

        recommendNonexistentPointResult.Status.IsSuccess.Should().BeTrue();

        recommendNonexistentPointResult.Result.Length.Should().Be(0);
    }

    [Test]
    public async Task RecommendPoints_OnePoint_ById_WithoutFilter()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var vector1Vector2Vector = CreateTestVector(vectorSize);

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

        var recommendedPoints = await _qdrantHttpClient.RecommendPoints(
            TestCollectionName,
            RecommendPointsRequest.ByPointIds(
                positiveExamplePointId.YieldSingle(),
                1,
                negativeVectorExamples: vectorToAvoidPointId.YieldSingle(),
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
    public async Task RecommendPoints_OnePoint_ByExample_WithoutFilter()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
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
                PointId.Integer(2), // same vector but with different id, this one an previous one should be recommended
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
        var vectorToAvoid = upsertPoints.Last().Vector.Default;

        var request = RecommendPointsRequest.ByVectorExamples(
            positiveExampleVector.YieldSingle(),
            2,
            negativeVectorExamples: vectorToAvoid.YieldSingle(),
            withVector: true,
            withPayload: true);

        // this is set only in this test since we want to ensure that serialization works ok
        request.Strategy = RecommendStrategy.AverageVector;

        var recommendedPoints = await _qdrantHttpClient.RecommendPoints(
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
    public async Task RecommendPoints_ByIds_WithoutFilter()
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

        var positiveExamplePointId1 = upsertPoints[0].Id;
        var positiveExamplePointId2 = upsertPoints[1].Id;

        var recommendedPoints = await _qdrantHttpClient.RecommendPoints(
            TestCollectionName,
            RecommendPointsRequest.ByPointIds(
                new[]{positiveExamplePointId1, positiveExamplePointId2},
                (uint) vectorCount,
                withVector: true,
                withPayload: true),
            CancellationToken.None);

        recommendedPoints.Status.IsSuccess.Should().BeTrue();
        recommendedPoints.Result.Length.Should().Be(vectorCount-2); // all vectors except the two example ones
        recommendedPoints.Result.Should().AllSatisfy(
            p =>
                p.Id.Should().NotBe(positiveExamplePointId1)
                    .And.NotBe(positiveExamplePointId2)
        );

        recommendedPoints.Result.Should()
            .AllSatisfy(p => p.Vector.Should().NotBeNull())
            .And.AllSatisfy(p => p.Payload.Should().NotBeNull());
    }

    [Test]
    public async Task RecommendPoints_ByExamples_WithoutFilter()
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

        var singleVector1 = CreateConstantTestVector(1.1f, vectorSize);
        var singleVector2 = CreateConstantTestVector(1.1f, vectorSize);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            var vector = i < 5
                ? singleVector1
                : singleVector2;

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

        var recommendedPoints = await _qdrantHttpClient.RecommendPoints(
            TestCollectionName,
            RecommendPointsRequest.ByVectorExamples(
                new[]{singleVector1, singleVector2},
                (uint) vectorCount,
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
    public async Task RecommendPoints_ByIds_WithFilter()
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

        var positiveExamplePointId1 = upsertPoints[0].Id;
        var positiveExamplePointId2 = upsertPoints[1].Id;
        var filterOutPointId = upsertPoints.Last().Id;

        var request = RecommendPointsRequest.ByPointIds(
            new[] {positiveExamplePointId1, positiveExamplePointId2},
            (uint) vectorCount,
            withVector: true,
            withPayload: true);

        request.Filter = QdrantFilter.Create(
            Q.MustNot(
                Q.HaveAnyId(filterOutPointId)
            )
        );

        var recommendedPoints = await _qdrantHttpClient.RecommendPoints(
            TestCollectionName,
            request,
            CancellationToken.None);

        recommendedPoints.Status.IsSuccess.Should().BeTrue();
        recommendedPoints.Result.Length.Should().Be(vectorCount - 3); // all vectors except the two example ones and one filtered out
        recommendedPoints.Result.Should()
            .AllSatisfy(p =>
                p.Id.Should().NotBe(positiveExamplePointId1)
                    .And.NotBe(positiveExamplePointId2)
                    .And.NotBe(filterOutPointId));

        recommendedPoints.Result.Should()
            .AllSatisfy(p => p.Vector.Should().NotBeNull())
            .And.AllSatisfy(p => p.Payload.Should().NotBeNull());
    }

    [Test]
    public async Task RecommendPoints_ByExamples_WithFilter()
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

        var singleVector1 = CreateConstantTestVector(1.1f, vectorSize);
        var singleVector2 = CreateConstantTestVector(1.1f, vectorSize);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            var vector = i < 5
                ? singleVector1
                : singleVector2;

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

        var request = RecommendPointsRequest.ByVectorExamples(
            new[] {singleVector1, singleVector2},
            (uint) vectorCount,
            withVector: true,
            withPayload: true);

        var filterOutPointId = upsertPoints.Last().Id;

        request.Filter = QdrantFilter.Create(
            Q.MustNot(
                Q.HaveAnyId(filterOutPointId)
            )
        );

        var recommendedPoints = await _qdrantHttpClient.RecommendPoints(
            TestCollectionName,
            request,
            CancellationToken.None);

        recommendedPoints.Status.IsSuccess.Should().BeTrue();
        recommendedPoints.Result.Length.Should()
            .Be(vectorCount - 1); // all points since we supplied all of the existing vectors minus one filtered out

        recommendedPoints.Result.Should()
            .AllSatisfy(p => p.Id.Should().NotBe(filterOutPointId));

        recommendedPoints.Result.Should()
            .AllSatisfy(p => p.Vector.Should().NotBeNull())
            .And.AllSatisfy(p => p.Payload.Should().NotBeNull());
    }

    [Test]
    public async Task RecommendPointsBatched_ByExamples_WithFilter()
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

        var singleVector1 = CreateConstantTestVector(1.1f, vectorSize);
        var singleVector2 = CreateConstantTestVector(1.1f, vectorSize);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            var vector = i < 5
                ? singleVector1
                : singleVector2;

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

        var filterOutPointId = upsertPoints.Last().Id;

        var request1 = RecommendPointsRequest.ByVectorExamples(
            new[] {singleVector1},
            (uint) vectorCount,
            withVector: true,
            withPayload: true);

        request1.Filter = QdrantFilter.Create(
            Q.MustNot(
                Q.HaveAnyId(filterOutPointId)
            )
        );

        var request2 = RecommendPointsRequest.ByVectorExamples(
            new[] {singleVector2},
            (uint) vectorCount,
            withVector: true,
            withPayload: true);

        request2.Filter = QdrantFilter.Create(
            Q.MustNot(
                Q.HaveAnyId(filterOutPointId)
            )
        );

        var recommendedPoints = await _qdrantHttpClient.RecommendPointsBatched(
            TestCollectionName,
            new RecommendPointsBatchedRequest(request1, request2),
            CancellationToken.None);

        recommendedPoints.Status.IsSuccess.Should().BeTrue();

        recommendedPoints.Result.Length.Should().Be(2); // two requests in a batch

        foreach (var pointsForRequestInBatch in recommendedPoints.Result)
        {
            pointsForRequestInBatch.Length.Should()
                .Be(vectorCount - 1); // all points since we supplied all of the existing vectors minus one filtered out

            pointsForRequestInBatch.Should()
                .AllSatisfy(p => p.Id.Should().NotBe(filterOutPointId));

            pointsForRequestInBatch.Should()
                .AllSatisfy(p => p.Vector.Should().NotBeNull())
                .And.AllSatisfy(p => p.Payload.Should().NotBeNull());
        }
    }

    [Test]
    public async Task RecommendPointsGrouped_ByExamples_WithFilter()
    {
        var vectorSize = 10U;
        var vectorCount = 11; // +1 from usual 10 vectors for test assertions to be similar to SearchPointsGrouped

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var singleVector1 = CreateConstantTestVector(1.1f, vectorSize);
        var singleVector2 = CreateConstantTestVector(1.1f, vectorSize);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            var vector = i < 5
                ? singleVector1
                : singleVector2;

            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    vector,
                    new TestPayload()
                    {
                        Integer = i < 5 ? 1 : 2,
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

        var request = RecommendPointsGroupedRequest.ByVectorExamples(
            new[] {singleVector1, singleVector2},
            groupBy: Q<TestPayload>.GetPayloadFieldName(p=>p.Integer),
            groupsLimit: 2,
            groupSize: 10,
            withVector: true,
            withPayload: true);

        var filterOutPointId = upsertPoints.Last().Id;

        request.Filter = QdrantFilter.Create(
            Q.MustNot(
                Q.HaveAnyId(filterOutPointId)
            )
        );

        var recommendedPoints = await _qdrantHttpClient.RecommendPointsGrouped(
            TestCollectionName,
            request,
            CancellationToken.None);

        recommendedPoints.Status.IsSuccess.Should().BeTrue();

        recommendedPoints.Result.Groups.Length.Should().Be(2); // 2 possible values of Integer payload property

        recommendedPoints.Result.Groups.Should()
            .AllSatisfy(g => g.Hits.Length.Should().Be(vectorCount / 2))
            .And.AllSatisfy(
                g => g.Hits.Should()
                    .AllSatisfy(h => h.Payload.Should().NotBeNull())
                    .And.AllSatisfy(h => h.Vector.Should().NotBeNull())
            );
    }
}
