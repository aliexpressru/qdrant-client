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
        _qdrantHttpClient = GetClusterClient();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage(_qdrantHttpClient, isDeleteCollectionFiles: false);
    }

    [Test]
    public async Task TestClusterInfo_ClusterEnabled()
    {
        var clusterInfo = await _qdrantHttpClient.GetClusterInfo(CancellationToken.None);

        clusterInfo.Status.IsSuccess.Should().BeTrue();
        clusterInfo.Result.Status.Should().Be("enabled");
    }

    [Test]
    public async Task TestGetCollectionClusteringInfo_ManualSharding()
    {
        await CreateSmallTestShardedCollection(TestCollectionName, 10U);

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

    [Test]
    public async Task TestCollectionShardMove_OneShard()
    {
        await CreateSmallTestShardedCollection(TestCollectionName, 10U);

        var collectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var localShardId = collectionClusteringInfo.LocalShards[0].ShardId;
        var localPeerId = collectionClusteringInfo.PeerId;
        var remotePeerId = collectionClusteringInfo.RemoteShards[0].PeerId;

        var moveShardResult =
            await _qdrantHttpClient.UpdateCollectionClusteringSetup(
                TestCollectionName,
                UpdateCollectionClusteringSetupRequest.CreateMoveShardRequest(
                    localShardId,
                    localPeerId,
                    remotePeerId),
                CancellationToken.None);

        moveShardResult.EnsureSuccess();

        moveShardResult.Result.Should().BeTrue();

        var newCollectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        newCollectionClusteringInfo.ShardTransfers.Length.Should().Be(1);
        newCollectionClusteringInfo.ShardTransfers[0].ShardId.Should().Be(localShardId);
        newCollectionClusteringInfo.ShardTransfers[0].From.Should().Be(localPeerId);
        newCollectionClusteringInfo.ShardTransfers[0].To.Should().Be(remotePeerId);
        newCollectionClusteringInfo.ShardTransfers[0].Sync.Should().Be(false);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            checkAllCollectionShardTransfersCompleted: true);

        var newCollectionClusteringInfoAfterShardMove = (
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        if (newCollectionClusteringInfoAfterShardMove.PeerId == localPeerId)
        {
            // means we got response from local peer

            // local peer should not contain shards since we moved one
            newCollectionClusteringInfoAfterShardMove.LocalShards.Length.Should().Be(0);
            newCollectionClusteringInfoAfterShardMove.RemoteShards.Length.Should().Be(2);
        }
        else
        {
            // means we got response from remote peer

            // remote peer should contain all shards since we moved one
            newCollectionClusteringInfoAfterShardMove.LocalShards.Length.Should().Be(2);
            newCollectionClusteringInfoAfterShardMove.RemoteShards.Length.Should().Be(0);
        }
    }

    [Test]
    public async Task TestCollectionShardReplicate_OneShard()
    {
        await CreateSmallTestShardedCollection(TestCollectionName, 10U);

        var collectionClusteringInfo =
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None);

        var localShardId = collectionClusteringInfo.Result.LocalShards[0].ShardId;
        var localPeerId = collectionClusteringInfo.Result.PeerId;
        var remotePeerId = collectionClusteringInfo.Result.RemoteShards[0].PeerId;

        var replicateShardResult =
            await _qdrantHttpClient.UpdateCollectionClusteringSetup(
                TestCollectionName,
                UpdateCollectionClusteringSetupRequest.CreateReplicateShardRequest(
                    localShardId,
                    localPeerId,
                    remotePeerId),
                CancellationToken.None);

        replicateShardResult.EnsureSuccess();

        replicateShardResult.Result.Should().BeTrue();

        var newCollectionClusteringInfo =
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None);

        newCollectionClusteringInfo.Result.ShardTransfers.Length.Should().Be(1);
        newCollectionClusteringInfo.Result.ShardTransfers[0].ShardId.Should().Be(localShardId);
        newCollectionClusteringInfo.Result.ShardTransfers[0].From.Should().Be(localPeerId);
        newCollectionClusteringInfo.Result.ShardTransfers[0].To.Should().Be(remotePeerId);
        newCollectionClusteringInfo.Result.ShardTransfers[0].Sync.Should().Be(true);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            checkAllCollectionShardTransfersCompleted: true);

        var newCollectionClusteringInfoAfterShardMove = (
                await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        // local peer should contain one shard
        // remote peer should contain two shards since we replicated one

        if (newCollectionClusteringInfoAfterShardMove.PeerId == localPeerId)
        {
            // means we got response from local peer

            newCollectionClusteringInfoAfterShardMove.LocalShards.Length.Should().Be(1);
            newCollectionClusteringInfoAfterShardMove.RemoteShards.Length.Should().Be(2);
        }
        else
        {
            // means we got response from remote peer

            newCollectionClusteringInfoAfterShardMove.LocalShards.Length.Should().Be(2);
            newCollectionClusteringInfoAfterShardMove.RemoteShards.Length.Should().Be(1);
        }
    }

    [Test]
    public async Task TestCollectionCreateShardKey_ManualPlacement()
    {
        (await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10U, isServeVectorsFromDisk: true)
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

        var createFirstShardKey = await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            TestShardKey1,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.First()]);

        var createSecondShardKey = await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            TestShardKey2,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.Skip(1).First()]);

        createFirstShardKey.Status.IsSuccess.Should().BeTrue();
        createFirstShardKey.Result.Should().BeTrue();

        createSecondShardKey.Status.IsSuccess.Should().BeTrue();
        createSecondShardKey.Result.Should().BeTrue();
    }

    [Test]
    public async Task TestCollectionDeleteShardKey_ManualPlacement()
    {
        (await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10U, isServeVectorsFromDisk: true)
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

        var deleteShardKeyResult = await _qdrantHttpClient.DeleteShardKey(
            TestCollectionName,
            TestShardKey1,
            CancellationToken.None);

        deleteShardKeyResult.Status.IsSuccess.Should().BeTrue();
    }

    //[Test]
    [Ignore("This test works only once since for adding peer back the whole cluster should be recreated")]
    public async Task TestCollectionRemovePeer_RemotePeer()
    {
        // NOTE: after performing this test - stop all containers, remove volumes
        // and rerun docker compose up to restore cluster to its 2-node state

        await CreateSmallTestShardedCollection(TestCollectionName, 10U);

        var collectionClusteringInfo =
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None);

        var remotePeerId = collectionClusteringInfo.Result.RemoteShards[0].PeerId;

        var deletePeerResult = await _qdrantHttpClient.RemovePeer(
            remotePeerId,
            CancellationToken.None);

        deletePeerResult.Status.IsSuccess.Should().BeFalse();
        deletePeerResult.Status.GetErrorMessage().Should().Contain("there are shards on it");

        var forceDeletePeerResult = await _qdrantHttpClient.RemovePeer(
            remotePeerId,
            CancellationToken.None,
            isForceDropOperation: true);

        forceDeletePeerResult.Status.IsSuccess.Should().BeTrue();

        var newCollectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        newCollectionClusteringInfo.RemoteShards.Length.Should().Be(0);
    }

    [Test]
    public async Task TestCollectionRecoverRaftStart_Success()
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
        recoverCollectionRaftState.Result.Should().BeTrue();
    }

    private async Task CreateSmallTestShardedCollection(string collectionName, uint vectorSize)
    {
        (await _qdrantHttpClient.CreateCollection(
            collectionName,
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
            collectionName,
            TestShardKey1,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.First()])).EnsureSuccess();

        (await _qdrantHttpClient.CreateShardKey(
            collectionName,
            TestShardKey2,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.Skip(1).First()])).EnsureSuccess();

        (await _qdrantHttpClient.UpsertPoints(
            collectionName,
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
            collectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
                {
                    new(PointId.NewGuid(), CreateTestFloatVector(vectorSize), "test2"),
                },
                ShardKey = TestShardKey2
            },
            CancellationToken.None)).EnsureSuccess();

        await _qdrantHttpClient.EnsureCollectionReady(collectionName, cancellationToken: CancellationToken.None);

        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}
