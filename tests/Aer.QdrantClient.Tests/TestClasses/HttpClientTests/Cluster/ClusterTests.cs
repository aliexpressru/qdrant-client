using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

//#if !DEBUG
//[Ignore(
//    "I didn't find a way to configure both single-node and a multi-node cluster in "
//        + "GitHub actions so these tests will run only locally"
//)]
//#endif
internal partial class ClusterTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();
        _qdrantHttpClient = GetClusterClient(ClusterNode.First);
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
        var collectionClusteringInfo = await _qdrantHttpClient.GetCollectionClusteringInfo(
            TestCollectionName,
            CancellationToken.None,
            isTranslatePeerIdsToUris: true
        );

        collectionClusteringInfo.Status.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task GetCollectionClusteringInfo()
    {
        await CreateTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U);

        var collectionClusteringInfo = await _qdrantHttpClient.GetCollectionClusteringInfo(
            TestCollectionName,
            CancellationToken.None,
            isTranslatePeerIdsToUris: true
        );

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
    public async Task CollectionShardMove()
    {
        await CreateTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U);

        var collectionClusteringInfo = (
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None)
        ).EnsureSuccess();

        var localShardId = collectionClusteringInfo.LocalShards[0].ShardId;
        var localPeerId = collectionClusteringInfo.PeerId;
        var remotePeerId = collectionClusteringInfo.RemoteShards[0].PeerId;

        var moveShardResult = await _qdrantHttpClient.UpdateCollectionClusteringSetup(
            TestCollectionName,
            UpdateCollectionClusteringSetupRequest.CreateMoveShardRequest(localShardId, localPeerId, remotePeerId),
            CancellationToken.None
        );

        moveShardResult.EnsureSuccess();

        moveShardResult.Result.Should().BeTrue();

        var newCollectionClusteringInfo = (
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None)
        ).EnsureSuccess();

        newCollectionClusteringInfo.ShardTransfers.Length.Should().Be(1);
        newCollectionClusteringInfo.ShardTransfers[0].ShardId.Should().Be(localShardId);
        newCollectionClusteringInfo.ShardTransfers[0].From.Should().Be(localPeerId);
        newCollectionClusteringInfo.ShardTransfers[0].To.Should().Be(remotePeerId);
        newCollectionClusteringInfo.ShardTransfers[0].Sync.Should().Be(false);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true
        );

        var newCollectionClusteringInfoAfterShardMove = (
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None)
        ).EnsureSuccess();

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
    public async Task CollectionShardReplicate()
    {
        await CreateTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U);

        var collectionClusteringInfo = await _qdrantHttpClient.GetCollectionClusteringInfo(
            TestCollectionName,
            CancellationToken.None
        );

        var localShardId = collectionClusteringInfo.Result.LocalShards[0].ShardId;
        var localPeerId = collectionClusteringInfo.Result.PeerId;
        var remotePeerId = collectionClusteringInfo.Result.RemoteShards[0].PeerId;

        var replicateShardResult = await _qdrantHttpClient.UpdateCollectionClusteringSetup(
            TestCollectionName,
            UpdateCollectionClusteringSetupRequest.CreateReplicateShardRequest(
                localShardId,
                localPeerId,
                remotePeerId,
                shardTransferMethod: ShardTransferMethod.StreamRecords
            ),
            CancellationToken.None
        );

        replicateShardResult.EnsureSuccess();

        replicateShardResult.Result.Should().BeTrue();

        var newCollectionClusteringInfo = await _qdrantHttpClient.GetCollectionClusteringInfo(
            TestCollectionName,
            CancellationToken.None
        );

        newCollectionClusteringInfo.Result.ShardTransfers.Length.Should().Be(1);
        newCollectionClusteringInfo.Result.ShardTransfers[0].ShardId.Should().Be(localShardId);
        newCollectionClusteringInfo.Result.ShardTransfers[0].From.Should().Be(localPeerId);
        newCollectionClusteringInfo.Result.ShardTransfers[0].To.Should().Be(remotePeerId);
        newCollectionClusteringInfo.Result.ShardTransfers[0].Sync.Should().Be(true);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true
        );

        var newCollectionClusteringInfoAfterShardMove = (
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None)
        ).EnsureSuccess();

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

    // [Test] // The resharding process stage 1 : creation of a new shard works ok,
    // but points are not moved to the new shard. We seem to be missing some external component
    public async Task CollectionResharding_WithShardKey()
    {
        // Create collection with just 1 shard
        await CreateTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U, shardNumber: 1);

        (
            await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points =
                    [
                        new(PointId.NewGuid(), CreateTestFloat32Vector(10U), (TestPayload)"test1"),
                        new(PointId.NewGuid(), CreateTestFloat32Vector(10U), (TestPayload)"test2"),
                        new(PointId.NewGuid(), CreateTestFloat32Vector(10U), (TestPayload)"test3"),
                        new(PointId.NewGuid(), CreateTestFloat32Vector(10U), (TestPayload)"test4"),
                        new(PointId.NewGuid(), CreateTestFloat32Vector(10U), (TestPayload)"test5"),
                    ],
                    ShardKey = TestShardKey1,
                },
                CancellationToken.None
            )
        ).EnsureSuccess();

        var collectionClusteringInfoBeforeScaleUp = (
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None)
        ).EnsureSuccess();

        collectionClusteringInfoBeforeScaleUp.ShardCount.Should().Be(1);

        var firstPeerId = collectionClusteringInfoBeforeScaleUp.ShardsByPeers.Keys.First();

        var scaleCollectionUpResult = await _qdrantHttpClient.UpdateCollectionClusteringSetup(
            TestCollectionName,
            UpdateCollectionClusteringSetupRequest.CreateStartReshardingRequest(
                ReshardingOperationDirection.Up,
                firstPeerId,
                shardKey: TestShardKey1
            ),
            CancellationToken.None
        );

        scaleCollectionUpResult.Status.IsSuccess.Should().BeTrue();

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true
        );

        var collectionClusteringInfoAfterScaleUp = (
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None)
        ).EnsureSuccess();

        collectionClusteringInfoAfterScaleUp.ShardCount.Should().BeGreaterThan(1);
    }

    //[Test]
    [Ignore("This test works only once since for adding peer back the whole cluster should be recreated")]
