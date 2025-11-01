using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal class CollectionLifetimeTests : QdrantTestsBase
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
    public async Task CreateCollection_InvalidCollectionName()
    {
        var createCollectionAct = async () => await _qdrantHttpClient.CreateCollection(
            TestCollectionName + "/" + "aaa", // invalid collection name
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var createCollectionAct2 = async () => await _qdrantHttpClient.CreateCollection(
            null, // invalid collection name
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        await createCollectionAct.Should().ThrowAsync<QdrantInvalidEntityNameException>();

        await createCollectionAct2.Should().ThrowAsync<QdrantInvalidEntityNameException>();
    }

    [Test]
    public async Task CheckCollectionExists()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName2,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        // check non-existent collection first

        var collectionDoesNoExistResult =
            await _qdrantHttpClient.CheckCollectionExists(TestCollectionName, CancellationToken.None);

        collectionDoesNoExistResult.Status.IsSuccess.Should().BeTrue();

        collectionDoesNoExistResult.Result.Exists.Should().BeFalse();

        // check whether the created collection exists

        var collectionExistsResult =
            await _qdrantHttpClient.CheckCollectionExists(TestCollectionName2, CancellationToken.None);

        collectionExistsResult.Status.IsSuccess.Should().BeTrue();

        collectionExistsResult.Result.Exists.Should().BeTrue();
    }

    [Test]
    public async Task CreateCollection_Delete_RecreateSameCollection()
    {
        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();

        var collectionDeletionResult = await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);

        collectionDeletionResult.EnsureSuccess();

        collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();
    }

    [Test]
    public async Task CreateCollection_InitFrom()
    {
        OnlyIfVersionBefore("1.16.0", "init_from parameter is removed in 1.16.0");
        
        var vectorSize = 10U;

        var createCollectionResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        createCollectionResult.EnsureSuccess();

        var testPointId = PointId.Integer(1);
        var testVector = CreateTestVector(vectorSize);
        TestPayload testPayload = "test";

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

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var copyCollectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName2,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
#pragma warning disable CS0618 // Type or member is obsolete
                InitFrom = CreateCollectionRequest.InitFromCollection.ByName(TestCollectionName)
#pragma warning restore CS0618 // Type or member is obsolete
            },
            CancellationToken.None);

        copyCollectionCreationResult.EnsureSuccess();

        await Task.Delay(TimeSpan.FromMilliseconds(500));

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName2, CancellationToken.None);

        var readPointsResult = await _qdrantHttpClient.GetPoint(
            TestCollectionName2,
            testPointId,
            CancellationToken.None);

        copyCollectionCreationResult.Status.IsSuccess.Should().BeTrue();

        copyCollectionCreationResult.Should().NotBeNull();
        copyCollectionCreationResult.Result.Should().BeTrue();

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
    public async Task CreateCollection_CollectionAlreadyExists()
    {
        // create collection once
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var collectionDoubleCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionDoubleCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Error);
        collectionDoubleCreationResult.Status.IsSuccess.Should().BeFalse();

        collectionDoubleCreationResult.Status.Error.Should()
            .Contain("already exists")
            .And.Contain(TestCollectionName);

        collectionDoubleCreationResult.Should().NotBeNull();
        collectionDoubleCreationResult.Result.Should().BeNull();
    }

    [Test]
    public async Task DeleteCollection()
    {
        // create collection first
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var collectionDeletionResult = await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);

        collectionDeletionResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionDeletionResult.Status.IsSuccess.Should().BeTrue();

        collectionDeletionResult.Should().NotBeNull();
        collectionDeletionResult.Result.Should().BeTrue();
    }

    [Test]
    public async Task DeleteCollection_DoubleDeleteSameCollection()
    {
        // create collection first
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        // delete collection once
        await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);

        // delete same collection
        var collectionDoubleDeletionResult = await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);

        collectionDoubleDeletionResult.Status.IsSuccess.Should().BeTrue();

        collectionDoubleDeletionResult.Should().NotBeNull();
        collectionDoubleDeletionResult.Result.Should().BeFalse();
    }
}
