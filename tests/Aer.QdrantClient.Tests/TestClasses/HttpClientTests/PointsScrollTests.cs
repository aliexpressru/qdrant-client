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

    [Test]
    public async Task ScrollPoints_OrderBy()
    {
        var vectorCount = 10;

        DateTime startDateTimeValue = DateTime.Parse("2020-01-01T00:00:00");

        var (_, upsertPointsByPointIds, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: vectorCount,
                payloadInitializerFunction: i => new TestPayload
                {
                    Integer = Random.Shared.Next(1, 101),
                    FloatingPointNumber = Math.Round(Random.Shared.NextDouble(), 2, MidpointRounding.AwayFromZero),
                    DateTimeValue = startDateTimeValue.AddDays(i)
                });

        // create collection indexes to enable order by functionality

        var integerIndexCreationResult = await _qdrantHttpClient.CreatePayloadIndex(
            TestCollectionName,
            "integer",
            PayloadIndexedFieldType.Integer,
            CancellationToken.None,
            isWaitForResult: true);

        integerIndexCreationResult.EnsureSuccess();

        var floatIndexCreationResult = await _qdrantHttpClient.CreatePayloadIndex(
            TestCollectionName,
            "floating_point_number",
            PayloadIndexedFieldType.Float,
            CancellationToken.None,
            isWaitForResult: true);

        floatIndexCreationResult.EnsureSuccess();

        var dateTimeIndexCreationResult = await _qdrantHttpClient.CreatePayloadIndex(
            TestCollectionName,
            "date_time_value",
            PayloadIndexedFieldType.Datetime,
            CancellationToken.None,
            isWaitForResult: true);

        dateTimeIndexCreationResult.EnsureSuccess();

        // order by integer ascending

        var readPointsResult = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            QdrantFilter.Empty,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true,
            orderBySelector: "integer");

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Points.Length.Should().Be(vectorCount);

        int previousInteger = 0;

        foreach (var readPoint in readPointsResult.Result.Points)
        {
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPoint.Payload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPoint.Payload.Text);
            readPointPayload.DateTimeValue.Should().Be(expectedPoint.Payload.DateTimeValue);

            readPointPayload.Integer.Should().BeGreaterThanOrEqualTo(previousInteger);

            previousInteger = readPointPayload.Integer!.Value;
        }

        // order by double descending

        var previousDouble = double.MaxValue;

        var readPointsResult2 = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            QdrantFilter.Empty,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true,
            orderBySelector: OrderBySelector.Desc("floating_point_number"));

        readPointsResult2.Status.IsSuccess.Should().BeTrue();
        readPointsResult2.Result.Points.Length.Should().Be(vectorCount);

        foreach (var readPoint in readPointsResult2.Result.Points)
        {
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPoint.Payload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPoint.Payload.Text);
            readPointPayload.DateTimeValue.Should().Be(expectedPoint.Payload.DateTimeValue);

            readPointPayload.FloatingPointNumber.Should().BeLessOrEqualTo(previousDouble);

            previousDouble = readPointPayload.FloatingPointNumber!.Value;
        }

        // order by datetime descending

        var previousDateTime = DateTime.MaxValue;

        var readPointsResult3 = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            QdrantFilter.Empty,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true,
            orderBySelector: OrderBySelector.Desc("date_time_value"));

        readPointsResult3.Status.IsSuccess.Should().BeTrue();
        readPointsResult3.Result.Points.Length.Should().Be(vectorCount);

        foreach (var readPoint in readPointsResult3.Result.Points)
        {
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPoint.Payload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPoint.Payload.Text);
            readPointPayload.DateTimeValue.Should().Be(expectedPoint.Payload.DateTimeValue);

            readPointPayload.DateTimeValue.Should().BeBefore(previousDateTime);

            previousDateTime = readPointPayload.DateTimeValue!.Value;
        }
    }

    [Test]
    public async Task ScrollPoints_OrderBy_StartFrom()
    {
        var vectorCount = 10;

        DateTime startDateTimeValue = DateTime.Parse("2020-01-01T00:00:00");

        var (_, upsertPointsByPointIds, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: vectorCount,
                payloadInitializerFunction: i => new TestPayload
                {
                    Integer = i,
                    FloatingPointNumber = 1.567 + i,
                    DateTimeValue = startDateTimeValue.AddDays(i)
                });

        // create collection indexes to enable order by functionality

        var integerIndexCreationResult = await _qdrantHttpClient.CreatePayloadIndex(
            TestCollectionName,
            "integer",
            PayloadIndexedFieldType.Integer,
            CancellationToken.None,
            isWaitForResult: true);

        integerIndexCreationResult.EnsureSuccess();

        var floatIndexCreationResult = await _qdrantHttpClient.CreatePayloadIndex(
            TestCollectionName,
            "floating_point_number",
            PayloadIndexedFieldType.Float,
            CancellationToken.None,
            isWaitForResult: true);

        floatIndexCreationResult.EnsureSuccess();

        var dateTimeIndexCreationResult = await _qdrantHttpClient.CreatePayloadIndex(
            TestCollectionName,
            "date_time_value",
            PayloadIndexedFieldType.Datetime,
            CancellationToken.None,
            isWaitForResult: true);

        dateTimeIndexCreationResult.EnsureSuccess();

        // order by integer ascending

        var readPointsResult = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            QdrantFilter.Empty,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true,
            orderBySelector: OrderBySelector.Asc("integer", startFrom: 3));

        readPointsResult.Status.IsSuccess.Should().BeTrue();

        // we skip 3 first points by using startFrom
        readPointsResult.Result.Points.Length.Should().Be(vectorCount - 3);

        int previousInteger = 0;

        foreach (var readPoint in readPointsResult.Result.Points)
        {
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPoint.Payload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPoint.Payload.Text);
            readPointPayload.DateTimeValue.Should().Be(expectedPoint.Payload.DateTimeValue);

            readPointPayload.Integer.Should().BeGreaterThanOrEqualTo(previousInteger);

            previousInteger = readPointPayload.Integer!.Value;
        }

        // order by double descending

        var previousDouble = double.MaxValue;

        var readPointsResult2 = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            QdrantFilter.Empty,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true,
            orderBySelector: OrderBySelector.Desc("floating_point_number", startFrom: 4.567));

        readPointsResult2.Status.IsSuccess.Should().BeTrue();

        // we invert points then start getting them from value 4.567
        // which gives us only first 4 points
        readPointsResult2.Result.Points.Length.Should().Be(vectorCount-6);

        foreach (var readPoint in readPointsResult2.Result.Points)
        {
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPoint.Payload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPoint.Payload.Text);
            readPointPayload.DateTimeValue.Should().Be(expectedPoint.Payload.DateTimeValue);

            readPointPayload.FloatingPointNumber.Should().BeLessOrEqualTo(previousDouble);

            previousDouble = readPointPayload.FloatingPointNumber!.Value;
        }

        // order by datetime descending

        var previousDateTime = DateTime.MaxValue;

        var readPointsResult3 = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            QdrantFilter.Empty,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true,
            orderBySelector: OrderBySelector.Desc("date_time_value", startFrom: startDateTimeValue.AddDays(3)));

        readPointsResult3.Status.IsSuccess.Should().BeTrue();

        // we invert points then start getting them from value "2020-01-04T00:00:00"
        // which gives us only first 4 points
        readPointsResult3.Result.Points.Length.Should().Be(vectorCount-6);

        foreach (var readPoint in readPointsResult3.Result.Points)
        {
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPoint.Payload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPoint.Payload.Text);
            readPointPayload.DateTimeValue.Should().Be(expectedPoint.Payload.DateTimeValue);

            readPointPayload.DateTimeValue.Should().BeBefore(previousDateTime);

            previousDateTime = readPointPayload.DateTimeValue!.Value;
        }
    }
}
