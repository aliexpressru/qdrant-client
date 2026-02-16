using System.Text.Json.Nodes;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Helpers;
using Aer.QdrantClient.Tests.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;
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
                    Points =
                    [
                        new(testPointId, testVector, testPayload)
                    ]
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
    [TestCase(VectorDataType.Float32)]
    [TestCase(VectorDataType.Uint8)]
    [TestCase(VectorDataType.Float16)]
    public async Task UpsertPoint_By_Batch(VectorDataType vectorDataType)
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

        PointId[] testPointIds = [
            PointId.NewGuid(), 
            PointId.NewGuid()
        ];

        VectorBase[] testVectors = [
            CreateTestVector(vectorSize, vectorDataType), 
            CreateTestVector(vectorSize, vectorDataType)
        ];

        TestPayload[] testPayloads = ["test_1", "test_2"];

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(testVectors),
                        testPayloads)
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            VectorSelector.All);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Should().NotBeNull();

        readPointsResult.Result.Select(x => x.Id.ObjectId).Should().BeEquivalentTo(testPointIds.Select(x => x.ObjectId));
        readPointsResult.Result.Select(x => x.Vector.Default.AsDenseVector().VectorValues)
            .Should().BeEquivalentTo(testVectors.Select(x => x.AsDenseVector().VectorValues));
        var readTestPayloads = readPointsResult.Result.Select(x => x.Payload.As<TestPayload>()).ToArray();

        for (int i = 0; i < readTestPayloads.Length; i++)
        {
            var readTestPayload = readTestPayloads[i];
            var testPayload = testPayloads[i];
            readTestPayload.Integer.Should().Be(testPayload.Integer);
            readTestPayload.FloatingPointNumber.Should().Be(testPayload.FloatingPointNumber);
            readTestPayload.Text.Should().Be(testPayload.Text);
        }
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
            TypeCode anyOther => throw new InvalidOperationException($"Unsupported point ID type code: {anyOther}")
        };

        var testPointId = PointId.Integer(integerPointId);

        var testVector = CreateTestVector(vectorSize);

        TestPayload testPayload = "test";

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points =
                    [
                        new(testPointId, testVector, testPayload)
                    ]
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
    public async Task UpsertPoint_By_Batch_IntegerId(object pointIdIntegerTypeExample)
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
            TypeCode anyOther => throw new InvalidOperationException($"Unsupported point ID type code: {anyOther}")
        };

        PointId[] testPointIds = [
            PointId.Integer(integerPointId),
            PointId.Integer(integerPointId + 1)
        ];

        VectorBase[] testVectors = [
            CreateTestVector(vectorSize),
            CreateTestVector(vectorSize)
        ];

        TestPayload[] testPayloads = ["test_1", "test_2"];

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(testVectors),
                        testPayloads)
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            VectorSelector.All);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Should().NotBeNull();

        readPointsResult.Result.Select(x => x.Id.ObjectId).Should().BeEquivalentTo(testPointIds.Select(x => x.ObjectId));
        readPointsResult.Result.Select(x => x.Vector.Default.AsDenseVector().VectorValues)
            .Should().BeEquivalentTo(testVectors.Select(x => x.AsDenseVector().VectorValues));
        var readTestPayloads = readPointsResult.Result.Select(x => x.Payload.As<TestPayload>()).ToArray();

        for (int i = 0; i < readTestPayloads.Length; i++)
        {
            var readTestPayload = readTestPayloads[i];
            var testPayload = testPayloads[i];
            readTestPayload.Integer.Should().Be(testPayload.Integer);
            readTestPayload.FloatingPointNumber.Should().Be(testPayload.FloatingPointNumber);
            readTestPayload.Text.Should().Be(testPayload.Text);
        }
    }

    [Test]
    public async Task UpsertPoint_VariousPayloadTypes()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        object[] payloads =
        [
            new JsonObject()
            {
                {"test_field", 1},
                {"test_field2", "some_string"},
            },

            new JObject()
            {
                {"test_field", 1},
                {"test_field2", "some_string"},
            },

            new
            {
                TestField = 1,
                TestField2 = "some_string",
            },

            JsonConvert.SerializeObject(
                new
                {
                    TestField = 1,
                    TestField2 = "some_string"
                },
                Formatting.None,
                new JsonSerializerSettings()
                {
                    Converters = new List<JsonConverter>(
                    [
                        new StringEnumConverter
                        {
                            NamingStrategy = new CamelCaseNamingStrategy()
                        }
                    ]),
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    }
                }
            ),

            JsonSerializer.Serialize(
                new
                {
                    TestField = 1,
                    TestField2 = "some_string",
                },
                JsonSerializerConstants.DefaultSerializerOptions),

            """
            {
                "test_field": 1,
                "test_field2": "some_string"
            }
            """,

            new Dictionary<string, object>
            {
                {"test_field", 1},
                {"test_field2", "some_string"},
            }
        ];

        foreach (var payload in payloads)
        {
            var testPointId = PointId.NewGuid();
            var testVector = CreateTestVector(vectorSize);

            var upsertPointsResult
                = await _qdrantHttpClient.UpsertPoints(
                    TestCollectionName,
                    new UpsertPointsRequest()
                    {
                        Points =
                        [
                            new(testPointId, testVector, payload)
                        ]
                    },
                    CancellationToken.None);

            var readPointResult = await _qdrantHttpClient.GetPoint(
                TestCollectionName,
                testPointId,
                CancellationToken.None);

            upsertPointsResult.Status.IsSuccess.Should().BeTrue();
            upsertPointsResult.Result.Status.Should()
                .BeOneOf(QdrantOperationStatus.Completed);

            readPointResult.Status.IsSuccess.Should().BeTrue();
            readPointResult.Result.Should().NotBeNull();

            readPointResult.Result.Id.ObjectId.Should().Be(testPointId.ObjectId);
            readPointResult.Result.Vector.Default.AsDenseVector().VectorValues
                .Should().BeEquivalentTo(testVector);

            readPointResult.Result.IsPayloadNullOrEmpty.Should().BeFalse();
            readPointResult.Result.Payload.Should().NotBeNull();
            readPointResult.Result.Payload.IsEmpty.Should().BeFalse();

            readPointResult.Result.Payload.ContainsField("test_field").Should().BeTrue();
            readPointResult.Result.Payload.ContainsField("test_field2").Should().BeTrue();

            readPointResult.Result.Payload.TryGetValue<int>("test_field", out var testValue).Should().BeTrue();
            testValue.Should().Be(1);
            readPointResult.Result.Payload.GetValue<int>("test_field").Should().Be(1);

            readPointResult.Result.Payload.TryGetValue<string>("test_field2", out var test2Value).Should().BeTrue();
            test2Value.Should().Be("some_string");
            readPointResult.Result.Payload.GetValue<string>("test_field2").Should().Be("some_string");
        }
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
                    Points =
                    [
                        new(testPointId, testVector, null)
                    ]
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

        readPointsResult.Result.Payload.RawPayloadString.Should().Be(Payload.EmptyString);

        // Since we are not filtering out all payload properties but didn't
        // have any payload in the first place, we get an empty payload {}, not null

        readPointsResult.Result.Payload.Should().NotBeNull();
        readPointsResult.Result.IsPayloadNullOrEmpty.Should().BeTrue();
    }
    
    [Test]
    public async Task UpsertPoint_By_Batch_NullPayload()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        PointId[] testPointIds = [
            PointId.NewGuid(),
            PointId.NewGuid()
        ];
        VectorBase[] testVectors =
        [
            CreateTestVector(vectorSize),
            CreateTestVector(vectorSize)
        ];

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(testVectors), 
                        null)
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            VectorSelector.All);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Should().NotBeNull();
        
        readPointsResult.Result.Select(x => x.Id.ObjectId).Should().BeEquivalentTo(testPointIds.Select(x => x.ObjectId));
        readPointsResult.Result.Select(x => x.Vector.Default.AsDenseVector().VectorValues)
            .Should().BeEquivalentTo(testVectors.Select(x => x.AsDenseVector().VectorValues));
        readPointsResult.Result.Select(x => x.Payload.RawPayloadString).Should().AllBe(Payload.EmptyString);

        // Since we are not filtering out all payload properties but didn't
        // have any payload in the first place, we get an empty payload {}, not null

        readPointsResult.Result.Select(x => x.Payload).Should().AllSatisfy(x => x.Should().NotBeNull());
        readPointsResult.Result.Select(x => x.IsPayloadNullOrEmpty).Should().AllSatisfy(x => x.Should().BeTrue());
    }
    
    [Test]
    public async Task UpsertPoint_By_Batch_EmptyPayloads_Should_Fail()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        PointId[] testPointIds = [
            PointId.NewGuid(), 
            PointId.NewGuid()
        ];

        VectorBase[] testVectors = [
            CreateTestVector(vectorSize), 
            CreateTestVector(vectorSize)
        ];

        var act =
            async () => await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(testVectors),
                        [])
                },
                CancellationToken.None);

        await act.Should().ThrowAsync<QdrantCommunicationException>()
            .Where(e => 
                e.Message.Contains("batch.batch: number of ids and payloads must be equal", StringComparison.InvariantCultureIgnoreCase)); 
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
                    Points =
                    [
                        new(testPointId, testVector, testPayload)
                    ]
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
        readPointsResult.Result[0].IsPayloadNullOrEmpty.Should().BeTrue();
    }
    
    [Test]
    public async Task UpsertPoint_By_Batch_GetWithoutVectorAndPayload()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        PointId[] testPointIds = [
            PointId.NewGuid(), 
            PointId.NewGuid()
        ];

        VectorBase[] testVectors = [
            CreateTestVector(vectorSize), 
            CreateTestVector(vectorSize)
        ];
        
        TestPayload[] testPayloads = ["test_1", "test_2"];

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(testVectors),
                        testPayloads)
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.None,
            CancellationToken.None,
            withVector: false);
        
        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(2);

        readPointsResult.Result.Select(x => x.Id.ObjectId).Should().BeEquivalentTo(testPointIds.Select(x => x.ObjectId));
        readPointsResult.Result.Select(x => x.Vector).Should().AllSatisfy(x => x.Should().BeNull());
        readPointsResult.Result.Select(x => x.Payload).Should().AllSatisfy(x => x.Should().BeNull());
        readPointsResult.Result.Select(x => x.IsPayloadNullOrEmpty).Should().AllSatisfy(x => x.Should().BeTrue());
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
                Points =
                [
                    new(testPointId, testVector, testPayload)
                ]
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
        readPointNoPayloadPropertiesImplicitResult.Result[0].IsPayloadNullOrEmpty.Should().BeTrue();
    }
    
     [Test]
    public async Task UpsertPoint_By_Batch_GetWithPayloadSelector()
    {
        var vectorSize = 10U;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        PointId[] testPointIds = [
            PointId.NewGuid(), 
            PointId.NewGuid()
        ];

        VectorBase[] testVectors = [
            CreateTestVector(vectorSize), 
            CreateTestVector(vectorSize)
        ];

        TestPayload testPayload1 = "test_1";
        testPayload1.Integer = 1567;
        testPayload1.FloatingPointNumber = 15.67;
        testPayload1.DateTimeValue = DateTime.Parse("2020-01-01T00:00:00");
        TestPayload testPayload2 = "test_2";
        testPayload2.Integer = 1345;
        testPayload2.FloatingPointNumber = 11.07;
        testPayload2.DateTimeValue = DateTime.Parse("2021-11-01T00:00:00");

        await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest()
            {
                Batch = new UpsertPointsRequest.UpsertPointsBatch(
                    testPointIds, 
                    new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(testVectors), 
                    [testPayload1, testPayload2])
            },
            CancellationToken.None);

        // some payload properties included

        var readPointIncludePayloadPropertyResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.Include("integer"),
            CancellationToken.None,
            withVector: false);

        readPointIncludePayloadPropertyResult.Status.IsSuccess.Should().BeTrue();
        readPointIncludePayloadPropertyResult.Result.Length.Should().Be(2);

        readPointIncludePayloadPropertyResult.Result.Select(x => x.Payload).Should().AllSatisfy(x => x.Should().NotBeNull());

        var payloadsWithIncludedProperties = readPointIncludePayloadPropertyResult.Result.Select(x => x.Payload.As<TestPayload>()).ToArray();

        payloadsWithIncludedProperties.Select(x => x.Integer.HasValue).Should().AllSatisfy(x => x.Should().BeTrue());
        payloadsWithIncludedProperties.Select(x => x.FloatingPointNumber.HasValue).Should().AllSatisfy(x => x.Should().BeFalse());
        payloadsWithIncludedProperties.Select(x => x.Text).Should().AllBe(null);
        payloadsWithIncludedProperties.Select(x => x.DateTimeValue).Should().AllSatisfy(x => x.Should().BeNull());

        // some payload properties excluded

        var readPointExcludePayloadPropertyResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.Exclude("integer"),
            CancellationToken.None,
            withVector: false);

        readPointExcludePayloadPropertyResult.Status.IsSuccess.Should().BeTrue();
        readPointExcludePayloadPropertyResult.Result.Length.Should().Be(2);

        readPointExcludePayloadPropertyResult.Result[0].Payload.Should().NotBeNull();

        var payloadWithExcludedProperties = readPointExcludePayloadPropertyResult.Result[0].Payload.As<TestPayload>();

        payloadWithExcludedProperties.Integer.HasValue.Should().BeFalse();
        payloadWithExcludedProperties.FloatingPointNumber.HasValue.Should().BeTrue();
        payloadWithExcludedProperties.Text.Should().NotBeNull();
        payloadWithExcludedProperties.DateTimeValue.Should().NotBeNull();

        // all payload properties selected (explicit PayloadSelector)

        var readPointAllPayloadPropertiesResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: false);

        readPointAllPayloadPropertiesResult.Status.IsSuccess.Should().BeTrue();
        readPointAllPayloadPropertiesResult.Result.Length.Should().Be(2);

        readPointAllPayloadPropertiesResult.Result.Select(x => x.Payload).Should().AllSatisfy(x => x.Should().NotBeNull());
        var payloadsWithAllProperties = readPointAllPayloadPropertiesResult.Result.Select(x => x.Payload.As<TestPayload>()).ToArray();

        payloadsWithAllProperties.Should().AllSatisfy(x => x.AllPropertiesNotNull().Should().BeTrue());

        // all payload properties selected (implicit PayloadSelector)

        var readPointAllPayloadPropertiesImplicitResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            withPayload: true,
            CancellationToken.None,
            withVector: false);

        readPointAllPayloadPropertiesImplicitResult.Status.IsSuccess.Should().BeTrue();
        readPointAllPayloadPropertiesImplicitResult.Result.Length.Should().Be(2);

        readPointAllPayloadPropertiesImplicitResult.Result[0].Payload.Should().NotBeNull();

        readPointAllPayloadPropertiesResult.Result.Select(x => x.Payload).Should().AllSatisfy(x => x.Should().NotBeNull());
        var payloadsWithAllProperties2 = readPointAllPayloadPropertiesResult.Result.Select(x => x.Payload.As<TestPayload>()).ToArray();

        payloadsWithAllProperties2.Should().AllSatisfy(x => x.AllPropertiesNotNull().Should().BeTrue());
        
        // no payload properties selected (explicit PayloadSelector)

        var readPointNoPayloadPropertiesResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.None,
            CancellationToken.None,
            withVector: false);

        readPointNoPayloadPropertiesResult.Status.IsSuccess.Should().BeTrue();
        readPointNoPayloadPropertiesResult.Result.Length.Should().Be(2);

        readPointNoPayloadPropertiesResult.Result.Select(x => x.Payload).Should().AllSatisfy(x => x.Should().BeNull());

        // no payload properties selected (implicit PayloadSelector)

        var readPointNoPayloadPropertiesImplicitResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            withPayload: false,
            CancellationToken.None,
            withVector: false);

        readPointNoPayloadPropertiesImplicitResult.Status.IsSuccess.Should().BeTrue();
        readPointNoPayloadPropertiesImplicitResult.Result.Length.Should().Be(2);

        readPointNoPayloadPropertiesImplicitResult.Result.Select(x => x.Payload).Should().AllSatisfy(x => x.Should().BeNull());
        readPointNoPayloadPropertiesImplicitResult.Result.Select(x => x.IsPayloadNullOrEmpty).Should().AllSatisfy(x => x.Should().BeTrue());
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
                    Points =
                    [
                        new(PointId.NewGuid(), CreateTestVector(vectorSize), (TestPayload) "test")
                    ]
                },
                CancellationToken.None,
                isWaitForResult: false);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Acknowledged);
    }
    
    [Test]
    public async Task UpsertPoint_By_Batch_AsyncOperation()
    {
        var vectorSize = 10U;
        
        PointId[] testPointIds = [
            PointId.NewGuid(), 
            PointId.NewGuid()
        ];

        VectorBase[] testVectors = [
            CreateTestVector(vectorSize), 
            CreateTestVector(vectorSize)
        ];
        
        TestPayload[] testPayloads = ["test_1", "test_2"];

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
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds,
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(testVectors), 
                        testPayloads)
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
                    PointId.Integer((ulong)i),
                    CreateTestVector(vectorSize),
                    (TestPayload)i
                )
            );
        }

        Dictionary<ulong, UpsertPointsRequest.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId)p.Id).Id);

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

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);
        }
    }
    
        [Test]
    public async Task UpsertPoints_By_Batch()
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
        
        PointId[] testPointIds = Enumerable.Range(0, vectorCount).Select(x => PointId.Integer((ulong)x)).ToArray();
        TestPayload[] testPayloads = Enumerable.Range(0, vectorCount).Select(x => (TestPayload)x).ToArray();
        VectorBase[] testVectors = Enumerable.Range(0, vectorCount).Select(x => (VectorBase)CreateTestVector(vectorSize)).ToArray();
        
        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(testVectors), 
                        testPayloads)
                },
                CancellationToken.None,
                isWaitForResult: true,
                ordering: OrderingType.Strong);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
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

        for (int i = 0; i < testPointIds.Length; i++)
        {
            var pointId = testPointIds[i];
    
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(pointId));

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            var expectedPointPayload = testPayloads[i].As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);
        }
    }

    [Test]
    public async Task UpsertPoints_WithUpdateFilter()
    {
        OnlyIfVersionAfterOrEqual("1.16.0", "Conditional updates are available only from v1.16");

        var vectorSize = 10U;
        var vectorCount = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        // Initial upsert of points

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong)i),
                    CreateTestVector(vectorSize),
                    (TestPayload)i
                )
            );
        }

        Dictionary<ulong, UpsertPointsRequest.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId)p.Id).Id);

        (await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest()
            {
                Points = upsertPoints
            },
            CancellationToken.None,
            isWaitForResult: true,
            ordering: OrderingType.Strong)
        ).EnsureSuccess();

        // Conditional upsert with update filter

        var notUpdatedPointPointId = PointId.Integer((ulong)0);
        var updatedPointPointId = PointId.Integer((ulong)1);
        var upsertedPointPointId = PointId.Integer((ulong)1000);

        List<UpsertPointsRequest.UpsertPoint> updatedPoints = [
            // Does not satisfy the filter and should NOT be updated
            new(
                notUpdatedPointPointId,
                upsertPoints[0].Vector,
                (TestPayload)1000
            ),

            // Satisfies the filter and should be updated
            new(
                updatedPointPointId,
                upsertPoints[1].Vector,
                new TestPayload(){
                    Integer = 2,
                    Text = "test"
                }
            ),

            // Completely new point, should be inserted
            new(
                upsertedPointPointId,
                CreateTestVector(vectorSize),
                new TestPayload(){
                    Integer = 1000,
                    Text = "test_new"
                }
            ),
        ];

        var readPointsBeforeUpdateResult = (
            await _qdrantHttpClient.GetPoints(
                TestCollectionName,
                updatedPoints.Select(p => p.Id),
                PayloadPropertiesSelector.All,
                CancellationToken.None,
                withVector: true,
                retryCount: 0)
            ).EnsureSuccess();

        var conditionalUpsertResult = await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest()
            {
                Points = updatedPoints,
                UpdateFilter = Q.MatchValue("integer", 1)
            },
            CancellationToken.None,
            isWaitForResult: true,
            ordering: OrderingType.Strong);

        conditionalUpsertResult.Status.IsSuccess.Should().BeTrue();

        var readPointsAfterUpdateResult = (
            await _qdrantHttpClient.GetPoints(
                TestCollectionName,
                updatedPoints.Select(p => p.Id),
                PayloadPropertiesSelector.All,
                CancellationToken.None,
                withVector: true,
                retryCount: 0)
            ).EnsureSuccess();

        readPointsAfterUpdateResult.Length.Should().Be(updatedPoints.Count);

        readPointsAfterUpdateResult.Length.Should().BeGreaterThan(readPointsBeforeUpdateResult.Length); // 1 new point inserted

        // Check point was not updated

        var initialNotUpdatedPoint = readPointsBeforeUpdateResult.Single(p => p.Id == notUpdatedPointPointId);
        var pointNotUpdated = readPointsAfterUpdateResult.Single(p => p.Id == notUpdatedPointPointId);

        var initialPayload = initialNotUpdatedPoint.Payload.As<TestPayload>();
        var notUpdatedPayload = pointNotUpdated.Payload.As<TestPayload>();

        notUpdatedPayload.Integer.Should().Be(initialPayload.Integer);
        notUpdatedPayload.Text.Should().Be(initialPayload.Text);

        // Check point was updated

        var initialUpdatedPoint = readPointsBeforeUpdateResult.Single(p => p.Id == updatedPointPointId);
        var pointUpdated = readPointsAfterUpdateResult.Single(p => p.Id == updatedPointPointId);

        var initialUpdatedPayload = initialUpdatedPoint.Payload.As<TestPayload>();
        var updatedPayload = pointUpdated.Payload.As<TestPayload>();

        updatedPayload.Integer.Should().NotBe(initialUpdatedPayload.Integer);
        updatedPayload.Integer.Should().Be(2);

        updatedPayload.Text.Should().NotBe(initialUpdatedPayload.Text);
        updatedPayload.Text.Should().Be("test");

        // Check point was inserted

        var pointInserted = readPointsAfterUpdateResult.Single(p => p.Id == upsertedPointPointId);
        var insertedPayload = pointInserted.Payload.As<TestPayload>();
        insertedPayload.Integer.Should().Be(1000);
        insertedPayload.Text.Should().Be("test_new");
    }
    
     [Test]
    public async Task UpsertPoints_By_Batch_WithUpdateFilter()
    {
        OnlyIfVersionAfterOrEqual("1.16.0", "Conditional updates are available only from v1.16");

        var vectorSize = 10U;
        var vectorCount = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        PointId[] testPointIds = Enumerable.Range(0, vectorCount).Select(x => PointId.Integer((ulong)x)).ToArray();
        TestPayload[] testPayloads = Enumerable.Range(0, vectorCount).Select(x => (TestPayload)x).ToArray();
        VectorBase[] testVectors = Enumerable.Range(0, vectorCount).Select(x => (VectorBase)CreateTestVector(vectorSize)).ToArray();
        
        (await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest()
            {
                Batch = new UpsertPointsRequest.UpsertPointsBatch(
                    testPointIds, 
                    new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(testVectors), 
                    testPayloads)
            },
            CancellationToken.None,
            isWaitForResult: true,
            ordering: OrderingType.Strong)
        ).EnsureSuccess();

        // Conditional upsert with update filter

        var notUpdatedPointPointId = PointId.Integer((ulong)0);
        var updatedPointPointId = PointId.Integer((ulong)1);
        var upsertedPointPointId = PointId.Integer((ulong)1000);

        PointId[] updatedtPointIds = [notUpdatedPointPointId, updatedPointPointId, upsertedPointPointId];
        TestPayload[] updatedPayloads = [
            1000, 
            new TestPayload(){
                Integer = 2,
                Text = "test"
            },
            new TestPayload(){
                Integer = 1000,
                Text = "test_new"
            }
        ];
        VectorBase[] updatedVectors = [ testVectors[0], testVectors[1], CreateTestVector(vectorSize)];

        var readPointsBeforeUpdateResult = (
            await _qdrantHttpClient.GetPoints(
                TestCollectionName,
                updatedtPointIds,
                PayloadPropertiesSelector.All,
                CancellationToken.None,
                withVector: true,
                retryCount: 0)
            ).EnsureSuccess();

        var conditionalUpsertResult = await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest()
            {
                Batch = new UpsertPointsRequest.UpsertPointsBatch(
                    updatedtPointIds, 
                    new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(updatedVectors), 
                    updatedPayloads),
                UpdateFilter = Q.MatchValue("integer", 1)
            },
            CancellationToken.None,
            isWaitForResult: true,
            ordering: OrderingType.Strong);

        conditionalUpsertResult.Status.IsSuccess.Should().BeTrue();

        var readPointsAfterUpdateResult = (
            await _qdrantHttpClient.GetPoints(
                TestCollectionName,
                updatedtPointIds,
                PayloadPropertiesSelector.All,
                CancellationToken.None,
                withVector: true,
                retryCount: 0)
            ).EnsureSuccess();

        readPointsAfterUpdateResult.Length.Should().Be(updatedtPointIds.Length);

        readPointsAfterUpdateResult.Length.Should().BeGreaterThan(readPointsBeforeUpdateResult.Length); // 1 new point inserted

        // Check point was not updated

        var initialNotUpdatedPoint = readPointsBeforeUpdateResult.Single(p => p.Id == notUpdatedPointPointId);
        var pointNotUpdated = readPointsAfterUpdateResult.Single(p => p.Id == notUpdatedPointPointId);

        var initialPayload = initialNotUpdatedPoint.Payload.As<TestPayload>();
        var notUpdatedPayload = pointNotUpdated.Payload.As<TestPayload>();

        notUpdatedPayload.Integer.Should().Be(initialPayload.Integer);
        notUpdatedPayload.Text.Should().Be(initialPayload.Text);

        // Check point was updated

        var initialUpdatedPoint = readPointsBeforeUpdateResult.Single(p => p.Id == updatedPointPointId);
        var pointUpdated = readPointsAfterUpdateResult.Single(p => p.Id == updatedPointPointId);

        var initialUpdatedPayload = initialUpdatedPoint.Payload.As<TestPayload>();
        var updatedPayload = pointUpdated.Payload.As<TestPayload>();

        updatedPayload.Integer.Should().NotBe(initialUpdatedPayload.Integer);
        updatedPayload.Integer.Should().Be(2);

        updatedPayload.Text.Should().NotBe(initialUpdatedPayload.Text);
        updatedPayload.Text.Should().Be("test");

        // Check point was inserted

        var pointInserted = readPointsAfterUpdateResult.Single(p => p.Id == upsertedPointPointId);
        var insertedPayload = pointInserted.Payload.As<TestPayload>();
        insertedPayload.Integer.Should().Be(1000);
        insertedPayload.Text.Should().Be("test_new");
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
                    PointId.Integer((ulong)i),
                    CreateTestVector(vectorSize, VectorDataType.Uint8),
                    (TestPayload)i
                )
            );
        }

        Dictionary<ulong, UpsertPointsRequest.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId)p.Id).Id);

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

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);
        }
    }
    
    [Test]
    public async Task UpsertPoints_By_Batch_ByteVector()
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

        PointId[] testPointIds = Enumerable.Range(0, vectorCount).Select(x => PointId.Integer((ulong)x)).ToArray();
        TestPayload[] testPayloads = Enumerable.Range(0, vectorCount).Select(x => (TestPayload)x).ToArray();
        VectorBase[] testVectors = Enumerable.Range(0, vectorCount).Select(x => (VectorBase)CreateTestVector(vectorSize, VectorDataType.Uint8)).ToArray();

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(testVectors), 
                        testPayloads)
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(vectorCount);

        for (int i = 0; i < testPointIds.Length; i++)
        {
            var pointId = testPointIds[i];

            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(pointId));

            readPoint.Vector.Default.Should().BeEquivalentTo(testVectors[i].Default);

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            var expectedPointPayload = testPayloads[i].As<TestPayload>();

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
                    PointId.Integer((ulong)i),
                    CreateTestNamedVectors(vectorSize, namedVectorsCount),
                    (TestPayload)i
                )
            );
        }

        Dictionary<ulong, UpsertPointsRequest.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId)p.Id).Id);

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

            var readPointPayload = readPoint.Payload.As<TestPayload>();

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
    public async Task UpsertPoints_By_Batch_NamedVectors_SameConfig()
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

        PointId[] testPointIds = Enumerable.Range(0, vectorCount).Select(x => (PointId)x).ToArray();
        TestPayload[] testPayloads = Enumerable.Range(0, vectorCount).Select(x => (TestPayload)x).ToArray();
        Dictionary<string, IEnumerable<VectorBase>> namedVectors = new();

        for (int i = 0; i < namedVectorsCount; i++)
        {
            namedVectors.Add(vectorNames[i], Enumerable.Range(0, vectorCount).Select(x => (VectorBase)CreateTestVector(vectorSize)));
        }
        
        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(namedVectors),
                        testPayloads)
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(vectorCount);
        
        for (int i = 0; i < testPointIds.Length; i++)
        {
            var pointId = testPointIds[i];
            
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(pointId));
            
            var expectedPointPayload = testPayloads[i];
            var readPointPayload = readPoint.Payload.As<TestPayload>();

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
                    PointId.Integer((ulong)i),
                    CreateTestNamedVectors(vectorSize, namedVectorsCount),
                    (TestPayload)i
                )
            );
        }

        Dictionary<ulong, UpsertPointsRequest.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId)p.Id).Id);

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

            var readPointPayload = readPoint.Payload.As<TestPayload>();

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
    public async Task UpsertPoints_By_Batch_NamedVectors_VectorSelector()
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

        PointId[] testPointIds = Enumerable.Range(0, vectorCount).Select(x => (PointId)x).ToArray();
        TestPayload[] testPayloads = Enumerable.Range(0, vectorCount).Select(x => (TestPayload)x).ToArray();
        Dictionary<string, IEnumerable<VectorBase>> namedVectors = new();

        for (int i = 0; i < namedVectorsCount; i++)
        {
            namedVectors.Add(vectorNames[i], Enumerable.Range(0, vectorCount).Select(x => (VectorBase)CreateTestVector(vectorSize)));
        }

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(namedVectors),
                        testPayloads)
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: VectorSelector.Include(includedVectorNames));

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(vectorCount);
        
        for (int i = 0; i < testPointIds.Length; i++)
        {
            var pointId = testPointIds[i];
            
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(pointId));
            
            var expectedPointPayload = testPayloads[i];
            var readPointPayload = readPoint.Payload.As<TestPayload>();

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
                payload: (TestPayload)1);

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
            payload: (TestPayload)2);

        UpsertPointsRequest.UpsertPoint thirdPoint = new(
            id: 3,
            vector: new NamedVectors()
            {
                Vectors = new Dictionary<string, VectorBase>()
                {
                    ["Vector_1"] = CreateTestVector(100U),
                }
            },
            payload: (TestPayload)3);

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
                vector: new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        // here and further on float[] will be implicitly converted to Vector
                        [VectorBase.DefaultVectorName] = CreateTestVector(100U, VectorDataType.Uint8),
                        ["Vector_2"] = CreateTestVector(5U),
                    }
                },
                payload: (TestPayload)1);

        UpsertPointsRequest.UpsertPoint secondPoint = new(
            id: 2,
            vector: new NamedVectors()
            {
                Vectors = new Dictionary<string, VectorBase>()
                {
                    [VectorBase.DefaultVectorName] = CreateTestVector(100, VectorDataType.Uint8),
                }
            },
            payload: (TestPayload)2);

        UpsertPointsRequest.UpsertPoint thirdPoint = new(
            id: 3,
            vector: new NamedVectors()
            {
                Vectors = new Dictionary<string, VectorBase>()
                {
                    ["Vector_2"] = CreateTestVector(5U),
                }
            },
            payload: (TestPayload)3);

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
                vector: new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        ["Vector_1"] = CreateTestSparseVector(100U, 4),
                        ["Vector_2"] = CreateTestSparseVector(50U, 5),
                    }
                },
                payload: (TestPayload)1);

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
            payload: (TestPayload)2);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>()
        {
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
    public async Task UpsertPoints_By_Batch_SparseVectors_OnlySparse()
    {
        Dictionary<string, SparseVectorConfiguration> sparseVectorsConfig = new()
        {
            ["Vector_1"] = new(true, fullScanThreshold: 1000),
            ["Vector_2"] = new(), // default sparse vector configuration
        };
        
        Dictionary<string, IEnumerable<VectorBase>> sparseVectors = new();
        sparseVectors.Add("Vector_1", [CreateTestSparseVector(100U, 4), CreateTestSparseVector(100U, 6)]);
        sparseVectors.Add("Vector_2", [CreateTestSparseVector(50U, 5), CreateTestSparseVector(50U, 7)]);
        
        PointId[] testPointIds = [1, 2];

        TestPayload[] testPayloads = [1, 2];

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(sparseVectorsConfiguration: sparseVectorsConfig)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(sparseVectors),
                        testPayloads)
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(testPointIds.Length);

        for (int i = 0; i < testPointIds.Length; i++)
        {
            var pointId = testPointIds[i];
            
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(pointId));

            var readPointVectors = readPoint.Vector.AsNamedVectors();
            
            foreach (var upsertPointVectors in sparseVectors)
            {
                readPointVectors.ContainsVector(upsertPointVectors.Key).Should().BeTrue();
            
                var readPointSparseVectorValue = readPointVectors[upsertPointVectors.Key].AsSparseVector();
                var upsertPointSparseVectorValue = upsertPointVectors.Value.ElementAt(i).AsSparseVector();
            
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
                vector: new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        ["Vector_1"] = CreateTestVector(100U),
                        ["Vector_2"] = CreateTestSparseVector(50U, 5),
                    }
                },
                payload: (TestPayload)1);

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
            payload: (TestPayload)2);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>()
        {
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

            var readPointVectors = readPoint.Vector.AsNamedVectors();

            foreach (var upsertPointVector in upsertPoint.Vector.AsNamedVectors().Vectors)
            {
                readPointVectors.ContainsVector(upsertPointVector.Key).Should().BeTrue();

                if (readPointVectors[upsertPointVector.Key].VectorKind != VectorKind.Sparse)
                {
                    var readPointNamedVectorValue =
                        readPointVectors[upsertPointVector.Key].AsDenseVector().VectorValues;
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
    public async Task UpsertPoints_By_Batch_SparseVectors_MixedSparseAndNamed()
    {
        Dictionary<string, VectorConfigurationBase.SingleVectorConfiguration> namedDenseVectorsConfig = new()
        {
            ["Vector_1"] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Manhattan,
                100,
                isServeVectorsFromDisk: true)
        };

        Dictionary<string, SparseVectorConfiguration> sparseVectorsConfig = new()
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
        
        Dictionary<string, IEnumerable<VectorBase>> namedVectors = new();
        namedVectors.Add("Vector_1", [CreateTestVector(100U), CreateTestVector(100U)]);
        namedVectors.Add("Vector_2", [CreateTestSparseVector(50U, 5), CreateTestSparseVector(50U, 7)]);
        
        PointId[] testPointIds = [1, 2];

        TestPayload[] testPayloads = [1, 2];

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                namedVectorsConfiguration: namedDenseVectorsConfig,
                sparseVectorsConfiguration: sparseVectorsConfig)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(namedVectors),
                        testPayloads)
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(testPointIds.Length);

        for (int i = 0; i < testPointIds.Length; i++)
        {
            var pointId = testPointIds[i];
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(pointId));

            var readPointVectors = readPoint.Vector.AsNamedVectors();

            foreach (var upsertPointVectors in namedVectors)
            {
                readPointVectors.ContainsVector(upsertPointVectors.Key).Should().BeTrue();

                if (readPointVectors[upsertPointVectors.Key].VectorKind != VectorKind.Sparse)
                {
                    var readPointNamedVectorValue =
                        readPointVectors[upsertPointVectors.Key].AsDenseVector().VectorValues;
                    var upsertPointNamedVectorValue = upsertPointVectors.Value.ElementAt(i).AsDenseVector().VectorValues;

                    readPointNamedVectorValue.Should().Equal(upsertPointNamedVectorValue);
                }
                else
                {
                    // means we are checking sparse vector
                    var readPointSparseVectorValue = readPointVectors[upsertPointVectors.Key].AsSparseVector();
                    var upsertPointSparseVectorValue = upsertPointVectors.Value.ElementAt(i).AsSparseVector();

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
                payload: (TestPayload)1);

        UpsertPointsRequest.UpsertPoint secondPoint = new(
            id: 2,
            vector: new MultiVector()
            {
                Vectors = CreateTestMultivector(vectorLength, 3, VectorDataType.Float32)
            },
            payload: (TestPayload)2);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>()
        {
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

            readPoint.Vector.VectorKind.Should().Be(VectorKind.Multi);

            var readPointVectors = readPoint.Vector.AsMultiVector();

            readPointVectors.Vectors.Should().BeEquivalentTo(upsertPoint.Vector.AsMultiVector().Vectors);
        }
    }
    
    [Test]
    public async Task UpsertPoints_By_Batch_MultiVectors_OnlyMulti()
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

        VectorBase[] testVectors = [CreateTestMultivector(vectorLength, 2, VectorDataType.Float32), CreateTestMultivector(vectorLength, 3, VectorDataType.Float32)];
            
        PointId[] testPointIds = [1, 2];

        TestPayload[] testPayloads = [1, 2];

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(testVectors),
                        testPayloads)
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(testPointIds.Length);

        for (int i = 0; i < testPointIds.Length; i++)
        {
            var pointId = testPointIds[i];
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(pointId));

            readPoint.Vector.VectorKind.Should().Be(VectorKind.Multi);

            var readPointVectors = readPoint.Vector.AsMultiVector();

            readPointVectors.Vectors.Should().BeEquivalentTo(testVectors[i].AsMultiVector().Vectors);
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
                vector: new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        ["Vector_1"] = CreateTestVector(100U),
                        ["Vector_2"] = CreateTestSparseVector(50U, 5),
                        ["Vector_3"] = CreateTestMultivector(20, 5, VectorDataType.Float32),
                    }
                },
                payload: (TestPayload)1);

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
            payload: (TestPayload)2);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>()
        {
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
                    var readPointNamedVectorValue =
                        readPointVectors[upsertPointVector.Key].AsDenseVector().VectorValues;
                    var upsertPointNamedVectorValue = upsertPointVector.Value.AsDenseVector().VectorValues;

                    readPointNamedVectorValue.Should().Equal(upsertPointNamedVectorValue);
                }
                else if (readPointVectorKind == VectorKind.Multi)
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
    public async Task UpsertPoints_By_Batch_MultiVectors_NamedVectors_MixedSingleSparseAndMulti()
    {
        Dictionary<string, VectorConfigurationBase.SingleVectorConfiguration> namedVectorsConfig = new()
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

        Dictionary<string, SparseVectorConfiguration> sparseVectorsConfig = new()
        {
            ["Vector_2"] = new(true, fullScanThreshold: 1000)
        };

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                namedVectorsConfiguration: namedVectorsConfig,
                sparseVectorsConfiguration: sparseVectorsConfig)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();
        
        Dictionary<string, IEnumerable<VectorBase>> namedVectors = new();
        namedVectors.Add("Vector_1", [CreateTestVector(100U), CreateTestVector(100U)]);
        namedVectors.Add("Vector_2", [CreateTestSparseVector(50U, 5), CreateTestSparseVector(50U, 7)]);
        namedVectors.Add("Vector_3", [CreateTestMultivector(20, 5, VectorDataType.Float32), CreateTestMultivector(20, 10, VectorDataType.Float32)]);
        
        PointId[] testPointIds = [1, 2];

        TestPayload[] testPayloads = [1, 2];

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(namedVectors),
                        testPayloads)
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(testPointIds.Length);

        for (int i = 0; i < testPointIds.Length; i++)
        {
            var pointId = testPointIds[i];
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(pointId));

            var readPointVectors = readPoint.Vector.AsNamedVectors();

            foreach (var upsertPointVectors in namedVectors)
            {
                readPointVectors.ContainsVector(upsertPointVectors.Key).Should().BeTrue();

                var readPointVectorKind = readPointVectors[upsertPointVectors.Key].VectorKind;

                if (readPointVectorKind == VectorKind.Sparse)
                {
                    // means we are checking sparse vector
                    var readPointSparseVectorValue = readPointVectors[upsertPointVectors.Key].AsSparseVector();
                    var upsertPointSparseVectorValue = upsertPointVectors.Value.ElementAt(i).AsSparseVector();

                    readPointSparseVectorValue.Indices.Should().Equal(upsertPointSparseVectorValue.Indices);
                    readPointSparseVectorValue.Values.Should().Equal(upsertPointSparseVectorValue.Values);
                }
                else if (readPointVectorKind == VectorKind.Dense)
                {
                    var readPointNamedVectorValue =
                        readPointVectors[upsertPointVectors.Key].AsDenseVector().VectorValues;
                    var upsertPointNamedVectorValue = upsertPointVectors.Value.ElementAt(i).AsDenseVector().VectorValues;

                    readPointNamedVectorValue.Should().Equal(upsertPointNamedVectorValue);
                }
                else if (readPointVectorKind == VectorKind.Multi)
                {
                    var readPointNamedMultiVectorValue = readPointVectors[upsertPointVectors.Key].AsMultiVector();

                    readPointNamedMultiVectorValue.Vectors.Should()
                        .BeEquivalentTo(upsertPointVectors.Value.ElementAt(i).AsMultiVector().Vectors);
                }
                else
                {
                    Assert.Fail($"Unexpected vector kind {readPointVectors[upsertPointVectors.Key].VectorKind}");
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
                payload: (TestPayload)1);

        UpsertPointsRequest.UpsertPoint secondPoint = new(
            id: 2,
            vector: new MultiVector()
            {
                Vectors = CreateTestMultivector(vectorLength, 3, VectorDataType.Float16)
            },
            payload: (TestPayload)2);

        // when we have a collection configured for multivectors single vectors get
        // automatically converted to multivectors

        UpsertPointsRequest.UpsertPoint thirdPoint = new(
            id: 3,
            vector: new DenseVector(CreateTestVector(vectorLength, VectorDataType.Float16)),
            payload: (TestPayload)3);

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
    public async Task UpsertPoints_By_Batch_MultiVectors_MixedMultiAndSimple()
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

        PointId[] testPointIds = [1, 2, 3];

        TestPayload[] testPayloads = [1, 2, 3];

        VectorBase[] testVectors = [
            CreateTestMultivector(vectorLength, 2, VectorDataType.Float16),
            CreateTestMultivector(vectorLength, 3, VectorDataType.Float16), 
            CreateTestMultivector(vectorLength, 1, VectorDataType.Float16)]; // it is a simple dense vector

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Batch = new UpsertPointsRequest.UpsertPointsBatch(
                        testPointIds, 
                        new UpsertPointsRequest.UpsertPointsBatch.VectorsBatch(testVectors),
                        testPayloads)
                },
                CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            testPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        upsertPointsResult.Result.Status.Should()
            .BeOneOf(QdrantOperationStatus.Completed);

        readPointsResult.Status.IsSuccess.Should().BeTrue();
        readPointsResult.Result.Length.Should().Be(testPointIds.Length);

        for (int i = 0; i < testPointIds.Length; i++)
        {
            var pointId = testPointIds[i];
            var vector = testVectors[i];
    
            var readPoint =
                readPointsResult.Result.Single(p => p.Id.Equals(pointId));

            if (vector.VectorKind == VectorKind.Multi)
            {
                readPoint.Vector.VectorKind.Should().Be(VectorKind.Multi);

                var readPointVector = readPoint.Vector.AsMultiVector();

                readPointVector.Vectors.Should().BeEquivalentTo(vector.AsMultiVector().Vectors);
            }
            else if (vector.VectorKind == VectorKind.Dense)
            {
                // readPoint is read as single component vector multivector (!) so compare Default vector
                readPoint.Vector.VectorKind.Should().Be(VectorKind.Multi);
                var readPointVector = readPoint.Vector.AsMultiVector();

                readPointVector.Default.AsDenseVector().VectorValues
                    .Should().BeEquivalentTo(vector.AsDenseVector().VectorValues);
            }
            else
            {
                Assert.Fail($"Unexpected vector kind {vector.VectorKind}");
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

        List<PointId> pointIdsToDelete = [.. pointsToDelete.Select(p => p.Id)];

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

            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);
        }
    }
}
