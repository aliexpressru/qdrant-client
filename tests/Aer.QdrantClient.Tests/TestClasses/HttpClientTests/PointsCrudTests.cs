using System.Text.Json.Nodes;
using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Helpers;
using Aer.QdrantClient.Tests.Model;
using Newtonsoft.Json.Linq;
using QdrantOperationStatus = Aer.QdrantClient.Http.Models.Shared.QdrantOperationStatus;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal class PointsCrudTests : QdrantTestsBase
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

    #region Non existent entities and invalid cases tests

    [Test]
    public async Task GetPoint_CollectionDoesNotExist()
    {
        var getPointFromNonexistentCollectionResult
            = await _qdrantHttpClient.GetPoint(TestCollectionName, PointId.Integer(1), CancellationToken.None);

        getPointFromNonexistentCollectionResult.Status.IsSuccess.Should().BeFalse();
        getPointFromNonexistentCollectionResult.Status.Error.Should()
            .Contain(TestCollectionName)
            .And.Contain("doesn't exist");
    }

    [Test]
    public async Task GetPoint_EnsureSuccess_CollectionDoesNotExist()
    {
        var getPointFromNonexistentCollectionResult
            = await _qdrantHttpClient.GetPoint(TestCollectionName, PointId.Integer(1), CancellationToken.None);

        var act = () => getPointFromNonexistentCollectionResult.EnsureSuccess();

        act.Should().Throw<QdrantUnsuccessfulResponseStatusException>()
            .Where(e => e.Message.Contains("doesn't exist"));
    }

    [Test]
    public async Task GetPoint_PointDoesNotExist_NumberId()
    {
        var nonexistentPointId = PointId.Integer(1);

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var getNonexistentPointResult
            = await _qdrantHttpClient.GetPoint(TestCollectionName, nonexistentPointId, CancellationToken.None);

        getNonexistentPointResult.Status.IsSuccess.Should().BeFalse();
        getNonexistentPointResult.Status.Error.Should()
            .Contain("Not found")
            .And.Contain(nonexistentPointId.ToString())
            .And.Contain("does not exists");

        getNonexistentPointResult.Result.Should().BeNull();
    }

    [Test]
    public async Task GetPoint_PointDoesNotExist_GuidId()
    {
        var nonexistentPointId = PointId.Guid(Guid.NewGuid());

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var getNonexistentPointResult
            = await _qdrantHttpClient.GetPoint(TestCollectionName, nonexistentPointId, CancellationToken.None);

        getNonexistentPointResult.Status.IsSuccess.Should().BeFalse();
        getNonexistentPointResult.Status.Error.Should()
            .Contain("Not found")
            .And.Contain(nonexistentPointId.ToString())
            .And.Contain("does not exists");

        getNonexistentPointResult.Result.Should().BeNull();
    }

    [Test]
    public async Task GetPoints_CollectionDoesNotExist()
    {
        var getPointFromNonexistentCollectionResult
            = await _qdrantHttpClient.GetPoints(
                TestCollectionName,
                PointId.Integer(1).YieldSingle(),
                PayloadPropertiesSelector.None,
                CancellationToken.None);

        getPointFromNonexistentCollectionResult.Status.IsSuccess.Should().BeFalse();
        getPointFromNonexistentCollectionResult.Status.Error.Should()
            .Contain(TestCollectionName)
            .And.Contain("doesn't exist");
    }

    [Test]
    public async Task GetPoints_PointDoesNotExist_NumberId()
    {
        var nonexistentPointId = PointId.Integer(1);

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var getNonexistentPointResult
            = await _qdrantHttpClient.GetPoints(
                TestCollectionName,
                nonexistentPointId.YieldSingle(),
                PayloadPropertiesSelector.None,
                CancellationToken.None);

        getNonexistentPointResult.Status.IsSuccess.Should().BeTrue();
        getNonexistentPointResult.Result.Length.Should().Be(0);
    }

    [Test]
    public async Task GetPoints_PointDoesNotExist_GuidId()
    {
        var nonexistentPointId = PointId.Guid(Guid.NewGuid());

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var getNonexistentPointResult
            = await _qdrantHttpClient.GetPoints(
                TestCollectionName,
                nonexistentPointId.YieldSingle(),
                PayloadPropertiesSelector.None,
                CancellationToken.None);

        getNonexistentPointResult.Status.IsSuccess.Should().BeTrue();
        getNonexistentPointResult.Result.Length.Should().Be(0);
    }

    [Test]
    public async Task UpsertPoint_CollectionDoesNotExist()
    {
        var upsertPointsToNonExistentCollectionResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
                    {
                        new(
                            PointId.Integer(1),
                            CreateTestVector(1),
                            "test"
                        )
                    }
                },
                CancellationToken.None);

        upsertPointsToNonExistentCollectionResult.Status.IsSuccess.Should().BeFalse();
        upsertPointsToNonExistentCollectionResult.Status.Error.Should()
            .Contain(TestCollectionName)
            .And.Contain("doesn't exist");
    }

    [Test]
    public async Task UpsertPoint_InvalidPayloadType()
    {
        var upsertPointsToNonExistentCollectionAct
            = async () => await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<string>()
                {
                    Points = new List<UpsertPointsRequest<string>.UpsertPoint>()
                    {
                        new(
                            PointId.Integer(1),
                            CreateTestVector(1),
                            "test"
                        )
                    }
                },
                CancellationToken.None);

        await upsertPointsToNonExistentCollectionAct.Should().ThrowAsync<QdrantInvalidPayloadTypeException>();
    }

    [Test]
    public async Task DeletePoints_CollectionDoesNotExist()
    {
        var getPointFromNonexistentCollectionResult
            = await _qdrantHttpClient.DeletePoints(
                TestCollectionName,
                PointId.Integer(1).YieldSingle(),
                CancellationToken.None);

        getPointFromNonexistentCollectionResult.Status.IsSuccess.Should().BeFalse();
        getPointFromNonexistentCollectionResult.Status.Error.Should()
            .Contain(TestCollectionName)
            .And.Contain("doesn't exist");
    }

    [Test]
    public async Task DeletePoints_PointDoesNotExist()
    {
        var nonexistentPointId = PointId.Integer(1);

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var getNonexistentPointResult
            = await _qdrantHttpClient.DeletePoints(
                TestCollectionName,
                nonexistentPointId.YieldSingle(),
                CancellationToken.None);

        getNonexistentPointResult.Status.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Upsert/Read/Delete operations tests

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
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
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
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
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
                new UpsertPointsRequest<object>()
                {
                    Points = new List<UpsertPointsRequest<object>.UpsertPoint>()
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
                new UpsertPointsRequest<object>()
                {
                    Points = new List<UpsertPointsRequest<object>.UpsertPoint>()
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
                new UpsertPointsRequest<object>()
                {
                    Points = new List<UpsertPointsRequest<object>.UpsertPoint>()
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
                new UpsertPointsRequest<object>()
                {
                    Points = new List<UpsertPointsRequest<object>.UpsertPoint>()
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

        readPointsResult.Result.Id.AsString().Should().Be(testPointId.AsString());
        readPointsResult.Result.Vector.Default.AsDenseVector().VectorValues
            .Should().BeEquivalentTo(testVector);
        readPointsResult.Result.Payload.Should().BeNull();
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
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
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
            new UpsertPointsRequest<TestPayload>()
            {
                Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
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

        var payloadWithIncludedProperties = readPointIncludePayloadPropertyResult.Result[0].Payload.As<TestPayload>();

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

        var payloadWithExcludedProperties = readPointExcludePayloadPropertyResult.Result[0].Payload.As<TestPayload>();

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
        var payloadWithAllProperties = readPointAllPayloadPropertiesResult.Result[0].Payload.As<TestPayload>();

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

        var payloadWithAllProperties2 = readPointAllPayloadPropertiesResult.Result[0].Payload.As<TestPayload>();

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
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
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

    [TestCase(0U)]
    [TestCase(3U)]
    public async Task UpsertPoints(uint readRetryCount)
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

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
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

        Dictionary<ulong, UpsertPointsRequest<TestPayload>.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId) p.Id).Id);

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
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
            retryCount: readRetryCount);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);
        
        // Upsert operations do not report statistics
        upsertPointsResult.Usage.PayloadIoWrite.Should().Be(0);
        upsertPointsResult.Usage.VectorIoWrite.Should().Be(0);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(vectorCount);
        
        readPointsResult.Usage.PayloadIoRead.Should().BeGreaterThan(0);
        readPointsResult.Usage.VectorIoRead.Should().BeGreaterThan(0);

        foreach (var readPoint in readPointsResult.Result)
        {
            var readPointId = readPoint.Id.As<IntegerPointId>().Id;

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPoint.Payload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPoint.Payload.Text);
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

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
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

        Dictionary<ulong, UpsertPointsRequest<TestPayload>.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId) p.Id).Id);

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
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

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPoint.Payload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPoint.Payload.Text);
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

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
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

        Dictionary<ulong, UpsertPointsRequest<TestPayload>.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId) p.Id).Id);

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
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

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPoint.Payload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPoint.Payload.Text);

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

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
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

        Dictionary<ulong, UpsertPointsRequest<TestPayload>.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId) p.Id).Id);

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
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

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPoint.Payload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPoint.Payload.Text);

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

        UpsertPointsRequest<TestPayload>.UpsertPoint firstPoint =
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

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>(){
            firstPoint,
            secondPoint,
            thirdPoint
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
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

        UpsertPointsRequest<TestPayload>.UpsertPoint firstPoint =
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

        UpsertPointsRequest<TestPayload>.UpsertPoint secondPoint = new(
            id: 2,
            vector: new NamedVectors()
            {
                Vectors = new Dictionary<string, VectorBase>()
                {
                    [VectorBase.DefaultVectorName] = CreateTestVector(100, VectorDataType.Uint8),
                }
            },
            payload: 2);

        UpsertPointsRequest<TestPayload>.UpsertPoint thirdPoint = new(
            id: 3,
            vector: new NamedVectors()
            {
                Vectors = new Dictionary<string, VectorBase>()
                {
                    ["Vector_2"] = CreateTestVector(5U),
                }
            },
            payload: 3);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>(){
            firstPoint,
            secondPoint,
            thirdPoint
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
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
            new CreateCollectionRequest(
                sparseVectorsConfiguration: sparseVectors)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        UpsertPointsRequest<TestPayload>.UpsertPoint firstPoint =
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

        UpsertPointsRequest<TestPayload>.UpsertPoint secondPoint = new(
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

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>(){
            firstPoint,
            secondPoint,
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
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

        UpsertPointsRequest<TestPayload>.UpsertPoint firstPoint =
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

        UpsertPointsRequest<TestPayload>.UpsertPoint secondPoint = new(
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

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>(){
            firstPoint,
            secondPoint,
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
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

        UpsertPointsRequest<TestPayload>.UpsertPoint firstPoint =
            new(
                id: 1,
                vector: new MultiVector()
                {
                    Vectors = CreateTestMultivector(vectorLength, 2, VectorDataType.Float32)
                },
                payload: 1);

        UpsertPointsRequest<TestPayload>.UpsertPoint secondPoint = new(
            id: 2,
            vector: new MultiVector()
            {
                Vectors = CreateTestMultivector(vectorLength, 3, VectorDataType.Float32)
            },
            payload: 2);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>(){
            firstPoint,
            secondPoint,
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
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

        UpsertPointsRequest<TestPayload>.UpsertPoint firstPoint =
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

        UpsertPointsRequest<TestPayload>.UpsertPoint secondPoint = new(
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

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>(){
            firstPoint,
            secondPoint,
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
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

        UpsertPointsRequest<TestPayload>.UpsertPoint firstPoint =
            new(
                id: 1,
                vector: new MultiVector()
                {
                    Vectors = CreateTestMultivector(vectorLength, 2, VectorDataType.Float16)
                },
                payload: 1);

        UpsertPointsRequest<TestPayload>.UpsertPoint secondPoint = new(
            id: 2,
            vector: new MultiVector()
            {
                Vectors = CreateTestMultivector(vectorLength, 3, VectorDataType.Float16)
            },
            payload: 2);

        // when we have a collection configured for multivectors single vectors get
        // automatically converted to multivectors

        UpsertPointsRequest<TestPayload>.UpsertPoint thirdPoint = new(
            id: 3,
            vector: new DenseVector(){
                VectorValues = CreateTestVector(vectorLength, VectorDataType.Float16)
            },
            payload: 3);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
        {
            firstPoint,
            secondPoint,
            thirdPoint
        };

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
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

        var (_, upsertPointsByPointIds, _) =
            await PrepareCollection<TestPayload>(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: vectorCount);

        var pointsToDelete = upsertPointsByPointIds.Values.Take(pointsToDeleteCount).ToList();

        List<PointId> pintIdsToDelete = pointsToDelete.Select(p => p.Id).ToList();
        foreach (var pointIdToDelete in pintIdsToDelete)
        {
            upsertPointsByPointIds.Remove(((IntegerPointId) pointIdToDelete).Id);
        }

        var deletePointsResult = await _qdrantHttpClient.DeletePoints(
            TestCollectionName,
            pintIdsToDelete,
            CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointsByPointIds.Keys.Select(PointId.Integer),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        deletePointsResult.Status.IsSuccess.Should().BeTrue();

        deletePointsResult.Result.Status.Should().Be(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(vectorCount - pointsToDeleteCount);

        foreach (var readPoint in readPointsResult.Result)
        {
            var readPointId = readPoint.Id.As<IntegerPointId>().Id;

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.As<IntegerPointId>().Id.Should().Be(readPointId);

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPoint.Payload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPoint.Payload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPoint.Payload.Text);
        }
    }

    #endregion

    #region Points vectors and payload operations

    [Test]
    public async Task SetPointsPayload()
    {
        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection<TestPayload>(
                _qdrantHttpClient,
                TestCollectionName,
                payloadInitializerFunction: (i) => i + 1);

        // update payload by id

        var pointIdsToUpdatePayloadFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var setPayloadById = await _qdrantHttpClient.SetPointsPayload(
            TestCollectionName,
            new SetPointsPayloadRequest<TestPayload>(
                "100",
                pointIdsToUpdatePayloadFor
            ),
            CancellationToken.None
        );

        setPayloadById.Status.IsSuccess.Should().BeTrue();

        // check payload updated by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdatePayloadFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdatePayloadFor.Contains(p.Id));

        // check initial key is intact
        readAllPoints.Result.Should().AllSatisfy(p => p.Payload.As<TestPayload>()
            .Integer.Should().BeGreaterThan(0));

        pointsThatShouldBeUpdated.Should().AllSatisfy(p => p.Payload.As<TestPayload>()
            .Text.Should().Be("100"));
        pointsThatShouldNotBeUpdated.Should().AllSatisfy(p => p.Payload.As<TestPayload>()
            .Text.Should().NotBe("100"));

        // update payload by filter

        var pointsToUpdatePayloadByFilter = upsertPoints.Skip(5).Take(5);

        var pointIdsToUpdateByFilter = pointsToUpdatePayloadByFilter.Select(p => p.Id).ToHashSet();

        var pointFilterToUpdatePayloadFor = QdrantFilter.Create(
            Q.Must(
                Q.HaveAnyId(pointIdsToUpdateByFilter))
        );

        var setPayloadByFilter = await _qdrantHttpClient.SetPointsPayload(
            TestCollectionName,
            new SetPointsPayloadRequest<TestPayload>(
                "1000",
                pointFilterToUpdatePayloadFor),
            CancellationToken.None);

        setPayloadByFilter.Status.IsSuccess.Should().BeTrue();

        // check payload updated by filter

        readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        readAllPoints.Status.IsSuccess.Should().BeTrue();

        pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdateByFilter.Contains(p.Id));
        pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdateByFilter.Contains(p.Id));

        // check initial key is intact
        readAllPoints.Result.Should().AllSatisfy(
            p => p.Payload.As<TestPayload>().Integer.Should().BeGreaterThan(0)
        );

        pointsThatShouldBeUpdated.Should().AllSatisfy(
            p => p.Payload.As<TestPayload>().Text.Should().Be("1000")
        );
        pointsThatShouldNotBeUpdated.Should().AllSatisfy(
            p => p.Payload.As<TestPayload>().Text.Should().NotBe("1000")
        );
    }

    [Test]
    public async Task OverwritePointsPayload()
    {
        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection<TestPayload>(
                _qdrantHttpClient,
                TestCollectionName,
                payloadInitializerFunction: (i) => i + 1);

        // overwrite payload by id

        var pointIdsToUpdatePayloadFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var overwritePayloadById = await _qdrantHttpClient.OverwritePointsPayload(
            TestCollectionName,
            new OverwritePointsPayloadRequest<TestPayload>(
                "100",
                pointIdsToUpdatePayloadFor
            ),
            CancellationToken.None
        );

        overwritePayloadById.Status.IsSuccess.Should().BeTrue();

        // check payload updated by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdatePayloadFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdatePayloadFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(
            p =>
            {
                // check initial key is overwritten
                p.Payload.As<TestPayload>().Integer.Should().BeNull();

                p.Payload.As<TestPayload>().Text.Should().Be("100");
            });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(p => {
            // check initial key is not overwritten
            p.Payload.As<TestPayload>().Integer.Should().NotBeNull();

            p.Payload.As<TestPayload>().Text.Should().NotBe("100");
        });

        // overwrite payload by filter

        var pointsToUpdatePayloadByFilter = upsertPoints.Skip(5).Take(5);

        var pointIdsToUpdateByFilter = pointsToUpdatePayloadByFilter
            .Select(p => p.Id).ToHashSet();

        var pointFilterToUpdatePayloadFor = QdrantFilter.Create(
            Q.Must(
                Q.HaveAnyId(pointIdsToUpdateByFilter))
        );

        var overwritePayloadByFilter = await _qdrantHttpClient.OverwritePointsPayload(
            TestCollectionName,
            new OverwritePointsPayloadRequest<TestPayload>(
                "1000",
                pointFilterToUpdatePayloadFor),
            CancellationToken.None);

        overwritePayloadByFilter.Status.IsSuccess.Should().BeTrue();

        // check payload updated by filter

        readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        readAllPoints.Status.IsSuccess.Should().BeTrue();

        pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdateByFilter.Contains(p.Id));

        pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdateByFilter.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(
            p =>
            {
                // check initial key is overwritten
                p.Payload.As<TestPayload>().Integer.Should().BeNull();

                p.Payload.As<TestPayload>().Text.Should().Be("1000");
            });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(p => p.Payload.As<TestPayload>()
            .Text.Should().NotBe("1000"));
    }

    [Test]
    public async Task OverwritePointsPayload_NestedPropertyPath()
    {
        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                payloadInitializerFunction: (i) => new TestComplexPayload()
                {
                    IntProperty = i,
                    Nested = new TestComplexPayload.NestedClass()
                    {
                        Integer = i
                    }
                });

        // overwrite payload by id and path

        var pointIdsToUpdatePayloadFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var overwritePayloadById = await _qdrantHttpClient.OverwritePointsPayload(
            TestCollectionName,
            new OverwritePointsPayloadRequest<object>(
                new{Integer = 100},
                pointIdsToUpdatePayloadFor,
                "nested"
            ),
            CancellationToken.None
        );

        overwritePayloadById.Status.IsSuccess.Should().BeTrue();

        // check payload updated by id and path

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdatePayloadFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdatePayloadFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(
            p =>
            {
                // check initial key is overwritten
                p.Payload.As<TestComplexPayload>().IntProperty.Should().NotBeNull();

                p.Payload.As<TestComplexPayload>().Nested.Integer.Should().Be(100);
            });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(p => {
            // check initial key is not overwritten
            p.Payload.As<TestComplexPayload>().IntProperty.Should().NotBeNull();

            p.Payload.As<TestComplexPayload>().Nested.Integer.Should().NotBe(100);
        });
    }

    [Test]
    public async Task DeletePointsPayloadKeys()
    {
        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                payloadInitializerFunction: (i) => new TestPayload()
                {
                    Integer = i + 1,
                    Text = (i + 1).ToString()
                });

        // delete payload key by id

        var pointIdsToDeletePayloadKeysFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var deletePayloadKeysById = await _qdrantHttpClient.DeletePointsPayloadKeys(
            TestCollectionName,
            new DeletePointsPayloadKeysRequest(
                // we can't use the plain literal "Integer" here since when we are serializing json we are lowercasing property names
                // thus we are using filter helper here
                Q<TestPayload>.GetPayloadFieldName(p => p.Integer).YieldSingle(),
                pointIdsToDeletePayloadKeysFor),
            CancellationToken.None
        );

        deletePayloadKeysById.Status.IsSuccess.Should().BeTrue();

        // check payload key deleted by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToDeletePayloadKeysFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToDeletePayloadKeysFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(
            p =>
            {
                // check initial key intact
                p.Payload.As<TestPayload>().Text.Should().NotBeNull();

                p.Payload.As<TestPayload>().Integer.Should().BeNull();
            });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(
            p =>
            {
                // check initial key intact
                p.Payload.As<TestPayload>().Text.Should().NotBeNull();

                p.Payload.As<TestPayload>().Integer.Should().NotBeNull();
            });

        // delete payload key by filter

        var pointsToDeletePayloadKeysForByFilter = upsertPoints.Skip(5).Take(5);

        var pointIdsToUpdateByFilter = pointsToDeletePayloadKeysForByFilter
            .Select(p => p.Id).ToHashSet();

        var pointFilterToUpdatePayloadFor = QdrantFilter.Create(
            Q.Must(
                Q.HaveAnyId(pointIdsToUpdateByFilter))
        );

        var deletePayloadKeysByFilter = await _qdrantHttpClient.DeletePointsPayloadKeys(
            TestCollectionName,
            new DeletePointsPayloadKeysRequest(
                // we can't use the plain literal "Integer" here since when we are serializing json we are lowercasing property names
                // thus we are using filter helper here
                Q<TestPayload>.GetPayloadFieldName(p => p.Text).YieldSingle(),
                pointFilterToUpdatePayloadFor),
            CancellationToken.None);

        deletePayloadKeysByFilter.Status.IsSuccess.Should().BeTrue();

        // check payload updated by filter

        readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        readAllPoints.Status.IsSuccess.Should().BeTrue();

        pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdateByFilter.Contains(p.Id));

        pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdateByFilter.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(
            p =>
            {
                // check initial key intact
                p.Payload.As<TestPayload>().Text.Should().BeNull();

                // check initial key is overwritten
                p.Payload.As<TestPayload>().Integer.Should().NotBeNull();
            });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(
            p =>
            {
                p.Payload.As<TestPayload>().Text.Should().NotBeNull();
            });
    }

    [Test]
    public async Task ClearPointsPayload()
    {
        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                payloadInitializerFunction: (i) => new TestPayload()
                {
                    Integer = i + 1,
                    Text = (i + 1).ToString()
                });

        // clear payload by id

        var pointIdsToClearPayloadFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var clearPayloadById = await _qdrantHttpClient.ClearPointsPayload(
            TestCollectionName,
            new ClearPointsPayloadRequest(pointIdsToClearPayloadFor),
            CancellationToken.None
        );

        clearPayloadById.Status.IsSuccess.Should().BeTrue();

        // check payload cleared by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToClearPayloadFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToClearPayloadFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(
            p =>
            {
                p.Payload.Should().BeNull();
            });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(
            p =>
            {
                p.Payload.Should().NotBeNull();
            });

        // clear payload key by filter

        var pointsToClearPayloadForByFilter = upsertPoints.Skip(5).Take(5);

        var pointIdsToUpdateByFilter = pointsToClearPayloadForByFilter
            .Select(p => p.Id).ToHashSet();

        var pointFilterToUpdatePayloadFor = QdrantFilter.Create(
            Q.Must(
                Q.HaveAnyId(pointIdsToUpdateByFilter))
        );

        var clearPayloadByFilter = await _qdrantHttpClient.ClearPointsPayload(
            TestCollectionName,
            new ClearPointsPayloadRequest(pointFilterToUpdatePayloadFor),
            CancellationToken.None);

        clearPayloadByFilter.Status.IsSuccess.Should().BeTrue();

        // check payload cleared by filter

        readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        readAllPoints.Status.IsSuccess.Should().BeTrue();

        readAllPoints.Result.Should().AllSatisfy(p => p.Payload.Should().BeNull());
    }

    [Test]
    public async Task UpdatePointsVectors()
    {
        var vectorSize = 10U;

        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                vectorSize: vectorSize,
                payloadInitializerFunction: (i) => new TestPayload()
                {
                    Integer = i + 1,
                    Text = (i + 1).ToString()
                });

        // update points vectors by id

        var vectorToUpdateTo = CreateConstantTestVector(1.1f, vectorSize);

        var pointIdsToUpdateVectorsFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var updatePointVectorsById =
            await _qdrantHttpClient.UpdatePointsVectors(
                TestCollectionName,
                new UpdatePointsVectorsRequest()
                {
                    Points = pointIdsToUpdateVectorsFor.Select(
                        pid => new PointVector()
                        {
                            Id = pid,
                            Vector = vectorToUpdateTo
                        }).ToArray()
                },
                CancellationToken.None
            );

        updatePointVectorsById.Status.IsSuccess.Should().BeTrue();

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        // check vector updated by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdateVectorsFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdateVectorsFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(
            p =>
            {
                p.Vector.Default.AsDenseVector().VectorValues
                    .Should().BeEquivalentTo(vectorToUpdateTo);
            });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(
            p =>
            {
                p.Vector.Default.AsDenseVector().VectorValues.Should().NotBeEquivalentTo(vectorToUpdateTo);
            });
    }

    [Test]
    public async Task DeletePointsVectors_ById()
    {
        var vectorSize = 10U;
        var vectorCount = 10;
        var namedVectorsCount = 3;

        var vectorNames = CreateVectorNames(namedVectorsCount);

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true,
                vectorNames)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
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

        var upsertPointIds = upsertPoints.Select(p => p.Id).ToList();

        var pointIdsToDeleteVectorsFor = upsertPointIds.Take(5).ToHashSet();

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        upsertPointsResult.EnsureSuccess();

        var vectorNamesToDelete = vectorNames.Take(2).ToList();
        var vectorNamesToLeave = vectorNames.Skip(2).Take(2).ToList();

        var deletePointsVectorsResponse = await _qdrantHttpClient.DeletePointsVectors(
            TestCollectionName,
            new DeletePointsVectorsRequest(vectorNamesToDelete, pointIdsToDeleteVectorsFor),
            CancellationToken.None);

        deletePointsVectorsResponse.Status.IsSuccess.Should().BeTrue();

        // check vectors deleted by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToDeleteVectorsFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToDeleteVectorsFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should()
            .AllSatisfy(p => vectorNamesToDelete.All(vn => !p.Vector.ContainsVector(vn)).Should().BeTrue())
            .And.AllSatisfy(p => vectorNamesToLeave.All(vn => p.Vector.ContainsVector(vn)).Should().BeTrue());

        pointsThatShouldNotBeUpdated.Should()
            .AllSatisfy(p => vectorNamesToDelete.All(vn => p.Vector.ContainsVector(vn)).Should().BeTrue())
            .And.AllSatisfy(p => vectorNamesToLeave.All(vn => p.Vector.ContainsVector(vn)).Should().BeTrue());
    }

    [Test]
    public async Task DeletePointsVectors_ByFilter()
    {
        var vectorSize = 10U;
        var vectorCount = 10;
        var namedVectorsCount = 3;

        var vectorNames = CreateVectorNames(namedVectorsCount);

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true,
                vectorNames)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
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

        var upsertPointIds = upsertPoints.Select(p => p.Id).ToList();

        var pointIdsToDeleteVectorsFor = upsertPointIds.Take(5).ToHashSet();

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        upsertPointsResult.EnsureSuccess();

        var vectorNamesToDelete = vectorNames.Take(2).ToList();
        var vectorNamesToLeave = vectorNames.Skip(2).Take(2).ToList();

        var deletePointsVectorsResponse = await _qdrantHttpClient.DeletePointsVectors(
            TestCollectionName,
            new DeletePointsVectorsRequest(
                vectorNamesToDelete,
                QdrantFilter.Create(
                    Q.Must(
                        Q.HaveAnyId(pointIdsToDeleteVectorsFor)
                    )
                )
            ),
            CancellationToken.None);

        deletePointsVectorsResponse.Status.IsSuccess.Should().BeTrue();

        // check vectors deleted by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToDeleteVectorsFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToDeleteVectorsFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should()
            .AllSatisfy(p => vectorNamesToDelete.All(vn => !p.Vector.ContainsVector(vn)).Should().BeTrue())
            .And.AllSatisfy(p => vectorNamesToLeave.All(vn => p.Vector.ContainsVector(vn)).Should().BeTrue());

        pointsThatShouldNotBeUpdated.Should()
            .AllSatisfy(p => vectorNamesToDelete.All(vn => p.Vector.ContainsVector(vn)).Should().BeTrue())
            .And.AllSatisfy(p => vectorNamesToLeave.All(vn => p.Vector.ContainsVector(vn)).Should().BeTrue());
    }

    #endregion
}
