using System.Text.Json.Nodes;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Helpers;
using Aer.QdrantClient.Tests.Model;
using Newtonsoft.Json.Linq;
using QdrantOperationStatus = Aer.QdrantClient.Http.Models.Shared.QdrantOperationStatus;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal partial class PointsCrudTests
{
    [Test]
    [TestCase(VectorDataType.Float32)]
    [TestCase(VectorDataType.Uint8)]
    [TestCase(VectorDataType.Float16)]
    public async Task UpsertPoint(VectorDataType vectorDataType)
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true,
                vectorDataType: vectorDataType)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var testPointId = PointId.NewGuid();

        var testVector = CreateTestVector(vectorSize, vectorDataType);

        TestPayload testPayload = "test";

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = new List<UpsertPointsRequest.UpsertPoint>()
                    {
                        new(testPointId, testVector, testPayload)
                    }
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoint(
            TestCollectionName,
            testPointId,
            CancellationToken.None);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Should().NotBeNull();

        readPointsResult.Result.Id.ObjectId.Should().Be(testPointId.ObjectId);
        readPointsResult.Result.Vector.Default.AsDenseVector().VectorValues
            .Should().BeEquivalentTo(testVector);
        var readTestPayload = readPointsResult.Result.Payload.As<TestPayload>();

        readTestPayload.Integer.Should().Be(testPayload.Integer);
        readTestPayload.FloatingPointNumber.Should().Be(testPayload.FloatingPointNumber);
        readTestPayload.Text.Should().Be(testPayload.Text);
    }

    [Test]
    [TestCase((int)1)]
    [TestCase((uint)1)]
    [TestCase((long)1)]
    [TestCase((ulong)1)]
    public async Task UpsertPoint_IntegerId(object pointIdIntegerTypeExample)
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

        ulong integerPointId = Type.GetTypeCode(pointIdIntegerTypeExample.GetType()) switch
        {
            TypeCode.Int32 => int.MaxValue - 10,
            TypeCode.UInt32 => uint.MaxValue - 10,
            TypeCode.Int64 => long.MaxValue - 10,
            TypeCode.UInt64 => ulong.MaxValue - 10,
            _ => throw new ArgumentOutOfRangeException()
        };

        var testPointId = PointId.Integer(integerPointId);

        var testVector = CreateTestVector(vectorSize);

        TestPayload testPayload = "test";

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = new List<UpsertPointsRequest.UpsertPoint>()
                    {
                        new(testPointId, testVector, testPayload)
                    }
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoint(
            TestCollectionName,
            testPointId,
            CancellationToken.None);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Should().NotBeNull();

        readPointsResult.Result.Id.ObjectId.Should().Be(testPointId.ObjectId);
        readPointsResult.Result.Vector.Default.AsDenseVector().VectorValues
            .Should().BeEquivalentTo(testVector);
        var readTestPayload = readPointsResult.Result.Payload.As<TestPayload>();

        readTestPayload.Integer.Should().Be(testPayload.Integer);
        readTestPayload.FloatingPointNumber.Should().Be(testPayload.FloatingPointNumber);
        readTestPayload.Text.Should().Be(testPayload.Text);
    }

    [Test]
    public async Task UpsertPoint_JsonObjectPayload()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var testPointId = PointId.NewGuid();
        var testVector = CreateTestVector(vectorSize);

        JsonObject testPayload = new JsonObject()
        {
            {"test", 1},
            {"test_2", "some_string"},
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = new List<UpsertPointsRequest.UpsertPoint>()
                    {
                        new(testPointId, testVector, testPayload)
                    }
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoint(
            TestCollectionName,
            testPointId,
            CancellationToken.None);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Should().NotBeNull();

        readPointsResult.Result.Id.ObjectId.Should().Be(testPointId.ObjectId);
        readPointsResult.Result.Vector.Default.AsDenseVector().VectorValues
            .Should().BeEquivalentTo(testVector);
        var readTestPayload = readPointsResult.Result.Payload.As<JsonObject>();

        readTestPayload.Should().NotBeNull();

        readTestPayload["test"]?.GetValue<int>().Should().Be(1);
        readTestPayload["test_2"]?.GetValue<string>().Should().Be("some_string");
    }

    [Test]
    public async Task UpsertPoint_JObjectPayload()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var testPointId = PointId.NewGuid();
        var testVector = CreateTestVector(vectorSize);

        JObject testPayload = new JObject()
        {
            {"test", 1},
            {"test_2", "some_string"},
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = new List<UpsertPointsRequest.UpsertPoint>()
                    {
                        new(testPointId, testVector, testPayload)
                    }
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoint(
            TestCollectionName,
            testPointId,
            CancellationToken.None);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Should().NotBeNull();

        readPointsResult.Result.Id.ObjectId.Should().Be(testPointId.ObjectId);
        readPointsResult.Result.Vector.Default.AsDenseVector().VectorValues
            .Should().BeEquivalentTo(testVector);
        var readTestPayload = readPointsResult.Result.GetTypedPayload().As<JsonObject>();

        readTestPayload.Should().NotBeNull();

        readTestPayload["test"]?.GetValue<int>().Should().Be(1);
        readTestPayload["test_2"]?.GetValue<string>().Should().Be("some_string");
    }

    [Test]
    public async Task UpsertPoint_UnknownPayloadType()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var testPointId = PointId.NewGuid();
        var testVector = CreateTestVector(vectorSize);

        JsonObject testPayload = new JsonObject()
        {
            ["test_property_1"] = "test_value",
            ["test_property_2"] = 1567,
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = new List<UpsertPointsRequest.UpsertPoint>()
                    {
                        new(testPointId, testVector, testPayload)
                    }
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoint(
            TestCollectionName,
            testPointId,
            CancellationToken.None);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Should().NotBeNull();

        readPointsResult.Result.Id.ObjectId.Should().Be(testPointId.ObjectId);
        readPointsResult.Result.Vector.Default.AsDenseVector().VectorValues
            .Should().BeEquivalentTo(testVector);

        var readTestPayload = readPointsResult.Result.Payload.As<JsonObject>();

        readTestPayload["test_property_1"]!.GetValue<string>().Should().Be("test_value");
        readTestPayload["test_property_2"]!.GetValue<int>().Should().Be(1567);
    }

    [Test]
    public async Task UpsertPoint_NullPayload()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var testPointId = PointId.NewGuid();
        var testVector = CreateTestVector(vectorSize);

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = new List<UpsertPointsRequest.UpsertPoint>()
                    {
                        new(testPointId, testVector, null)
                    }
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoint(
            TestCollectionName,
            testPointId,
            CancellationToken.None);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Should().NotBeNull();

        readPointsResult.Result.Id.ToString().Should().Be(testPointId.ToString());
        readPointsResult.Result.Vector.Default.AsDenseVector().VectorValues
            .Should().BeEquivalentTo(testVector);

        readPointsResult.Result.Payload.Should().Be(Payload.EmptyString);
        readPointsResult.Result.GetTypedPayload().IsEmpty.Should().BeTrue();
    }

    [Test]
    public async Task UpsertPoint_GetWithoutVectorAndPayload()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var testPointId = PointId.NewGuid();
        var testVector = CreateTestVector(vectorSize);
        TestPayload testPayload = "test";

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = new List<UpsertPointsRequest.UpsertPoint>()
                    {
                        new(testPointId, testVector, testPayload)
                    }
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointId.YieldSingle(),
            PayloadPropertiesSelector.None,
            CancellationToken.None,
            withVector: false);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(1);

        readPointsResult.Result[0].Id.ObjectId.Should().Be(testPointId.ObjectId);
        readPointsResult.Result[0].Vector.Should().BeNull();
        readPointsResult.Result[0].Payload.Should().BeNull();
    }

    [Test]
    public async Task UpsertPoint_GetWithPayloadSelector()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var testPointId = PointId.NewGuid();
        var testVector = CreateTestVector(vectorSize);

        TestPayload testPayload = "test";
        testPayload.Integer = 1567;
        testPayload.FloatingPointNumber = 15.67;
        testPayload.DateTimeValue = DateTime.Parse("2020-01-01T00:00:00");

        await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest()
            {
                Points = new List<UpsertPointsRequest.UpsertPoint>()
                {
                    new(testPointId, testVector, testPayload)
                }
            },
            CancellationToken.None);

        // some payload properties included

        var readPointIncludePayloadPropertyResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointId.YieldSingle(),
            PayloadPropertiesSelector.Include("integer"),
            CancellationToken.None,
            withVector: false);

        readPointIncludePayloadPropertyResult.Status.IsSuccess.Should().BeTrue();
        readPointIncludePayloadPropertyResult.Result.Length.Should().Be(1);

        readPointIncludePayloadPropertyResult.Result[0].Payload.Should().NotBeNull();

        var payloadWithIncludedProperties = readPointIncludePayloadPropertyResult.Result[0].GetTypedPayload().As<TestPayload>();

        payloadWithIncludedProperties.Integer.HasValue.Should().BeTrue();
        payloadWithIncludedProperties.FloatingPointNumber.HasValue.Should().BeFalse();
        payloadWithIncludedProperties.Text.Should().BeNull();
        payloadWithIncludedProperties.DateTimeValue.Should().BeNull();

        // some payload properties excluded

        var readPointExcludePayloadPropertyResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointId.YieldSingle(),
            PayloadPropertiesSelector.Exclude("integer"),
            CancellationToken.None,
            withVector: false);

        readPointExcludePayloadPropertyResult.Status.IsSuccess.Should().BeTrue();
        readPointExcludePayloadPropertyResult.Result.Length.Should().Be(1);

        readPointExcludePayloadPropertyResult.Result[0].Payload.Should().NotBeNull();

        var payloadWithExcludedProperties = readPointExcludePayloadPropertyResult.Result[0].GetTypedPayload().As<TestPayload>();

        payloadWithExcludedProperties.Integer.HasValue.Should().BeFalse();
        payloadWithExcludedProperties.FloatingPointNumber.HasValue.Should().BeTrue();
        payloadWithExcludedProperties.Text.Should().NotBeNull();
        payloadWithExcludedProperties.DateTimeValue.Should().NotBeNull();

        // all payload properties selected (explicit PayloadSelector)

        var readPointAllPayloadPropertiesResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointId.YieldSingle(),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: false);

        readPointAllPayloadPropertiesResult.Status.IsSuccess.Should().BeTrue();
        readPointAllPayloadPropertiesResult.Result.Length.Should().Be(1);

        readPointAllPayloadPropertiesResult.Result[0].Payload.Should().NotBeNull();
        var payloadWithAllProperties = readPointAllPayloadPropertiesResult.Result[0].GetTypedPayload().As<TestPayload>();

        payloadWithAllProperties.AllPropertiesNotNull().Should().BeTrue();

        // all payload properties selected (implicit PayloadSelector)

        var readPointAllPayloadPropertiesImplicitResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointId.YieldSingle(),
            withPayload: true,
            CancellationToken.None,
            withVector: false);

        readPointAllPayloadPropertiesImplicitResult.Status.IsSuccess.Should().BeTrue();
        readPointAllPayloadPropertiesImplicitResult.Result.Length.Should().Be(1);

        readPointAllPayloadPropertiesImplicitResult.Result[0].Payload.Should().NotBeNull();

        var payloadWithAllProperties2 = readPointAllPayloadPropertiesResult.Result[0].GetTypedPayload().As<TestPayload>();

        payloadWithAllProperties2.AllPropertiesNotNull().Should().BeTrue();

        // no payload properties selected (explicit PayloadSelector)

        var readPointNoPayloadPropertiesResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointId.YieldSingle(),
            PayloadPropertiesSelector.None,
            CancellationToken.None,
            withVector: false);

        readPointNoPayloadPropertiesResult.Status.IsSuccess.Should().BeTrue();
        readPointNoPayloadPropertiesResult.Result.Length.Should().Be(1);

        readPointNoPayloadPropertiesResult.Result[0].Payload.Should().BeNull();

        // no payload properties selected (implicit PayloadSelector)

        var readPointNoPayloadPropertiesImplicitResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointId.YieldSingle(),
            withPayload: false,
            CancellationToken.None,
            withVector: false);

        readPointNoPayloadPropertiesImplicitResult.Status.IsSuccess.Should().BeTrue();
        readPointNoPayloadPropertiesImplicitResult.Result.Length.Should().Be(1);

        readPointNoPayloadPropertiesImplicitResult.Result[0].Payload.Should().BeNull();
    }

    [Test]
    public async Task UpsertPoint_AsyncOperation()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = new List<UpsertPointsRequest.UpsertPoint>()
                    {
                        new(PointId.NewGuid(), CreateTestVector(vectorSize), "test")
                    }
                },
                CancellationToken.None,
                isWaitForResult: false);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Acknowledged);
    }

    [Test]
    public async Task UpsertPoints()
    {
        var vectorSize = 10U;
        var vectorCount = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    CreateTestVector(vectorSize),
                    i
                )
            );
        }

        Dictionary<ulong, UpsertPointsRequest.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId) p.Id).Id);

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None,
                isWaitForResult: true,
                ordering: OrderingType.Strong);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointsByPointIds.Keys.Select(PointId.Integer),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true,
            retryCount: 0);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);
        
        // Upsert operations do not report statistics
        if (!IsCiEnvironment)
        {
            // CI environment container does not have usage statistics enabled
            upsertPointsResult.Usage.PayloadIoWrite.Should().NotBe(0);
            upsertPointsResult.Usage.VectorIoWrite.Should().NotBe(0);
        }
        else
        {
            upsertPointsResult.Usage.Should().BeNull();
        }

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(vectorCount);

        if (!IsCiEnvironment)
        {
            // CI environment container does not have usage statistics enabled
            readPointsResult.Usage.PayloadIoRead.Should().BeGreaterThan(0);
            readPointsResult.Usage.VectorIoRead.Should().BeGreaterThan(0);
        }
        else
        {
            readPointsResult.Usage.Should().BeNull();
        }

        foreach (var readPoint in readPointsResult.Result)
        {
            var readPointId = readPoint.Id.As<IntegerPointId>().Id;

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            var readPointPayload = readPoint.GetTypedPayload().As<TestPayload>();

            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();
            
            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);
        }
    }

    [Test]
    public async Task UpsertPoints_ByteVector()
    {
        var vectorSize = 10U;
        var vectorCount = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true,
                vectorDataType: VectorDataType.Uint8)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    CreateTestVector(vectorSize, VectorDataType.Uint8),
                    i
                )
            );
        }

        Dictionary<ulong, UpsertPointsRequest.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId) p.Id).Id);

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointsByPointIds.Keys.Select(PointId.Integer),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(vectorCount);

        foreach (var readPoint in readPointsResult.Result)
        {
            var readPointId = readPoint.Id.As<IntegerPointId>().Id;

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Vector.Default.Should().BeEquivalentTo(expectedPoint.Vector.Default);

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            var readPointPayload = readPoint.GetTypedPayload().As<TestPayload>();
            
            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);
        }
    }

    [Test]
    public async Task UpsertPoints_NamedVectors_SameConfig()
    {
        var vectorSize = 10U;
        var vectorCount = 10;
        var namedVectorsCount = 3;

        var vectorNames = CreateVectorNames(namedVectorsCount);

        var createCollectionResponse = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true,
                namedVectorNames: vectorNames)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        createCollectionResponse.Status.IsSuccess.Should().BeTrue();

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    CreateTestNamedVectors(vectorSize, namedVectorsCount),
                    i
                )
            );
        }

        Dictionary<ulong, UpsertPointsRequest.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId) p.Id).Id);

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointsByPointIds.Keys.Select(PointId.Integer),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(vectorCount);

        foreach (var readPoint in readPointsResult.Result)
        {
            var readPointId = readPoint.Id.As<IntegerPointId>().Id;

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            var readPointPayload = readPoint.GetTypedPayload().As<TestPayload>();

            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();
            
            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);

            readPoint.Vector.VectorKind.Should().Be(VectorKind.Named);
            var namedPointVectors = readPoint.Vector.AsNamedVectors();

            namedPointVectors.Vectors.Count.Should().Be(vectorNames.Count);

            foreach (var vectorName in vectorNames)
            {
                namedPointVectors[vectorName].AsDenseVector().VectorValues
                    .Should().NotBeNullOrEmpty();
            }
        }
    }

    [Test]
    public async Task UpsertPoints_NamedVectors_VectorSelector()
    {
        var vectorSize = 10U;
        var vectorCount = 10;
        var namedVectorsCount = 3;

        var vectorNames = CreateVectorNames(namedVectorsCount);

        var includedVectorNames = vectorNames.Take(2).ToHashSet();

        var createCollectionResponse = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true,
                namedVectorNames: vectorNames)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        createCollectionResponse.Status.IsSuccess.Should().BeTrue();

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    CreateTestNamedVectors(vectorSize, namedVectorsCount),
                    i
                )
            );
        }

        Dictionary<ulong, UpsertPointsRequest.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId) p.Id).Id);

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointsByPointIds.Keys.Select(PointId.Integer),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: VectorSelector.Include(includedVectorNames));

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(vectorCount);

        foreach (var readPoint in readPointsResult.Result)
        {
            var readPointId = readPoint.Id.As<IntegerPointId>().Id;

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();
            
            var readPointPayload = readPoint.GetTypedPayload().As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);

            readPoint.Vector.VectorKind.Should().Be(VectorKind.Named);
            var namedPointVectors = readPoint.Vector.AsNamedVectors();

            namedPointVectors.Vectors.Count.Should().Be(includedVectorNames.Count);

            foreach (var vectorName in vectorNames)
            {
                if (includedVectorNames.Contains(vectorName))
                {
                    namedPointVectors.Vectors.ContainsKey(vectorName).Should().BeTrue();

                    namedPointVectors[vectorName].AsDenseVector().VectorValues
                        .Should().NotBeNullOrEmpty();
                }
                else
                {
                    namedPointVectors.Vectors.ContainsKey(vectorName).Should().BeFalse();
                }
            }
        }
    }

    [Test]
    public async Task UpsertPoints_NamedVectors_DifferentConfig()
    {
        var cosineDistanceVectorName = "Vector_3";

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
            [cosineDistanceVectorName] = new VectorConfigurationBase.SingleVectorConfiguration(
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

        UpsertPointsRequest.UpsertPoint firstPoint =
            new(
                id: 1,
                vector : new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        // here and further on float[] will be implicitly converted to Vector
                        ["Vector_1"] = CreateTestVector(100U),
                        ["Vector_2"] = CreateTestVector(5U),
                        ["Vector_3"] = CreateTestVector(50U),
                    }
                },
                payload : 1);

        UpsertPointsRequest.UpsertPoint secondPoint = new(
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

        UpsertPointsRequest.UpsertPoint thirdPoint = new(
            id: 3,
            vector: new NamedVectors()
            {
                Vectors = new Dictionary<string, VectorBase>()
                {
                    ["Vector_1"] = CreateTestVector(100U),
                }
            },
            payload: 3);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>(){
            firstPoint,
            secondPoint,
            thirdPoint
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPoints.Select(p=>p.Id),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(upsertPoints.Count);

        foreach (var upsertPoint in upsertPoints)
        {
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(upsertPoint.Id));

            var readPointVectors = readPoint.Vector.AsNamedVectors();

            foreach (var upsertPointVector in upsertPoint.Vector.AsNamedVectors().Vectors)
            {
                readPointVectors.ContainsVector(upsertPointVector.Key).Should().BeTrue();

                if (upsertPointVector.Key == cosineDistanceVectorName)
                {
                    // according to documentation https://qdrant.tech/documentation/concepts/collections/
                    // "For search efficiency, Cosine similarity is implemented as dot-product over normalized vectors.
                    // Vectors are automatically normalized during upload"
                    // thus there is no point of comparing upserted and read vector - they will always be different!
                    continue;
                }

                var readPointNamedVectorValue = readPointVectors[upsertPointVector.Key].AsDenseVector().VectorValues;
                var upsertPointNamedVectorValue = upsertPointVector.Value.AsDenseVector().VectorValues;

                readPointNamedVectorValue.Should().Equal(upsertPointNamedVectorValue);
            }
        }
    }

    [Test]
    public async Task UpsertPoints_NamedVectors_DifferentConfig_DefaultVector()
    {
        HashSet<ulong> pointIdsWithDefaultVector = [1, 2];
        Dictionary<string, VectorConfigurationBase.SingleVectorConfiguration> namedVectors = new()
        {
            [VectorBase.DefaultVectorName] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Dot,
                100,
                isServeVectorsFromDisk: true,
                vectorDataType: VectorDataType.Uint8),
            ["Vector_2"] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Euclid,
                5,
                isServeVectorsFromDisk: false)
        };

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(namedVectors)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        UpsertPointsRequest.UpsertPoint firstPoint =
            new(
                id: 1,
                vector : new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        // here and further on float[] will be implicitly converted to Vector
                        [VectorBase.DefaultVectorName] = CreateTestVector(100U, VectorDataType.Uint8),
                        ["Vector_2"] = CreateTestVector(5U),
                    }
                },
                payload : 1);

        UpsertPointsRequest.UpsertPoint secondPoint = new(
            id: 2,
            vector: new NamedVectors()
            {
                Vectors = new Dictionary<string, VectorBase>()
                {
                    [VectorBase.DefaultVectorName] = CreateTestVector(100, VectorDataType.Uint8),
                }
            },
            payload: 2);

        UpsertPointsRequest.UpsertPoint thirdPoint = new(
            id: 3,
            vector: new NamedVectors()
            {
                Vectors = new Dictionary<string, VectorBase>()
                {
                    ["Vector_2"] = CreateTestVector(5U),
                }
            },
            payload: 3);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>(){
            firstPoint,
            secondPoint,
            thirdPoint
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPoints.Select(p=>p.Id),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(upsertPoints.Count);

        foreach (var upsertPoint in upsertPoints)
        {
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(upsertPoint.Id));

            var readPointVectors = readPoint.Vector.AsNamedVectors();

            foreach (var upsertPointVector in upsertPoint.Vector.AsNamedVectors().Vectors)
            {
                readPointVectors.ContainsVector(upsertPointVector.Key).Should().BeTrue();

                var readPointNamedVectorValue = readPointVectors[upsertPointVector.Key].AsDenseVector().VectorValues;
                var upsertPointNamedVectorValue = upsertPointVector.Value.AsDenseVector().VectorValues;

                readPointNamedVectorValue.Should().Equal(upsertPointNamedVectorValue);
            }

            var getDefaultVectorAct = () => readPoint.Vector.Default;

            if (!pointIdsWithDefaultVector.Contains(readPoint.Id.AsInteger()))
            {
                getDefaultVectorAct.Should().Throw<QdrantDefaultVectorNotFoundException>();
            }
            else
            {
                getDefaultVectorAct.Should().NotThrow();
            }
        }
    }

    [Test]
    public async Task UpsertPoints_SparseVectors_OnlySparse()
    {
        Dictionary<string, SparseVectorConfiguration> sparseVectors = new()
        {
            ["Vector_1"] = new(true, fullScanThreshold: 1000),
            ["Vector_2"] = new(), // default sparse vector configuration
        };

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(sparseVectorsConfiguration: sparseVectors)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        UpsertPointsRequest.UpsertPoint firstPoint =
            new(
                id: 1,
                vector : new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        ["Vector_1"] = CreateTestSparseVector(100U, 4),
                        ["Vector_2"] = CreateTestSparseVector(50U, 5),
                    }
                },
                payload : 1);

        UpsertPointsRequest.UpsertPoint secondPoint = new(
            id: 2,
            vector: new NamedVectors()
            {
                Vectors = new Dictionary<string, VectorBase>()
                {
                    ["Vector_1"] = CreateTestSparseVector(100U, 6),
                    ["Vector_2"] = CreateTestSparseVector(50U, 7),
                }
            },
            payload: 2);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>(){
            firstPoint,
            secondPoint,
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPoints.Select(p=>p.Id),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(upsertPoints.Count);

        foreach (var upsertPoint in upsertPoints)
        {
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(upsertPoint.Id));

            var readPointVectors = readPoint.Vector.AsNamedVectors();

            foreach (var upsertPointVector in upsertPoint.Vector.AsNamedVectors().Vectors)
            {
                readPointVectors.ContainsVector(upsertPointVector.Key).Should().BeTrue();

                var readPointSparseVectorValue = readPointVectors[upsertPointVector.Key].AsSparseVector();
                var upsertPointSparseVectorValue = upsertPointVector.Value.AsSparseVector();

                readPointSparseVectorValue.Indices.Should().Equal(upsertPointSparseVectorValue.Indices);
                readPointSparseVectorValue.Values.Should().Equal(upsertPointSparseVectorValue.Values);
            }
        }
    }

    [Test]
    public async Task UpsertPoints_SparseVectors_MixedSparseAndNamed()
    {
        Dictionary<string, VectorConfigurationBase.SingleVectorConfiguration> namedVectors = new()
        {
            ["Vector_1"] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Manhattan,
                100,
                isServeVectorsFromDisk: true)
        };

        Dictionary<string, SparseVectorConfiguration> sparseVectors = new()
        {
            ["Vector_2"] = new()
            {
                Index = new()
                {
                    OnDisk = true,
                    FullScanThreshold = 1000
                }
            }
        };

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                namedVectorsConfiguration: namedVectors,
                sparseVectorsConfiguration: sparseVectors)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        UpsertPointsRequest.UpsertPoint firstPoint =
            new(
                id: 1,
                vector : new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        ["Vector_1"] = CreateTestVector(100U),
                        ["Vector_2"] = CreateTestSparseVector(50U, 5),
                    }
                },
                payload : 1);

        UpsertPointsRequest.UpsertPoint secondPoint = new(
            id: 2,
            vector: new NamedVectors()
            {
                Vectors = new Dictionary<string, VectorBase>()
                {
                    ["Vector_1"] = CreateTestVector(100U),
                    ["Vector_2"] = CreateTestSparseVector(50U, 7),
                }
            },
            payload: 2);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>(){
            firstPoint,
            secondPoint,
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPoints.Select(p=>p.Id),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(upsertPoints.Count);

        foreach (var upsertPoint in upsertPoints)
        {
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(upsertPoint.Id));

            var readPointVectors = readPoint.Vector.AsNamedVectors();

            foreach (var upsertPointVector in upsertPoint.Vector.AsNamedVectors().Vectors)
            {
                readPointVectors.ContainsVector(upsertPointVector.Key).Should().BeTrue();

                if (readPointVectors[upsertPointVector.Key].VectorKind != VectorKind.Sparse)
                {
                    var readPointNamedVectorValue = readPointVectors[upsertPointVector.Key].AsDenseVector().VectorValues;
                    var upsertPointNamedVectorValue = upsertPointVector.Value.AsDenseVector().VectorValues;

                    readPointNamedVectorValue.Should().Equal(upsertPointNamedVectorValue);
                }
                else
                {
                    // means we are checking sparse vector
                    var readPointSparseVectorValue = readPointVectors[upsertPointVector.Key].AsSparseVector();
                    var upsertPointSparseVectorValue = upsertPointVector.Value.AsSparseVector();

                    readPointSparseVectorValue.Indices.Should().Equal(upsertPointSparseVectorValue.Indices);
                    readPointSparseVectorValue.Values.Should().Equal(upsertPointSparseVectorValue.Values);
                }
            }
        }
    }

    [Test]
    public async Task UpsertPoints_MultiVectors_OnlyMulti()
    {
        uint vectorLength = 10;

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorLength,
                isServeVectorsFromDisk: true,
                multivectorConfiguration: new(MultivectorComparator.MaxSim))
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        UpsertPointsRequest.UpsertPoint firstPoint =
            new(
                id: 1,
                vector: new MultiVector()
                {
                    Vectors = CreateTestMultivector(vectorLength, 2, VectorDataType.Float32)
                },
                payload: 1);

        UpsertPointsRequest.UpsertPoint secondPoint = new(
            id: 2,
            vector: new MultiVector()
            {
                Vectors = CreateTestMultivector(vectorLength, 3, VectorDataType.Float32)
            },
            payload: 2);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>(){
            firstPoint,
            secondPoint,
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPoints.Select(p=>p.Id),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(upsertPoints.Count);

        foreach (var upsertPoint in upsertPoints)
        {
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(upsertPoint.Id));

            readPoint.Vector.VectorKind.Should().Be(VectorKind.Multi);

            var readPointVectors = readPoint.Vector.AsMultiVector();

            readPointVectors.Vectors.Should().BeEquivalentTo(upsertPoint.Vector.AsMultiVector().Vectors);
        }
    }

    [Test]
    public async Task UpsertPoints_MultiVectors_NamedVectors_MixedSingleSparseAndMulti()
    {
        Dictionary<string, VectorConfigurationBase.SingleVectorConfiguration> namedVectors = new()
        {
            ["Vector_1"] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Manhattan,
                100,
                isServeVectorsFromDisk: true),

            ["Vector_3"] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Manhattan,
                20,
                isServeVectorsFromDisk: true,
                multivectorConfiguration: new MultivectorConfiguration(MultivectorComparator.MaxSim))
        };

        Dictionary<string, SparseVectorConfiguration> sparseVectors = new()
        {
            ["Vector_2"] = new(true, fullScanThreshold: 1000)
        };

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                namedVectorsConfiguration: namedVectors,
                sparseVectorsConfiguration: sparseVectors)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        UpsertPointsRequest.UpsertPoint firstPoint =
            new(
                id: 1,
                vector : new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        ["Vector_1"] = CreateTestVector(100U),
                        ["Vector_2"] = CreateTestSparseVector(50U, 5),
                        ["Vector_3"] = CreateTestMultivector(20, 5, VectorDataType.Float32),
                    }
                },
                payload : 1);

        UpsertPointsRequest.UpsertPoint secondPoint = new(
            id: 2,
            vector: new NamedVectors()
            {
                Vectors = new Dictionary<string, VectorBase>()
                {
                    ["Vector_1"] = CreateTestVector(100U),
                    ["Vector_2"] = CreateTestSparseVector(50U, 7),
                    ["Vector_3"] = CreateTestMultivector(20, 10, VectorDataType.Float32),
                }
            },
            payload: 2);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>(){
            firstPoint,
            secondPoint,
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPoints.Select(p=>p.Id),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(upsertPoints.Count);

        foreach (var upsertPoint in upsertPoints)
        {
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(upsertPoint.Id));

            var readPointVectors = readPoint.Vector.AsNamedVectors();

            foreach (var upsertPointVector
                     in upsertPoint.Vector.AsNamedVectors().Vectors)
            {
                readPointVectors.ContainsVector(upsertPointVector.Key).Should().BeTrue();

                var readPointVectorKind = readPointVectors[upsertPointVector.Key].VectorKind;

                if (readPointVectorKind == VectorKind.Sparse)
                {
                    // means we are checking sparse vector
                    var readPointSparseVectorValue = readPointVectors[upsertPointVector.Key].AsSparseVector();
                    var upsertPointSparseVectorValue = upsertPointVector.Value.AsSparseVector();

                    readPointSparseVectorValue.Indices.Should().Equal(upsertPointSparseVectorValue.Indices);
                    readPointSparseVectorValue.Values.Should().Equal(upsertPointSparseVectorValue.Values);
                }
                else if (readPointVectorKind == VectorKind.Dense)
                {
                    var readPointNamedVectorValue = readPointVectors[upsertPointVector.Key].AsDenseVector().VectorValues;
                    var upsertPointNamedVectorValue = upsertPointVector.Value.AsDenseVector().VectorValues;

                    readPointNamedVectorValue.Should().Equal(upsertPointNamedVectorValue);
                }
                else if(readPointVectorKind == VectorKind.Multi)
                {
                    var readPointNamedMultiVectorValue = readPointVectors[upsertPointVector.Key].AsMultiVector();

                    readPointNamedMultiVectorValue.Vectors.Should()
                        .BeEquivalentTo(upsertPointVector.Value.AsMultiVector().Vectors);
                }
                else
                {
                    Assert.Fail($"Unexpected vector kind {readPointVectors[upsertPointVector.Key].VectorKind}");
                }
            }
        }
    }

    [Test]
    public async Task UpsertPoints_MultiVectors_MixedMultiAndSimple()
    {
        uint vectorLength = 10;

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorLength,
                isServeVectorsFromDisk: true,
                multivectorConfiguration: new(MultivectorComparator.MaxSim),
                vectorDataType: VectorDataType.Float16)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        UpsertPointsRequest.UpsertPoint firstPoint =
            new(
                id: 1,
                vector: new MultiVector()
                {
                    Vectors = CreateTestMultivector(vectorLength, 2, VectorDataType.Float16)
                },
                payload: 1);

        UpsertPointsRequest.UpsertPoint secondPoint = new(
            id: 2,
            vector: new MultiVector()
            {
                Vectors = CreateTestMultivector(vectorLength, 3, VectorDataType.Float16)
            },
            payload: 2);

        // when we have a collection configured for multivectors single vectors get
        // automatically converted to multivectors

        UpsertPointsRequest.UpsertPoint thirdPoint = new(
            id: 3,
            vector: new DenseVector(){
                VectorValues = CreateTestVector(vectorLength, VectorDataType.Float16)
            },
            payload: 3);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>()
        {
            firstPoint,
            secondPoint,
            thirdPoint
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPoints.Select(p => p.Id),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(upsertPoints.Count);

        foreach (var upsertPoint in upsertPoints)
        {
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(upsertPoint.Id));

            if (upsertPoint.Vector.VectorKind == VectorKind.Multi)
            {
                readPoint.Vector.VectorKind.Should().Be(VectorKind.Multi);

                var readPointVector = readPoint.Vector.AsMultiVector();

                readPointVector.Vectors.Should().BeEquivalentTo(upsertPoint.Vector.AsMultiVector().Vectors);
            }
            else if (upsertPoint.Vector.VectorKind == VectorKind.Dense)
            {
                // readPoint is read as single component vector multivector (!) so compare Default vector
                readPoint.Vector.VectorKind.Should().Be(VectorKind.Multi);
                var readPointVector = readPoint.Vector.AsMultiVector();

                readPointVector.Default.AsDenseVector().VectorValues
                    .Should().BeEquivalentTo(upsertPoint.Vector.AsDenseVector().VectorValues);
            }
            else
            {
                Assert.Fail($"Unexpected vector kind {upsertPoint.Vector.VectorKind}");
            }
        }
    }

    [Test]
    public async Task DeletePoints()
    {
        var vectorCount = 10;
        var pointsToDeleteCount = 5;

        var (_, pointsByPointIds, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: vectorCount);

        var pointsToDelete = pointsByPointIds.Values.Take(pointsToDeleteCount).ToList();

        List<PointId> pointIdsToDelete = pointsToDelete.Select(p => p.Id).ToList();

        var deletePointsByPointIdResult = await _qdrantHttpClient.DeletePoints(
            TestCollectionName,
            pointIdsToDelete.Take(2),
            CancellationToken.None);

        deletePointsByPointIdResult.Status.IsSuccess.Should().BeTrue();
        deletePointsByPointIdResult.Result.Status.Should().Be(QdrantOperationStatus.Completed);
        
        var deletePointsByFilterResult = await _qdrantHttpClient.DeletePoints(
            TestCollectionName,
            // We use only the id filter for the test purposes, but we can actually use any filter
            Q.HaveAnyId(pointIdsToDelete.Skip(2)),
            CancellationToken.None);

        deletePointsByFilterResult.Status.IsSuccess.Should().BeTrue();
        deletePointsByFilterResult.Result.Status.Should().Be(QdrantOperationStatus.Completed);
        
        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            pointsByPointIds.Keys.Select(PointId.Integer),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(vectorCount - pointsToDeleteCount);

        foreach (var readPoint in readPointsResult.Result)
        {
            var readPointId = readPoint.Id.As<IntegerPointId>().Id;

            var expectedPoint = pointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);
            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();

            var readPointPayload = readPoint.GetTypedPayload().As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);
        }
    }
}
