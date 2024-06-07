using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

#if !DEBUG
[Ignore("I didn't find a way to configure both single-node deployment and 3 node cluster in "
+"GitHub actions so these tests will run only locally")]
#endif
public class ClusterTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();
        // create client with first cluster node port - 6343
        _qdrantHttpClient = GetClusterClient();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage(_qdrantHttpClient, isDeleteCollectionFiles: false);
    }

    [Test]
    public async Task TestClusterInfo()
    {
        var clusterInfo = await _qdrantHttpClient.GetClusterInfo(CancellationToken.None);

        clusterInfo.Status.IsSuccess.Should().BeTrue();
        clusterInfo.Result.Status.Should().Be("enabled");
    }

    [Test]
    public async Task TestGetCollectionClusteringInfo()
    {
        const uint vectorSize = 10;

        (await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                WriteConsistencyFactor = 2,
                ReplicationFactor = 1,
                ShardNumber = 2,
                ShardingMethod = ShardingMethod.Custom
            },
            CancellationToken.None)).EnsureSuccess();

        // configure collection manual sharding to ensure consistent results

        var allPeers = (await _qdrantHttpClient.GetClusterInfo(CancellationToken.None))
            .EnsureSuccess().AllPeerIds;

        (await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            TestShardKey1,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.First()])).EnsureSuccess();

        (await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            TestShardKey2,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.Skip(1).First()])).EnsureSuccess();

        (await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
                    {
                        new(PointId.NewGuid(), CreateTestFloatVector(vectorSize), "test"),
                    },
                    ShardKey = TestShardKey1
                },
                CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
                {
                    new(PointId.NewGuid(), CreateTestFloatVector(vectorSize), "test2"),
                },
                ShardKey = TestShardKey2
            },
            CancellationToken.None)).EnsureSuccess();

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, cancellationToken: CancellationToken.None);

        await Task.Delay(TimeSpan.FromSeconds(1));

        var collectionClusteringInfo =
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None);

        collectionClusteringInfo.Status.IsSuccess.Should().BeTrue();

        collectionClusteringInfo.Result.PeerId.Should().NotBe(0);
        collectionClusteringInfo.Result.ShardCount.Should().Be(2);
        collectionClusteringInfo.Result.LocalShards.Length.Should().Be(1);

        collectionClusteringInfo.Result.LocalShards[0].PointsCount.Should().Be(1);
        collectionClusteringInfo.Result.LocalShards[0].State.Should().Be(ShardState.Active);

        collectionClusteringInfo.Result.RemoteShards[0].State.Should().Be(ShardState.Active);

        collectionClusteringInfo.Result.RemoteShards.Length.Should().Be(1);
        collectionClusteringInfo.Result.ShardTransfers.Length.Should().Be(0);
    }

    //[Test]
    [Ignore(
        "this test works locally only once. Then the docker compose restart and volumes drop required"
        + " this test will work only when cluster is enabled in qdrant-config_node_0.yaml")]
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
        var testVector = CreateTestFloatVector(vectorSize);
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
    [Ignore(
        "this test works locally only once. Then the docker compose restart and volumes drop required"
        + " this test will work only when cluster is enabled in qdrant-config_node_0.yaml")]
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
        var testVector = CreateTestFloatVector(vectorSize);
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
    [Ignore(
        "this test works locally only once. Then the docker compose restart and volumes drop required"
        + " this test will work only when cluster is enabled in qdrant-config_node_0.yaml")]
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
    [Ignore(
        "this test works locally only once. Then the docker compose restart and volumes drop required"
        + " this test will work only when cluster is enabled in qdrant-config_node_0.yaml")]
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

    // [Test]
    [Ignore(
        "this test works locally only once. Then the docker compose restart and volumes drop required"
        + " this test will work only when cluster is enabled in qdrant-config_node_0.yaml")]
    public async Task TestCollectionRemovePeer()
    {
        const uint vectorSize = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var clusterInfo = await _qdrantHttpClient.GetClusterInfo(CancellationToken.None);

        var deleteShardKeyResult = await _qdrantHttpClient.RemovePeer(
            clusterInfo.Result.PeerId,
            CancellationToken.None);

        deleteShardKeyResult.Status.IsSuccess.Should().BeTrue();
    }

    // [Test]
    [Ignore(
        "this test works locally only once. Then the docker compose restart and volumes drop required"
        + " this test will work only when cluster is enabled in qdrant-config_node_0.yaml")]
    public async Task TestCollectionRecoverRaftStart()
    {
        const uint vectorSize = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var recoverCollectionRaftState = await _qdrantHttpClient.RecoverPeerRaftState(
            CancellationToken.None);

        recoverCollectionRaftState.Status.IsSuccess.Should().BeTrue();
    }
}
