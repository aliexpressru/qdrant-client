using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Helpers;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal class PointsBatchCrudTests : QdrantTestsBase
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
    public async Task BatchUpdatePoints_EmptyRequest()
    {
        var batchUpdateAct = async ()=> await _qdrantHttpClient.BatchUpdate(
            TestCollectionName,
            BatchUpdatePointsRequest.Create(),
            CancellationToken.None);

        await batchUpdateAct.Should().ThrowAsync<QdrantEmptyBatchRequestException>();
    }

    [Test]
    public async Task BatchUpdatePoints_Upsert()
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

        var batchUpdateRequest = BatchUpdatePointsRequest.Create()
            .UpsertPoints(upsertPoints);

        var batchUpdateResponse = await _qdrantHttpClient.BatchUpdate(
            TestCollectionName,
            batchUpdateRequest,
            CancellationToken.None);

        batchUpdateResponse.Status.IsSuccess.Should().BeTrue();
        batchUpdateResponse.Result.Length.Should().Be(batchUpdateRequest.OperationsCount);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPoints.Select(p => p.Id),
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        readAllPoints.Status.IsSuccess.Should().BeTrue();
        readAllPoints.Result.Length.Should().Be(upsertPoints.Count);
    }

    [Test]
    public async Task BatchUpdatePoints_DoubleUpsert()
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

        var batchUpdateRequest = BatchUpdatePointsRequest.Create()
            .UpsertPoints(upsertPoints.Take(5))
            .UpsertPoints(upsertPoints.Skip(5).Take(5));

        var batchUpdateResponse = await _qdrantHttpClient.BatchUpdate(
            TestCollectionName,
            batchUpdateRequest,
            CancellationToken.None);

        batchUpdateResponse.Status.IsSuccess.Should().BeTrue();
        batchUpdateResponse.Result.Length.Should().Be(batchUpdateRequest.OperationsCount);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPoints.Select(p => p.Id),
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        readAllPoints.Status.IsSuccess.Should().BeTrue();
        readAllPoints.Result.Length.Should().Be(upsertPoints.Count);
    }

    [Test]
    public async Task BatchUpdatePoints_UpsertDelete()
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

        var pointsToDelete = upsertPoints.Select(u => u.Id).Take(5).ToHashSet();

        var batchUpdateRequest = BatchUpdatePointsRequest.Create()
            .UpsertPoints(upsertPoints)
            .DeletePoints(pointsToDelete);

        var batchUpdateResponse = await _qdrantHttpClient.BatchUpdate(
            TestCollectionName,
            batchUpdateRequest,
            CancellationToken.None);

        batchUpdateResponse.Status.IsSuccess.Should().BeTrue();
        batchUpdateResponse.Result.Length.Should().Be(batchUpdateRequest.OperationsCount);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPoints.Select(p => p.Id),
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        readAllPoints.Status.IsSuccess.Should().BeTrue();
        readAllPoints.Result.Length.Should().Be(upsertPoints.Count - pointsToDelete.Count);
    }

    [Test]
    public async Task BatchUpdatePoints_UpsertDelete_ModifyPayload_UpdateVector()
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
                    new TestPayload()
                    {
                        Integer = i+1,
                        Text = (i+1).ToString()
                    }
                )
            );
        }

        var pointsToDelete = upsertPoints.Select(u => u.Id).Take(5).ToHashSet();
        var pointToSetPayloadFor = upsertPoints.Select(u => u.Id).Skip(5).First();
        var pointToOverwritePayloadFor = upsertPoints.Select(u => u.Id).Skip(6).First();
        var pointToDeletePayloadKeysFor = upsertPoints.Select(u => u.Id).Skip(7).First();
        var pointToClearPayloadFor = upsertPoints.Select(u => u.Id).Skip(8).First();
        var pointToUpdateVectorFor = upsertPoints.Select(u => u.Id).Skip(9).First();

        var vectorToUpdateTo = CreateConstantTestVector(1.1f, vectorSize);

        var batchUpdateRequest = BatchUpdatePointsRequest.Create()
            .UpsertPoints(upsertPoints)
            .DeletePoints(pointsToDelete)
            .SetPointsPayload(
                new TestPayload() {Integer = 100},
                pointToSetPayloadFor.YieldSingle()
            )
            .OverwritePointsPayload(
                new TestPayload() {Text = "Test"},
                pointToOverwritePayloadFor.YieldSingle()
            )
            .DeletePointsPayloadKeys(
                Q<TestPayload>.GetPayloadFieldName(p => p.Integer).YieldSingle(),
                pointToDeletePayloadKeysFor.YieldSingle()
            )
            .ClearPointsPayload(pointToClearPayloadFor.YieldSingle())
            .UpdatePointsVectors(
                new PointVector()
                {
                    Id = pointToUpdateVectorFor,
                    Vector = vectorToUpdateTo
                }.YieldSingle().ToArray()
            );

        var batchUpdateResponse = await _qdrantHttpClient.BatchUpdate(
            TestCollectionName,
            batchUpdateRequest,
            CancellationToken.None);

        batchUpdateResponse.Status.IsSuccess.Should().BeTrue();
        batchUpdateResponse.Result.Length.Should().Be(batchUpdateRequest.OperationsCount);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPoints.Select(p => p.Id),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        readAllPoints.Status.IsSuccess.Should().BeTrue();
        readAllPoints.Result.Length.Should().Be(upsertPoints.Count - pointsToDelete.Count);

        readAllPoints.Result.Single(p => p.Id.Equals(pointToSetPayloadFor))
            .Payload.As<TestPayload>().Integer.Should().Be(100);

        readAllPoints.Result.Single(p => p.Id.Equals(pointToOverwritePayloadFor))
            .Payload.As<TestPayload>().Integer.Should().BeNull();
        readAllPoints.Result.Single(p => p.Id.Equals(pointToOverwritePayloadFor))
            .Payload.As<TestPayload>().Text.Should().Be("Test");

        readAllPoints.Result.Single(p => p.Id.Equals(pointToDeletePayloadKeysFor))
            .Payload.As<TestPayload>().Integer.Should().BeNull();
        readAllPoints.Result.Single(p => p.Id.Equals(pointToDeletePayloadKeysFor))
            .Payload.As<TestPayload>().Text.Should().NotBeNull();

        readAllPoints.Result.Single(p => p.Id.Equals(pointToClearPayloadFor))
            .Payload.Should().BeNull();

        readAllPoints.Result.Single(p => p.Id.Equals(pointToUpdateVectorFor))
            .Vector.Default.AsDenseVector().VectorValues
            .Should().BeEquivalentTo(vectorToUpdateTo);
    }

    [Test]
    public async Task BatchUpdatePoints_Upsert_DeleteVector()
    {
        var vectorSize = 10U;
        var vectorCount = 5;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                namedVectorNames: CreateVectorNames(2),
                isServeVectorsFromDisk: true)
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
                    CreateTestNamedVectors(vectorSize, 2),
                    new TestPayload()
                    {
                        Integer = i + 1,
                        Text = (i + 1).ToString()
                    }
                )
            );
        }

        var pointToDeleteVectorFor = upsertPoints.Select(u => u.Id).Skip(1).First();
        var pointToDeleteVectorForVectorName = upsertPoints.Select(u => u.Vector).Skip(1).First()
            .AsNamedVectors().Vectors.Keys.First();

        var batchUpdateRequest = BatchUpdatePointsRequest.Create()
            .UpsertPoints(upsertPoints)
            .DeletePointsVectors(
                pointToDeleteVectorForVectorName.YieldSingle(),
                pointToDeleteVectorFor.YieldSingle()
            );

        var batchUpdateResponse = await _qdrantHttpClient.BatchUpdate(
            TestCollectionName,
            batchUpdateRequest,
            CancellationToken.None);

        batchUpdateResponse.Status.IsSuccess.Should().BeTrue();
        batchUpdateResponse.Result.Length.Should().Be(batchUpdateRequest.OperationsCount);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPoints.Select(p => p.Id),
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        readAllPoints.Status.IsSuccess.Should().BeTrue();
        readAllPoints.Result.Length.Should().Be(upsertPoints.Count);

        readAllPoints.Result.Single(p => p.Id.Equals(pointToDeleteVectorFor))
            .Vector.AsNamedVectors().Vectors.ContainsKey(pointToDeleteVectorForVectorName).Should().BeFalse();
    }
}
