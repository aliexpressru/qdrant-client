using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal class PointsScrollTests : QdrantTestsBase
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
    public async Task ScrollPoints_CollectionDoesNotExist()
    {
        var getPointFromNonexistentCollectionResult
            = await _qdrantHttpClient.ScrollPoints(
                TestCollectionName,
                QdrantFilter.Empty,
                PayloadPropertiesSelector.None,
                CancellationToken.None);

        getPointFromNonexistentCollectionResult.Status.IsSuccess.Should().BeFalse();
        getPointFromNonexistentCollectionResult.Status.Error.Should()
            .Contain(TestCollectionName)
            .And.Contain("doesn't exist");
    }

    [Test]
    public async Task ScrollPoints_PointDoesNotExist()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var getNonexistentPointResult
            = await _qdrantHttpClient.ScrollPoints(
                TestCollectionName,
                QdrantFilter.Empty,
                PayloadPropertiesSelector.None,
                CancellationToken.None);

        getNonexistentPointResult.Status.IsSuccess.Should().BeTrue();
        getNonexistentPointResult.Result.Points.Length.Should().Be(0);
    }

    [Test]
    public async Task ScrollPoints_WithoutFilter()
    {
        var vectorCount = 10;

        var (_, upsertPointsByPointIds, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: vectorCount);

        var readPointsResult = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            QdrantFilter.Empty,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Points.Length.Should().Be(vectorCount);

        foreach (var readPoint in readPointsResult.Result.Points)
        {
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            readPoint.Payload.As<TestPayload>().Integer.Should().Be(expectedPoint.Payload.Integer);
            readPoint.Payload.As<TestPayload>().FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPoint.Payload.As<TestPayload>().Text.Should().Be(expectedPoint.Payload.Text);
        }
    }

    [Test]
    public async Task ScrollPoints_WithFilter()
    {
        //NOTE: there is no point of testing this with every possible filter since we testing his library, not the Qdrant engine
        var vectorSize = 10U;
        var vectorCount = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
        List<int> valuesToMatchAgainst = new(vectorCount);

        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    CreateTestVector(vectorSize),
                    i
                )
            );

            if (i % 2 == 0)
            {
                valuesToMatchAgainst.Add(i);
            }
        }

        Dictionary<ulong, UpsertPointsRequest<TestPayload>.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => p.Id.AsInteger());

        await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = upsertPoints
            },
            CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            Q.Must(
                Q<TestPayload>.MatchAny(p => p.Integer, valuesToMatchAgainst.ToArray())
            ),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Points.Length.Should().Be(valuesToMatchAgainst.Count);

        foreach (var readPoint in readPointsResult.Result.Points)
        {
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            readPoint.Payload.As<TestPayload>().Integer.Should().Be(expectedPoint.Payload.Integer);
            readPoint.Payload.As<TestPayload>().FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPoint.Payload.As<TestPayload>().Text.Should().Be(expectedPoint.Payload.Text);
        }
    }

    [Test]
    public async Task ScrollPoints_Paginated()
    {
        // arrange

        var vectorCount = 10;
        uint pageSize = 5;

        var (_, upsertPointsByPointIds, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: vectorCount);

        // act 1

        var readPointsFirstPageResult = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            QdrantFilter.Empty,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true,
            limit: pageSize);

        // assert 1

        readPointsFirstPageResult.Status.IsSuccess.Should().BeTrue();
        readPointsFirstPageResult.Result.Points.Length.Should().Be((int)pageSize);
        readPointsFirstPageResult.Result.NextPageOffset.Should().NotBeNull();

        foreach (var readPoint in readPointsFirstPageResult.Result.Points)
        {
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            readPoint.Payload.As<TestPayload>().Integer.Should().Be(expectedPoint.Payload.Integer);
            readPoint.Payload.As<TestPayload>().FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPoint.Payload.As<TestPayload>().Text.Should().Be(expectedPoint.Payload.Text);
        }

        // act 2

        var readPointsSecondPageResult = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            QdrantFilter.Empty,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true,
            limit: pageSize,
            offsetPoint: readPointsFirstPageResult.Result.NextPageOffset);

        // assert 2

        readPointsSecondPageResult.Status.IsSuccess.Should().BeTrue();
        readPointsSecondPageResult.Result.Points.Length.Should().Be((int) pageSize);
        readPointsSecondPageResult.Result.NextPageOffset.Should().BeNull();

        foreach (var readPoint in readPointsSecondPageResult.Result.Points)
        {
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            readPoint.Payload.As<TestPayload>().Integer.Should().Be(expectedPoint.Payload.Integer);
            readPoint.Payload.As<TestPayload>().FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPoint.Payload.As<TestPayload>().Text.Should().Be(expectedPoint.Payload.Text);
        }
    }
}
