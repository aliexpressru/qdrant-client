using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
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
    public async Task CollectionDoesNotExist()
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
    public async Task PointDoesNotExist()
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
    public async Task WithoutFilter()
    {
        var vectorCount = 10;

        var (_, upsertPointsByPointIds, _) =
            await PrepareCollection<TestPayload>(
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
    public async Task WithFilter()
    {
        // NOTE: there is no point of testing this with every possible filter
        // since we are testing this library, not the Qdrant engine
        // we are adding tests for specific filters as needed
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

        var startDateTime = DateTime.Parse("2020-01-01T00:00:00");

        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    CreateTestVector(vectorSize),
                    new TestPayload(){
                        Integer = i,
                        DateTimeValue = startDateTime.AddDays(i)
                    }
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

        var readPointsRangeResult = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            Q.Must(
                Q<TestPayload>.BeInRange(
                    p => p.DateTimeValue,
                    greaterThan: startDateTime.AddDays(2),
                    lessThanOrEqual: startDateTime.AddDays(upsertPoints.Count - 1))
            ),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        readPointsRangeResult.Status.IsSuccess.Should().BeTrue();
        // -3 since one end of the BeInRange filter is exclusive and the second one is not
        readPointsRangeResult.Result.Points.Length.Should().Be(upsertPoints.Count - 3);

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
    public async Task WithFilter_ArrayPayloadProperty()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        List<UpsertPointsRequest<TestComplexPayload>.UpsertPoint> upsertPoints = [
            new(
                PointId.Integer((ulong) 1),
                CreateTestVector(vectorSize),
                new TestComplexPayload()
                {
                    Array = [1, 2, 3],
                    StringArray = ["a", "b", "c"]
                }
            ),
            new(
                PointId.Integer((ulong) 2),
                CreateTestVector(vectorSize),
                new TestComplexPayload()
                {
                    Array = [3, 4, 5],
                    StringArray = ["c", "d", "e"]
                }
            ),
            new(
                PointId.Integer((ulong) 3),
                CreateTestVector(vectorSize),
                new TestComplexPayload()
                {
                    Array = [7, 8, 9],
                    StringArray = ["f", "g", "h"]
                }
            )
        ];

        (await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestComplexPayload>()
            {
                Points = upsertPoints
            },
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClient.CreatePayloadIndex(
            TestCollectionName,
            ReflectionHelper.GetPayloadFieldName<TestComplexPayload, string[]>(p => p.StringArray),
            PayloadIndexedFieldType.Keyword,
            CancellationToken.None,
            isWaitForResult: true)).EnsureSuccess();

        (await _qdrantHttpClient.CreatePayloadIndex(
            TestCollectionName,
            ReflectionHelper.GetPayloadFieldName<TestComplexPayload, int[]>(p => p.Array),
            PayloadIndexedFieldType.Integer,
            CancellationToken.None,
            isWaitForResult: true)).EnsureSuccess();

        var readPointsResult = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            Q.Must(
                Q<TestComplexPayload>.MatchValue(p => p.StringArray, "c")
                & Q<TestComplexPayload>.MatchValue(p => p.Array, 3)
                // this condition is here to test ReflectionHelper's ability to reflect correct array names
                & Q<TestComplexPayload>.HaveValuesCount(p => p.Array, greaterThanOrEqual: 3)
            ),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        // since we have 2 points that have both "c" and 3 in array elements;
        readPointsResult.Result.Points.Length.Should().Be(2);

        readPointsResult.Result.Points.Should()
            .AllSatisfy(p => p.Payload.As<TestComplexPayload>().StringArray.Contains("c").Should().BeTrue());
    }

    [Test]
    public async Task Paginated()
    {
        // arrange

        var vectorCount = 10;
        uint pageSize = 5;

        var (_, upsertPointsByPointIds, _) =
            await PrepareCollection<TestPayload>(
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
    public async Task OrderBy()
    {
        var vectorCount = 10;

        DateTimeOffset startDateTimeValue = DateTime.Parse("2020-01-01T00:00:00");

        var (_, upsertPointsByPointIds, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: vectorCount,
                payloadInitializerFunction: i => new TestPayload
                {
                    Integer = Random.Next(1, 101),
                    FloatingPointNumber = Math.Round(Random.NextDouble(), 2, MidpointRounding.AwayFromZero),
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

            readPoint.OrderValue.Should().NotBe(0);

            previousDouble = readPointPayload.FloatingPointNumber!.Value;
        }

        // order by datetime descending

        var previousDateTime = DateTimeOffset.MaxValue;

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

            readPoint.OrderValue.Should().NotBe(0);

            previousDateTime = readPointPayload.DateTimeValue!.Value;
        }
    }

    [Test]
    public async Task OrderBy_StartFrom()
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

            readPoint.OrderValue.Should().NotBe(0);

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

            readPoint.OrderValue.Should().NotBe(0);

            previousDouble = readPointPayload.FloatingPointNumber!.Value;
        }

        // order by datetime descending

        var previousDateTime = DateTimeOffset.MaxValue;
        DateTimeOffset startFromDateTime = startDateTimeValue.AddDays(3);

        var readPointsResult3 = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            QdrantFilter.Empty,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true,
            orderBySelector: OrderBySelector.Desc("date_time_value", startFrom: startFromDateTime));

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

            readPoint.OrderValue.Should().NotBe(0);

            previousDateTime = readPointPayload.DateTimeValue!.Value;
        }
    }

    [Test]
    public async Task ScrollPoints_NamedVectors()
    {
        Dictionary<string, VectorConfigurationBase.SingleVectorConfiguration> namedVectors = new()
        {
            ["Vector_1"] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Dot,
                100,
                isServeVectorsFromDisk: true),
            ["Vector_2"] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Euclid,
                5,
                isServeVectorsFromDisk: false),
            ["Vector_3"] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Cosine,
                50,
                isServeVectorsFromDisk: true),
        };

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(namedVectors)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        UpsertPointsRequest<TestPayload>.UpsertPoint firstPoint =
            new(
                id: 1,
                vector: new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        // here and further on float[] will be implicitly converted to Vector
                        ["Vector_1"] = CreateTestVector(100U),
                        ["Vector_2"] = CreateTestVector(5U),
                        ["Vector_3"] = CreateTestVector(50U),
                    }
                },
                payload: 1);

        UpsertPointsRequest<TestPayload>.UpsertPoint secondPoint = new(
            id: 2,
            vector: new NamedVectors()
            {
                Vectors = new Dictionary<string, VectorBase>()
                {
                    ["Vector_2"] = CreateTestVector(5U),
                    ["Vector_3"] = CreateTestVector(50U),
                }
            },
            payload: 2);

        UpsertPointsRequest<TestPayload>.UpsertPoint thirdPoint = new(
            id: 3,
            vector: new NamedVectors()
            {
                Vectors = new Dictionary<string, VectorBase>()
                {
                    ["Vector_1"] = CreateTestVector(100U),
                }
            },
            payload: 3);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
        {
            firstPoint,
            secondPoint,
            thirdPoint
        };
        
        var upsertPointsResult = await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = upsertPoints
            },
            CancellationToken.None);

        upsertPointsResult.EnsureSuccess();

        var scrollPointsResponse =
            await _qdrantHttpClient.ScrollPoints(
                TestCollectionName,
                filter: Q.HasNamedVector("Vector_1"),
                withPayload: true,
                CancellationToken.None);

        scrollPointsResponse.Status.IsSuccess.Should().BeTrue();

        scrollPointsResponse.Result.Points.Length.Should().Be(2); // only two points of three have "Vector_1" named vector

        var foundPointIds = 
            scrollPointsResponse.Result.Points.Select(p => p.Id.AsInteger()).ToHashSet();
        
        foundPointIds.Should().Contain(1);
        foundPointIds.Should().Contain(3);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task FullText(bool createFullTextIndex)
    {
        var vectorSize = 10U;
        var vectorCount = 10;

        (await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None)).EnsureSuccess();

        if (createFullTextIndex)
        {
            (await _qdrantHttpClient.CreateFullTextPayloadIndex(
                TestCollectionName,
                "text",
                PayloadIndexedTextFieldTokenizerType.Prefix,
                cancellationToken: CancellationToken.None,
                retryCount: 0)).EnsureSuccess();
        }

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();

        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    CreateTestVector(vectorSize),
                    new TestPayload()
                    {
                        Text = $"test text_{i} {i}"
                    }
                )
            );
        }
        
        (await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = upsertPoints
            },
            CancellationToken.None)).EnsureSuccess();

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            Q.Must(
                Q<TestPayload>.MatchFulltext(p => p.Text, "test")
            ),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: false,
            retryCount: 0);
        
        readPointsResult.Status.IsSuccess.Should().BeTrue();
        
        readPointsResult.Result.Points.Length.Should().Be(vectorCount);
    }

    [Test]
    public async Task FullText_PhraseMatch()
    {
        OnlyIfVersionAfterOrEqual(Version.Parse("1.15.0"), "Phrase search available since 1.15.0");

        var vectorSize = 10U;
        var vectorCount = 10;

        (await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClient.CreateFullTextPayloadIndex(
            TestCollectionName,
            "text",
        
            PayloadIndexedTextFieldTokenizerType.Word,
            CancellationToken.None,
            minimalTokenLength: 0,
            maximalTokenLength: 100,
        
            isLowercasePayloadTokens: false,
            isWaitForResult: true,
            onDisk: true,
            enablePhraseMatching: true)).EnsureSuccess();

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();

        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    CreateTestVector(vectorSize),
                    new TestPayload()
                    {
                        Text = $"text_{i} {i} test"
                    }
                )
            );
        }

        (await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = upsertPoints
            },
            CancellationToken.None)).EnsureSuccess();

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            Q.Must(
                // No points should match this filter without a phrase match enabled
                Q<TestPayload>.MatchFulltext(p => p.Text, "test 1", isPhraseMatch: true)
            ),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: false,
            retryCount: 0);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Points.Length.Should().Be(0);

        var readPointsResult2 = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            Q.Must(
                // Only one point should match this phrase
                Q<TestPayload>.MatchFulltext(p => p.Text, "1 test", isPhraseMatch: true)
            ),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: false,
            retryCount: 0);

        readPointsResult2.Status.IsSuccess.Should().BeTrue();
        readPointsResult2.Result.Points.Length.Should().Be(1);
    }
}