#pragma warning disable NUnit1028 // The non-test method is public
    public async Task CollectionRemovePeer_RemotePeer()
#pragma warning restore NUnit1028 // The non-test method is public
    {
        // NOTE: after performing this test - stop all containers, remove volumes
        // and rerun docker compose up to restore cluster to its 2-node state

        await CreateTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U);

        var collectionClusteringInfo = await _qdrantHttpClient.GetCollectionClusteringInfo(
            TestCollectionName,
            CancellationToken.None
        );

        var remotePeerId = collectionClusteringInfo.Result.RemoteShards[0].PeerId;

        var deletePeerResult = await _qdrantHttpClient.RemovePeer(remotePeerId, CancellationToken.None);

        deletePeerResult.Status.IsSuccess.Should().BeFalse();
        deletePeerResult.Status.GetErrorMessage().Should().Contain("there are shards on it");

        var forceDeletePeerResult = await _qdrantHttpClient.RemovePeer(
            remotePeerId,
            CancellationToken.None,
            isForceDropOperation: true
        );

        forceDeletePeerResult.Status.IsSuccess.Should().BeTrue();

        var newCollectionClusteringInfo = (
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None)
        ).EnsureSuccess();

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
                OnDiskPayload = true,
            },
            CancellationToken.None
        );

        var recoverCollectionRaftState = await _qdrantHttpClient.RecoverPeerRaftState(CancellationToken.None);

        recoverCollectionRaftState.Status.IsSuccess.Should().BeTrue();
        recoverCollectionRaftState.Result.Should().BeTrue();
    }
}
