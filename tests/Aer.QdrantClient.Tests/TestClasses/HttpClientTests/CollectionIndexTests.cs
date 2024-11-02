using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal class CollectionIndexTests : QdrantTestsBase
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
    public async Task TestCreateIndex_CollectionDoesNotExist()
    {
        var createNonExistentCollectionIndexResult =
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                "whatever",
                PayloadIndexedFieldType.Float,
                CancellationToken.None,
                isWaitForResult: true);

        createNonExistentCollectionIndexResult.Status.IsSuccess.Should().BeFalse();
        createNonExistentCollectionIndexResult.Status.Type.Should().Be(QdrantOperationStatusType.Error);
        createNonExistentCollectionIndexResult.Status.Error
            .Should().Contain(TestCollectionName)
            .And.Contain("doesn't exist");
    }

    [Test]
    public async Task TestCreateIndex_OneField()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var createCollectionIndexResult =
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                PayloadIndexedFieldType.Float,
                CancellationToken.None,
                isWaitForResult: true);

        createCollectionIndexResult.Status.IsSuccess.Should().BeTrue();

        createCollectionIndexResult.Result.Should().NotBeNull();

        var collectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        collectionInfo.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionInfo.Status.IsSuccess.Should().BeTrue();

        collectionInfo.Result.PayloadSchema.Count.Should().Be(1);
        collectionInfo.Result.PayloadSchema.Should().ContainKey(TestPayloadFieldName);

        collectionInfo.Result.PayloadSchema[TestPayloadFieldName].DataType.Should().Be(PayloadIndexedFieldType.Float);
    }

    [Test]
    public async Task TestCreateFulltextIndex_OneField()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var createCollectionIndexResult =
            await _qdrantHttpClient.CreateFullTextPayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                PayloadIndexedTextFieldTokenizerType.Word,
                3,
                100,
                CancellationToken.None,
                isWaitForResult: true);

        createCollectionIndexResult.Status.IsSuccess.Should().BeTrue();

        createCollectionIndexResult.Result.Should().NotBeNull();

        var collectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        collectionInfo.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionInfo.Status.IsSuccess.Should().BeTrue();

        collectionInfo.Result.PayloadSchema.Count.Should().Be(1);
        collectionInfo.Result.PayloadSchema.Should().ContainKey(TestPayloadFieldName);

        collectionInfo.Result.PayloadSchema[TestPayloadFieldName].DataType.Should().Be(PayloadIndexedFieldType.Text);
    }

    [Test]
    public async Task TestCreateIndex_OnDisk()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var createCollectionIndexResult =
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                PayloadIndexedFieldType.Uuid,
                CancellationToken.None,
                isWaitForResult: true,
                onDisk: true);

        createCollectionIndexResult.Status.IsSuccess.Should().BeTrue();
        createCollectionIndexResult.Result.Should().NotBeNull();

        var collectionInfo = (await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        collectionInfo.PayloadSchema.Count.Should().Be(1);
        collectionInfo.PayloadSchema.Should().ContainKey(TestPayloadFieldName);

        collectionInfo.PayloadSchema[TestPayloadFieldName].DataType.Should().Be(PayloadIndexedFieldType.Uuid);
        collectionInfo.PayloadSchema[TestPayloadFieldName].Params.OnDisk.Should().Be(true);
    }

    [Test]
    public async Task TestCreateIndex_MultiTenant()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                HnswConfig = new HnswConfiguration()
                {
                    PayloadM = 16,
                    M = 0
                }
            },
            CancellationToken.None);

        var createCollectionTenantIndexResult =
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                PayloadIndexedFieldType.Keyword,
                CancellationToken.None,
                isWaitForResult: true,
                isTenant: true);

        var createCollectionIndexResult =
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName2,
                PayloadIndexedFieldType.Integer,
                CancellationToken.None,
                isWaitForResult: true);

        createCollectionTenantIndexResult.Status.IsSuccess.Should().BeTrue();
        createCollectionTenantIndexResult.Result.Should().NotBeNull();

        createCollectionIndexResult.Status.IsSuccess.Should().BeTrue();
        createCollectionIndexResult.Result.Should().NotBeNull();

        var collectionInfo =
            (await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        collectionInfo.PayloadSchema.Count.Should().Be(2);
        collectionInfo.PayloadSchema.Should()
            .ContainKey(TestPayloadFieldName)
            .And.ContainKey(TestPayloadFieldName2);

        collectionInfo.PayloadSchema[TestPayloadFieldName].DataType.Should().Be(PayloadIndexedFieldType.Keyword);
        collectionInfo.PayloadSchema[TestPayloadFieldName].Params.OnDisk.Should().Be(false);
        collectionInfo.PayloadSchema[TestPayloadFieldName].Params.IsTenant.Should().Be(true);

        collectionInfo.PayloadSchema[TestPayloadFieldName2].DataType.Should().Be(PayloadIndexedFieldType.Integer);
        collectionInfo.PayloadSchema[TestPayloadFieldName2].Params.OnDisk.Should().Be(false);
        collectionInfo.PayloadSchema[TestPayloadFieldName2].Params.IsTenant.Should().Be(false);
    }

    [Test]
    public async Task TestCreateIndex_Principal()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                HnswConfig = new HnswConfiguration()
                {
                    PayloadM = 16,
                    M = 0
                }
            },
            CancellationToken.None);

        var createCollectionPrincipalIndexResult =
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                PayloadIndexedFieldType.Integer,
                CancellationToken.None,
                isWaitForResult: true,
                isPrincipal: true);

        createCollectionPrincipalIndexResult.Status.IsSuccess.Should().BeTrue();
        createCollectionPrincipalIndexResult.Result.Should().NotBeNull();

        var collectionInfo =
            (await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        collectionInfo.PayloadSchema.Count.Should().Be(1);
        collectionInfo.PayloadSchema.Should().ContainKey(TestPayloadFieldName);

        collectionInfo.PayloadSchema[TestPayloadFieldName].DataType.Should().Be(PayloadIndexedFieldType.Integer);
        collectionInfo.PayloadSchema[TestPayloadFieldName].Params.OnDisk.Should().Be(false);
        collectionInfo.PayloadSchema[TestPayloadFieldName].Params.IsTenant.Should().Be(false);
        collectionInfo.PayloadSchema[TestPayloadFieldName].Params.IsPrincipal.Should().Be(true);
    }

    [Test]
    public async Task TestCreateIndex_TwoFields()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var createCollectionIndexResult1 =
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                PayloadIndexedFieldType.Float,
                CancellationToken.None,
                isWaitForResult: true);

        var createCollectionIndexResult2 =
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName2,
                PayloadIndexedFieldType.Keyword,
                CancellationToken.None,
                isWaitForResult: true);

        createCollectionIndexResult1.Status.IsSuccess.Should().BeTrue();
        createCollectionIndexResult1.Result.Should().NotBeNull();

        createCollectionIndexResult2.Status.IsSuccess.Should().BeTrue();
        createCollectionIndexResult2.Result.Should().NotBeNull();

        var collectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        collectionInfo.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionInfo.Status.IsSuccess.Should().BeTrue();

        collectionInfo.Result.PayloadSchema.Count.Should().Be(2);
        collectionInfo.Result.PayloadSchema.Should().ContainKey(TestPayloadFieldName);
        collectionInfo.Result.PayloadSchema.Should().ContainKey(TestPayloadFieldName2);

        collectionInfo.Result.PayloadSchema[TestPayloadFieldName].DataType.Should().Be(PayloadIndexedFieldType.Float);
        collectionInfo.Result.PayloadSchema[TestPayloadFieldName2].DataType.Should().Be(PayloadIndexedFieldType.Keyword);
    }

    [Test]
    public async Task TestCreateIndex_TwoIdenticalFields()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var createCollectionIndexResult1 =
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                PayloadIndexedFieldType.Keyword,
                CancellationToken.None,
                isWaitForResult: true);

        var createCollectionIndexResult2 =
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                PayloadIndexedFieldType.Keyword,
                CancellationToken.None,
                isWaitForResult: true);

        createCollectionIndexResult1.Status.IsSuccess.Should().BeTrue();
        createCollectionIndexResult1.Result.Should().NotBeNull();

        createCollectionIndexResult2.Status.IsSuccess.Should().BeTrue();
        createCollectionIndexResult2.Result.Should().NotBeNull();

        var collectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        collectionInfo.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionInfo.Status.IsSuccess.Should().BeTrue();

        collectionInfo.Result.PayloadSchema.Count.Should().Be(1);
        collectionInfo.Result.PayloadSchema.Should().ContainKey(TestPayloadFieldName);

        collectionInfo.Result.PayloadSchema[TestPayloadFieldName].DataType.Should().Be(PayloadIndexedFieldType.Keyword);
    }

    [Test]
    public async Task TestCreateIndex_TwoIdenticalFields_DifferentTypes()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var createCollectionIndexResult1 =
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                PayloadIndexedFieldType.Keyword,
                CancellationToken.None,
                isWaitForResult: true);

        var createCollectionIndexResult2 =
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                PayloadIndexedFieldType.Float,
                CancellationToken.None,
                isWaitForResult: true);

        createCollectionIndexResult1.Status.IsSuccess.Should().BeTrue();
        createCollectionIndexResult1.Result.Should().NotBeNull();

        createCollectionIndexResult2.Status.IsSuccess.Should().BeTrue();
        createCollectionIndexResult2.Result.Should().NotBeNull();

        var collectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        collectionInfo.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionInfo.Status.IsSuccess.Should().BeTrue();
        collectionInfo.Result.PayloadSchema.Count.Should().Be(1);
        collectionInfo.Result.PayloadSchema.Should().ContainKey(TestPayloadFieldName);

        collectionInfo.Result.PayloadSchema[TestPayloadFieldName].DataType.Should().Be(PayloadIndexedFieldType.Float);
    }

    [Test]
    public async Task TestDeleteIndex_CollectionDoesNotExist()
    {
        var deleteNonExistentCollectionIndexResult =
            await _qdrantHttpClient.DeletePayloadIndex(
                TestCollectionName,
                "whatever",
                CancellationToken.None,
                isWaitForResult: true);

        deleteNonExistentCollectionIndexResult.Status.IsSuccess.Should().BeFalse();
        deleteNonExistentCollectionIndexResult.Status.Type.Should().Be(QdrantOperationStatusType.Error);
        deleteNonExistentCollectionIndexResult.Status.Error
            .Should().Contain(TestCollectionName).And
            .Contain("doesn't exist");
    }

    [Test]
    public async Task TestDeleteIndex_IndexDoesNotExist()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var deleteNonExistentFieldIndexResult =
            await _qdrantHttpClient.DeletePayloadIndex(
                TestCollectionName,
                "whatever",
                CancellationToken.None,
                isWaitForResult: true);

        // this is unexpected, but it's the way the API is built. For idempotence reasons I presume.
        deleteNonExistentFieldIndexResult.Status.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task TestDeleteIndex_OneFieldOneDelete()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        await _qdrantHttpClient.CreatePayloadIndex(
            TestCollectionName,
            TestPayloadFieldName,
            PayloadIndexedFieldType.Float,
            CancellationToken.None,
            isWaitForResult: true);

        var deleteFieldIndexResult =
            await _qdrantHttpClient.DeletePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                CancellationToken.None,
                isWaitForResult: true);

        var collectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        deleteFieldIndexResult.Status.IsSuccess.Should().BeTrue();

        collectionInfo.Result.PayloadSchema.Count.Should().Be(0);
    }

    [Test]
    public async Task TestDeleteIndex_TwoFieldsOneDelete()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        await _qdrantHttpClient.CreatePayloadIndex(
            TestCollectionName,
            TestPayloadFieldName,
            PayloadIndexedFieldType.Float,
            CancellationToken.None,
            isWaitForResult: true);

        await _qdrantHttpClient.CreatePayloadIndex(
            TestCollectionName,
            TestPayloadFieldName2,
            PayloadIndexedFieldType.Keyword,
            CancellationToken.None,
            isWaitForResult: true);

        var deleteFieldIndexResult =
            await _qdrantHttpClient.DeletePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                CancellationToken.None,
                isWaitForResult: true);

        var collectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        deleteFieldIndexResult.Status.IsSuccess.Should().BeTrue();

        collectionInfo.Result.PayloadSchema.Count.Should().Be(1);
    }
}
