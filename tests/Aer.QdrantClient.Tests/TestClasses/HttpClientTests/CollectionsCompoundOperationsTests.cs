using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Configuration;
using Aer.QdrantClient.Http.DependencyInjection;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal class CollectionsCompoundOperationsTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;
    private QdrantClientSettings _qdrantClientSettings;
    private ILogger _logger;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();

        _qdrantHttpClient = ServiceProvider.GetRequiredService<QdrantHttpClient>();

        _qdrantClientSettings = GetQdrantClientSettings(ServiceCollectionExtensions.DefaultQdrantHttpClientName);

        _logger = ServiceProvider.GetRequiredService<ILogger<CollectionsCompoundOperationsTests>>();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage();
    }

    [Test]
    public async Task ListCollectionInfo()
    {
        var vectorSize = 10U;
        var vectorCount = 10;

        var collectionCreationResult1 = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                ReplicationFactor = 1
            },
            CancellationToken.None);

        var collectionCreationResult2 = await _qdrantHttpClient.CreateCollection(
            TestCollectionName2,
            new CreateCollectionRequest(VectorDistanceMetric.Cosine, vectorSize, isServeVectorsFromDisk: false)
            {
                OnDiskPayload = true,
                ReplicationFactor = 3
            },
            CancellationToken.None);

        collectionCreationResult1.EnsureSuccess();
        collectionCreationResult2.EnsureSuccess();

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

        var upsertPointsResult1
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None,
                isWaitForResult: true,
                ordering: OrderingType.Strong);

        var upsertPointsResult2
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None,
                isWaitForResult: true,
                ordering: OrderingType.Strong);

        upsertPointsResult1.EnsureSuccess();
        upsertPointsResult2.EnsureSuccess();

        List<Task> collectionReadyTasks = [
            _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None),
            _qdrantHttpClient.EnsureCollectionReady(TestCollectionName2, CancellationToken.None)
        ];

        await Task.WhenAll(collectionReadyTasks);

        // call both compound collection info operations

        var listCollectionInfoResult =
            await _qdrantHttpClient.ListCollectionInfo(isCountExactPointsNumber: true, CancellationToken.None);

        var firstCollectionInfoResult = await _qdrantHttpClient.GetCollectionInfo(
            TestCollectionName,
            isCountExactPointsNumber: true,
            CancellationToken.None);

        var secondCollectionInfoResult = await _qdrantHttpClient.GetCollectionInfo(
            TestCollectionName2,
            isCountExactPointsNumber: true,
            CancellationToken.None);

        firstCollectionInfoResult.Status.IsSuccess.Should().BeTrue();
        secondCollectionInfoResult.Status.IsSuccess.Should().BeTrue();

        listCollectionInfoResult.Status.IsSuccess.Should().BeTrue();

        listCollectionInfoResult.Result.Should().HaveCount(2);
        listCollectionInfoResult.Result.Should().ContainKey(TestCollectionName);
        listCollectionInfoResult.Result.Should().ContainKey(TestCollectionName2);

        listCollectionInfoResult.Result[TestCollectionName].Should().BeEquivalentTo(firstCollectionInfoResult.Result);
        listCollectionInfoResult.Result[TestCollectionName2].Should().BeEquivalentTo(secondCollectionInfoResult.Result);
    }

    [Test]
    public async Task StartCreatingCollectionPayloadIndexes()
    {
        var qdrantClient =
            new QdrantHttpClient(
                new Uri(_qdrantClientSettings.HttpAddress),
                apiKey: _qdrantClientSettings.ApiKey,
                logger: _logger,
                disableTracing: true);

        await qdrantClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        qdrantClient.StartCreatingCollectionPayloadIndexes(
            TestCollectionName,
            [
                new CollectionPayloadIndexDefinition(
                    TestPayloadFieldName,
                    PayloadIndexedFieldType.Keyword,
                    isTenant: true),

                new CollectionPayloadIndexDefinition(
                    TestPayloadFieldName2,
                    PayloadIndexedFieldType.Integer,
                    onDisk: true,
                    isPrincipal: true)
            ]
        );

        await Task.Delay(TimeSpan.FromMilliseconds(1000));

        await qdrantClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var collectionInfo = await qdrantClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        collectionInfo.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionInfo.Status.IsSuccess.Should().BeTrue();

        collectionInfo.Result.PayloadSchema.Count.Should().Be(2);
        collectionInfo.Result.PayloadSchema.Should().ContainKey(TestPayloadFieldName);
        collectionInfo.Result.PayloadSchema.Should().ContainKey(TestPayloadFieldName2);

        var firstPayloadSchema = collectionInfo.Result.PayloadSchema[TestPayloadFieldName];

        firstPayloadSchema.DataType.Should().Be(PayloadIndexedFieldType.Keyword);
        firstPayloadSchema.Params.IsTenant.Should().BeTrue();

        var secondPayloadSchema = collectionInfo.Result.PayloadSchema[TestPayloadFieldName2];

        secondPayloadSchema.DataType.Should().Be(PayloadIndexedFieldType.Integer);
        secondPayloadSchema.Params.OnDisk.Should().BeTrue();
        secondPayloadSchema.Params.IsPrincipal.Should().BeTrue();
    }
}
