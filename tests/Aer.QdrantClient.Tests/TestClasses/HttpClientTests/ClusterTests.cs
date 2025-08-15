using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

#if !DEBUG
[Ignore("I didn't find a way to configure both single-node and a multi-node cluster in "
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
        await ResetStorage(_qdrantHttpClient);
    }

    [Test]
    public async Task ClusterInfo_ClusterEnabled()
    {
        var clusterInfo = await _qdrantHttpClient.GetClusterInfo(CancellationToken.None);

        clusterInfo.Status.IsSuccess.Should().BeTrue();
        clusterInfo.Result.Status.Should().Be("enabled");
    }

    [Test]
    public async Task GetCollectionClusteringInfo_NonExistentCollection()
    {
        var collectionClusteringInfo =
            await _qdrantHttpClient.GetCollectionClusteringInfo(
                TestCollectionName,
                CancellationToken.None,
                isTranslatePeerIdsToUris: true);

        collectionClusteringInfo.Status.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task GetCollectionClusteringInfo()
    {
        await CreateSmallTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U);

        var collectionClusteringInfo =
            await _qdrantHttpClient.GetCollectionClusteringInfo(
                TestCollectionName,
                CancellationToken.None,
                isTranslatePeerIdsToUris: true);

        collectionClusteringInfo.Status.IsSuccess.Should().BeTrue();

        collectionClusteringInfo.Result.PeerId.Should().NotBe(0);
        collectionClusteringInfo.Result.PeerUri.Should().NotBeNullOrEmpty();
        
        collectionClusteringInfo.Result.ShardCount.Should().Be(2);
        collectionClusteringInfo.Result.LocalShards.Length.Should().Be(1);

        collectionClusteringInfo.Result.LocalShards[0].PointsCount.Should().Be(1);
        collectionClusteringInfo.Result.LocalShards[0].State.Should().Be(ShardState.Active);

        collectionClusteringInfo.Result.RemoteShards[0].State.Should().Be(ShardState.Active);
        collectionClusteringInfo.Result.RemoteShards[0].PeerId.Should().NotBe(0);
        collectionClusteringInfo.Result.RemoteShards[0].PeerUri.Should().NotBeNullOrEmpty();

        collectionClusteringInfo.Result.RemoteShards.Length.Should().Be(1);
        collectionClusteringInfo.Result.ShardTransfers.Length.Should().Be(0);
        
        collectionClusteringInfo.Result.PartialShardCount.Should().Be(0);
        collectionClusteringInfo.Result.DeadShardCount.Should().Be(0);
    }

    [Test]
    public async Task CollectionShardMove_OneShard()
    {
        await CreateSmallTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U);

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
            isCheckShardTransfersCompleted: true);

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
    public async Task CollectionShardReplicate_OneShard()
    {
        await CreateSmallTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U);

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
                    remotePeerId,
                    shardTransferMethod: ShardTransferMethod.StreamRecords),
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
            isCheckShardTransfersCompleted: true);

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
    public async Task CollectionCreateShardKey_ManualPlacement()
    {
        var vectorSize = 10U;

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

        var createFirstShardKey = await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            TestShardKey1,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.First()]);

        var createSecondShardKey = await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            TestShardKeyInt1,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.Skip(1).First()]);

        createFirstShardKey.Status.IsSuccess.Should().BeTrue();
        createFirstShardKey.Result.Should().BeTrue();

        createSecondShardKey.Status.IsSuccess.Should().BeTrue();
        createSecondShardKey.Result.Should().BeTrue();

        UpsertPointsRequest<TestPayload>.UpsertPoint firstShardPoint = new(
            id: 1,
            vector: CreateTestVector(vectorSize),
            payload: 1);

        UpsertPointsRequest<TestPayload>.UpsertPoint secondShardPoint = new(
            id: 2,
            vector: CreateTestVector(vectorSize),
            payload: 2);

        var upsertOnFirstShardResponse = await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points =
                [
                    firstShardPoint
                ],
                ShardKey = TestShardKey1
            },
            CancellationToken.None);

        var upsertOnSecondShardResponse = await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points =
                [
                    secondShardPoint
                ],
                ShardKey = TestShardKeyInt1
            },
            CancellationToken.None);

        upsertOnFirstShardResponse.Status.IsSuccess.Should().BeTrue();
        upsertOnSecondShardResponse.Status.IsSuccess.Should().BeTrue();

        var readPoints = (await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            QdrantFilter.Empty,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true,
            limit: 2)).EnsureSuccess();

        readPoints.Points.Length.Should().Be(2);

        var firstReadPoint = readPoints.Points.Single(p => p.Id == firstShardPoint.Id);
        var secondReadPoint = readPoints.Points.Single(p => p.Id == secondShardPoint.Id);

        firstReadPoint.Payload.As<int>().Should().Be(firstShardPoint.Payload.As<int>());

        // manual cast to eliminate cyclic reference
        // Default = {Cyclic reference to type Aer.QdrantClient.Http.Models.Primitives.Vectors.DenseVector detected},
        firstReadPoint.Vector.Default.AsDenseVector().VectorValues.Should().BeEquivalentTo(firstShardPoint.Vector.Default.AsDenseVector().VectorValues);
        firstReadPoint.ShardKey.IsString().Should().BeTrue();
        firstReadPoint.ShardKey.GetString().Should().Be(TestShardKey1);

        secondReadPoint.Payload.As<int>().Should().Be(secondShardPoint.Payload.As<int>());
        secondReadPoint.Vector.Default.AsDenseVector().VectorValues.Should().BeEquivalentTo(secondShardPoint.Vector.Default.AsDenseVector().VectorValues);
        secondReadPoint.ShardKey.IsInteger().Should().BeTrue();
        secondReadPoint.ShardKey.GetInteger().Should().Be(TestShardKeyInt1);
    }

    [Test]
    public async Task CollectionDeleteShardKey_ManualPlacement()
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
    public async Task CollectionRemovePeer_RemotePeer()
    {
        // NOTE: after performing this test - stop all containers, remove volumes
        // and rerun docker compose up to restore cluster to its 2-node state

        await CreateSmallTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U);

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
    public async Task CollectionRecoverRaftStart_Success()
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
}
