using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

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
    public async Task CreateIndex_CollectionDoesNotExist()
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
    public async Task CreateIndex_OneField()
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
    public async Task CreateIndex_CheckVectorsNonZero()
    {
        var vectorCount = 100;
        var vectorSize = 10U;
        
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
        
        upsertPointsResult.Status.IsSuccess.Should().BeTrue();
        
        // Enable HNSW index
        
        var enableIndexResult = await _qdrantHttpClient.UpdateCollectionParameters(TestCollectionName,
            new UpdateCollectionParametersRequest()
            {
                OptimizersConfig = new(){
                    IndexingThreshold = 1
                }
            },
            CancellationToken.None);

        enableIndexResult.Status.IsSuccess.Should().BeTrue();

        // Enable payload
        
        var createCollectionIndexResult =
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                "integer",
                PayloadIndexedFieldType.Float,
                CancellationToken.None,
                isWaitForResult: true);

        createCollectionIndexResult.Status.IsSuccess.Should().BeTrue();

        // Just in case wait for index to be created
        await Task.Delay(TimeSpan.FromMilliseconds(100)); 

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var collectionInfo = (await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None)).EnsureSuccess();
        collectionInfo.IndexedVectorsCount.Should().Be((uint)vectorCount);
        
        // Check that all vectors are non-zero
        
        var readAllPoints = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            QdrantFilter.Empty, 
            PayloadPropertiesSelector.All,
            limit: (uint)vectorCount,
            withVector: true,
            cancellationToken: CancellationToken.None);
        
        readAllPoints.Status.IsSuccess.Should().BeTrue();
        readAllPoints.Result.Points.Length.Should().Be(vectorCount);
        
        foreach(var readPoint in readAllPoints.Result.Points)
        {
            readPoint.Vector.Should().NotBeNull();
            var isWholeVectorZero = true;
            
            foreach (var vectorComponent in readPoint.Vector.AsDenseVector().VectorValues)
            {
                isWholeVectorZero &= vectorComponent == 0;
            }

            isWholeVectorZero.Should().BeFalse();
        }
    }

    [Test]
    public async Task CreateFulltextIndex()
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
                FullTextIndexTokenizerType.Prefix,
                CancellationToken.None,
                minimalTokenLength: 3,
                maximalTokenLength: 100,
                
                isLowercasePayloadTokens: true,
                isWaitForResult: true,
                onDisk: true);

        var createCollectionPhraseIndexResult =
            await _qdrantHttpClient.CreateFullTextPayloadIndex(
                TestCollectionName,
                TestPayloadFieldName2,
                FullTextIndexTokenizerType.Word,
                CancellationToken.None,
                minimalTokenLength: 3,
                maximalTokenLength: 100,

                isLowercasePayloadTokens: false,
                isWaitForResult: true,
                onDisk: true,
                enablePhraseMatching: true);

        createCollectionIndexResult.Status.IsSuccess.Should().BeTrue();
        createCollectionPhraseIndexResult.Status.IsSuccess.Should().BeTrue();

        createCollectionIndexResult.Result.Should().NotBeNull();
        createCollectionPhraseIndexResult.Result.Should().NotBeNull();

        var collectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        collectionInfo.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionInfo.Status.IsSuccess.Should().BeTrue();

        collectionInfo.Result.PayloadSchema.Count.Should().Be(2);
        
        collectionInfo.Result.PayloadSchema.Should().ContainKey(TestPayloadFieldName);
        collectionInfo.Result.PayloadSchema.Should().ContainKey(TestPayloadFieldName2);

        collectionInfo.Result.PayloadSchema[TestPayloadFieldName].DataType.Should().Be(PayloadIndexedFieldType.Text);
        collectionInfo.Result.PayloadSchema[TestPayloadFieldName].Params.OnDisk.Should().Be(true);
        collectionInfo.Result.PayloadSchema[TestPayloadFieldName].Params.Tokenizer.Should().Be(FullTextIndexTokenizerType.Prefix);
        collectionInfo.Result.PayloadSchema[TestPayloadFieldName].Params.Lowercase.Should().Be(true);
        collectionInfo.Result.PayloadSchema[TestPayloadFieldName].Params.PhraseMatching.Should().Be(false);
        
        collectionInfo.Result.PayloadSchema[TestPayloadFieldName2].DataType.Should().Be(PayloadIndexedFieldType.Text);
        collectionInfo.Result.PayloadSchema[TestPayloadFieldName2].Params.OnDisk.Should().Be(true);
        collectionInfo.Result.PayloadSchema[TestPayloadFieldName2].Params.Tokenizer.Should().Be(FullTextIndexTokenizerType.Word);
        collectionInfo.Result.PayloadSchema[TestPayloadFieldName2].Params.Lowercase.Should().Be(false);
        collectionInfo.Result.PayloadSchema[TestPayloadFieldName2].Params.PhraseMatching.Should().Be(true);
    }

    [Test]
    public async Task CreateIndex_OnDisk()
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
    public async Task CreateIndex_Tenant()
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
        collectionInfo.PayloadSchema[TestPayloadFieldName].Params.OnDisk.Should().BeFalse();
        collectionInfo.PayloadSchema[TestPayloadFieldName].Params.IsTenant.Should().BeTrue();

        collectionInfo.PayloadSchema[TestPayloadFieldName2].DataType.Should().Be(PayloadIndexedFieldType.Integer);
        collectionInfo.PayloadSchema[TestPayloadFieldName2].Params.OnDisk.Should().BeFalse();
        collectionInfo.PayloadSchema[TestPayloadFieldName2].Params.IsTenant.Should().BeFalse();
    }

    [Test]
    public async Task CreateIndex_RestrictedTypes()
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

        var createCollectionPrincipalIndexAct = async ()=>
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                PayloadIndexedFieldType.Keyword,
                CancellationToken.None,
                isWaitForResult: true,
                isPrincipal: true);

        var createCollectionTenantIndexAct = async () =>
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                TestPayloadFieldName,
                PayloadIndexedFieldType.Integer,
                CancellationToken.None,
                isWaitForResult: true,
                isTenant: true);

        await createCollectionPrincipalIndexAct.Should().ThrowAsync<QdrantUnsupportedFieldSchemaForIndexConfiguration>()
            .Where(e => e.Message.Contains("Principal"));

        await createCollectionTenantIndexAct.Should().ThrowAsync<QdrantUnsupportedFieldSchemaForIndexConfiguration>()
            .Where(e => e.Message.Contains("Tenant"));
    }

    [Test]
    public async Task CreateIndex_Principal()
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
    public async Task CreateIndex_TwoFields()
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
    public async Task CreateIndex_TwoIdenticalFields()
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
    public async Task CreateIndex_TwoIdenticalFields_DifferentTypes()
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
    public async Task DeleteIndex_CollectionDoesNotExist()
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
    public async Task DeleteIndex_IndexDoesNotExist()
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
    public async Task DeleteIndex_OneFieldOneDelete()
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
    public async Task DeleteIndex_TwoFieldsOneDelete()
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
