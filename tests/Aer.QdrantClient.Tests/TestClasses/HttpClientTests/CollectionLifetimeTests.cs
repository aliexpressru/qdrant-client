using Aer.QdrantClient.Http;
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
    public async Task TestCreateCollection_InvalidCollectionName()
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

        await createCollectionAct.Should().ThrowAsync<InvalidOperationException>();

        await createCollectionAct2.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task TestCreateCollection()
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
    }

    [Test]
    public async Task TestCreateCollection_WithSparseVectors()
    {
        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                SparseVectors = new Dictionary<string, SparseVectorConfiguration>(){
                    ["test"] = new (onDisk: true, fullScanThreshold: 5000)
                }
            },
            CancellationToken.None);

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();
    }

    [Test]
    public async Task TestCreateCollection_SameNamedVectors()
    {
        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                100,
                isServeVectorsFromDisk: true,
                CreateVectorNames(3))
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
    public async Task TestCreateCollection_DifferentNamedVectors()
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

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();
    }

    [Test]
    public async Task TestCreateCollection_Delete_RecreateSameCollection()
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
    public async Task TestCreateCollection_InitFrom()
    {
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
            new UpsertPointsRequest<TestPayload>()
            {
                Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
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
                InitFrom = CreateCollectionRequest.InitFromCollection.ByName(TestCollectionName)
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

        readPointsResult.Result.Id.ToJson().Should().Be(testPointId.ToJson());
        readPointsResult.Result.Vector.Default.Should().BeEquivalentTo(testVector);
        var readTestPayload = readPointsResult.Result.Payload.As<TestPayload>();

        readTestPayload.Integer.Should().Be(testPayload.Integer);
        readTestPayload.FloatingPointNumber.Should().Be(testPayload.FloatingPointNumber);
        readTestPayload.Text.Should().Be(testPayload.Text);
    }

    [Test]
    public async Task TestCreateCollection_CollectionAlreadyExists()
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
    public async Task TestDeleteCollection()
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
    public async Task TestDeleteCollection_DoubleDeleteSameCollection()
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

    [Test]
    public async Task TestUpdateCollectionParameters_CollectionDoesNotExist()
    {
        var updateNoCollectionParametersResult = await _qdrantHttpClient.UpdateCollectionParameters(
            TestCollectionName,
            new UpdateCollectionParametersRequest()
            {
                OptimizersConfig = new()
                {
                    MemmapThreshold = 1
                }
            },
            CancellationToken.None);

        updateNoCollectionParametersResult.Status.Type.Should().Be(QdrantOperationStatusType.Error);
        updateNoCollectionParametersResult.Status.IsSuccess.Should().BeFalse();
        updateNoCollectionParametersResult.Status.Error.Should()
            .Contain(TestCollectionName).And
            .Contain("doesn't exist");

        updateNoCollectionParametersResult.Result.Should().BeNull();
    }

    [Test]
    public async Task TestUpdateCollectionParameters()
    {
        // create collection first
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                OptimizersConfig = new OptimizersConfiguration()
                {
                    MemmapThreshold = 1000
                }
            },
            CancellationToken.None);

        const int newMemmapThreshold = 1;

        var updateNoCollectionParametersResult = await _qdrantHttpClient.UpdateCollectionParameters(
            TestCollectionName,
            new UpdateCollectionParametersRequest()
            {
                OptimizersConfig = new()
                {
                    MemmapThreshold = newMemmapThreshold,
                }
            },
            CancellationToken.None);

        var collectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        updateNoCollectionParametersResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        updateNoCollectionParametersResult.Status.IsSuccess.Should().BeTrue();

        updateNoCollectionParametersResult.Result.Should().NotBeNull();

        collectionInfo.Result.Status.Should().Be(QdrantCollectionStatus.Green);
        collectionInfo.Result.OptimizerStatus.IsOk.Should().BeTrue();
        collectionInfo.Result.Config.OptimizerConfig.MemmapThreshold.Should().Be(newMemmapThreshold);
    }

    [Test]
    public async Task TestListCollections_EmptyCollectionList()
    {
        var existingCollections = await _qdrantHttpClient.ListCollections(CancellationToken.None);

        existingCollections.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        existingCollections.Status.IsSuccess.Should().BeTrue();

        existingCollections.Result.Should().NotBeNull();
        existingCollections.Result.Collections.Should().BeEmpty();
    }

    [Test]
    public async Task TestListCollections()
    {
        // create collection
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var oneExistingCollection = await _qdrantHttpClient.ListCollections(CancellationToken.None);

        oneExistingCollection.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        oneExistingCollection.Status.IsSuccess.Should().BeTrue();

        oneExistingCollection.Result.Should().NotBeNull();
        oneExistingCollection.Result.Collections.Length.Should().Be(1);
        oneExistingCollection.Result.Collections[0].Name.Should().Be(TestCollectionName);

        // create second collection
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName2,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var twoExistingCollections = await _qdrantHttpClient.ListCollections(CancellationToken.None);

        twoExistingCollections.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        twoExistingCollections.Status.IsSuccess.Should().BeTrue();

        twoExistingCollections.Result.Should().NotBeNull();
        twoExistingCollections.Result.Collections.Length.Should().Be(2);

        twoExistingCollections.Result.Collections
            .SingleOrDefault(c => c.Name.Equals(TestCollectionName)).Should().NotBeNull();
        twoExistingCollections.Result.Collections
            .SingleOrDefault(c => c.Name.Equals(TestCollectionName2)).Should().NotBeNull();
    }

    [Test]
    public async Task TestGetCollectionInfo_NoCollection()
    {
        var nonExistentCollectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        nonExistentCollectionInfo.Status.Type.Should().Be(QdrantOperationStatusType.Error);
        nonExistentCollectionInfo.Status.IsSuccess.Should().BeFalse();
        nonExistentCollectionInfo.Status.Error.Should()
            .Contain("doesn't exist").And
            .Contain(TestCollectionName);

        nonExistentCollectionInfo.Result.Should().BeNull();
    }

    [Test]
    public async Task TestGetCollectionInfo()
    {
        // create collection
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var collectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        collectionInfo.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionInfo.Status.IsSuccess.Should().BeTrue();

        collectionInfo.Result.Should().NotBeNull();
        collectionInfo.Result.Status.Should().Be(QdrantCollectionStatus.Green);

        collectionInfo.Result.OptimizerStatus.IsOk.Should().BeTrue();
        collectionInfo.Result.OptimizerStatus.Status.Should().Be(QdrantOptimizerStatus.Ok);

        collectionInfo.Result.Config.Params.OnDiskPayload.Should().BeTrue();
    }
}
