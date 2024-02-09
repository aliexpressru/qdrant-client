using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class ClusterTests : QdrantTestsBase
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
    public async Task TestClusterInfo()
    {
        var clusterInfo = await _qdrantHttpClient.GetClusterInfo(CancellationToken.None);

        clusterInfo.Status.IsSuccess.Should().BeTrue();
        clusterInfo.Result.Status.Should().Be("disabled");
    }

    [Test]
    public async Task TestGetCollectionClusteringInfo()
    {
        const uint vectorSize = 10;

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

        var collectionClusteringInfo =
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None);

        collectionClusteringInfo.Status.IsSuccess.Should().BeTrue();

        collectionClusteringInfo.Result.PeerId.Should().NotBe(0);
        collectionClusteringInfo.Result.ShardCount.Should().Be(1);
        collectionClusteringInfo.Result.LocalShards.Length.Should().Be(1);

        collectionClusteringInfo.Result.LocalShards[0].ShardId.Should().Be(0);
        collectionClusteringInfo.Result.LocalShards[0].PointsCount.Should().Be(1);
        collectionClusteringInfo.Result.LocalShards[0].State.Should().Be(ShardState.Active);

        collectionClusteringInfo.Result.RemoteShards.Length.Should().Be(0);
        collectionClusteringInfo.Result.ShardTransfers.Length.Should().Be(0);
    }

    //[Test]
    // NOTE: this test works locally only once. Then the docker compose restart and volumes drop required
    // NOTE: this test will work only when cluster is enabled in qdrant-config_node_0.yaml
    public async Task TestCollectionShardMove()
    {
        const uint vectorSize = 10;

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

        var collectionClusteringInfo =
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None);

        var localShardId = collectionClusteringInfo.Result.LocalShards[0].ShardId;
        var localPeerId = collectionClusteringInfo.Result.PeerId;

        var moveSelfResult =
            await _qdrantHttpClient.UpdateCollectionClusteringSetup(
                TestCollectionName,
                UpdateCollectionClusteringSetupRequest.CreateMoveShardRequest(
                    localShardId,
                    localPeerId, // since we don't have more than one node in the cluster - we move shard from one peer to itself
                    localPeerId),
                CancellationToken.None);

        moveSelfResult.EnsureSuccess();

        moveSelfResult.Result.Should().BeTrue();

        var newCollectionClusteringInfo =
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None);

        newCollectionClusteringInfo.Result.ShardTransfers.Length.Should().Be(1);
        newCollectionClusteringInfo.Result.ShardTransfers[0].ShardId.Should().Be(localShardId);
        newCollectionClusteringInfo.Result.ShardTransfers[0].From.Should().Be(localPeerId);
        newCollectionClusteringInfo.Result.ShardTransfers[0].To.Should().Be(localPeerId);
        newCollectionClusteringInfo.Result.ShardTransfers[0].Sync.Should().Be(false);
    }

    //[Test]
    // NOTE: this test works locally only once. Then the docker compose restart and volumes drop required
    // NOTE: this test will work only when cluster is enabled in qdrant-config_node_0.yaml
    public async Task TestCollectionShardReplicate()
    {
        const uint vectorSize = 10;

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

        var collectionClusteringInfo =
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None);

        var localShardId = collectionClusteringInfo.Result.LocalShards[0].ShardId;
        var localPeerId = collectionClusteringInfo.Result.PeerId;

        var replicateSelfResult =
            await _qdrantHttpClient.UpdateCollectionClusteringSetup(
                TestCollectionName,
                UpdateCollectionClusteringSetupRequest.CreateReplicateShardRequest(
                    localShardId,
                    localPeerId, // since we don't have more than one node in the cluster - we move shard from one peer to itself
                    localPeerId),
                CancellationToken.None);

        replicateSelfResult.EnsureSuccess();

        replicateSelfResult.Result.Should().BeTrue();

        var newCollectionClusteringInfo =
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None);

        newCollectionClusteringInfo.Result.ShardTransfers.Length.Should().Be(1);
        newCollectionClusteringInfo.Result.ShardTransfers[0].ShardId.Should().Be(localShardId);
        newCollectionClusteringInfo.Result.ShardTransfers[0].From.Should().Be(localPeerId);
        newCollectionClusteringInfo.Result.ShardTransfers[0].To.Should().Be(localPeerId);
        newCollectionClusteringInfo.Result.ShardTransfers[0].Sync.Should().Be(true);
    }

    // [Test]
    // NOTE: this test will work only when cluster is enabled in qdrant-config_node_0.yaml
    public async Task TestCollectionShardCreate()
    {
        const uint vectorSize = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var createShardKeyResult = await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            "test",
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1);

        createShardKeyResult.Status.IsSuccess.Should().BeTrue();
    }

    // [Test]
    // NOTE: this test will work only when cluster is enabled in qdrant-config_node_0.yaml
    public async Task TestCollectionShardDelete()
    {
        const uint vectorSize = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var deleteShardKeyResult = await _qdrantHttpClient.DeleteShardKey(
            TestCollectionName,
            "test",
            CancellationToken.None);

        deleteShardKeyResult.Status.IsSuccess.Should().BeTrue();
    }
}
