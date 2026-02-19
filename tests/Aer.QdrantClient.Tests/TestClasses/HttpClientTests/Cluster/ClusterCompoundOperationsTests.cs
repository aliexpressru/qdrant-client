using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

#if !DEBUG
[Ignore("I didn't find a way to configure both single-node and a multi-node cluster in "
+"GitHub actions so these tests will run only locally")]
#endif
internal class ClusterCompoundOperationsTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;
    private ILogger _logger;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();
        _qdrantHttpClient = GetClusterClient(ClusterNode.First);

        _logger = ServiceProvider.GetRequiredService<ILogger<ClusterCompoundOperationsTests>>();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage(_qdrantHttpClient);
    }

    [Test]
    public async Task GetPeerInfoByUriSubstring_MoreThanOneNodeFound()
    {
        await CreateTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U);

        var collectionPeerInfoAct =
            async () => await _qdrantHttpClient.GetPeerInfo("://", CancellationToken.None);

        await collectionPeerInfoAct.Should().ThrowAsync<QdrantMoreThanOnePeerFoundForUriSubstringException>();
    }

    [Test]
    public async Task GetPeerInfoByUriSubstring()
    {
        await CreateTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U);

        var clusterInfo =
            (await _qdrantHttpClient.GetClusterInfo(CancellationToken.None)).EnsureSuccess();

        var expectedPeer = clusterInfo.ParsedPeers
            .Single(p => p.Value.Uri.Contains("http://qdrant-1"));

        var otherPeer = clusterInfo.ParsedPeers
            .Single(p => p.Key != expectedPeer.Key);

        var collectionPeerInfoResponse =
            await _qdrantHttpClient.GetPeerInfo("http://qdrant-1", CancellationToken.None);

        collectionPeerInfoResponse.Status.IsSuccess.Should().BeTrue();

        collectionPeerInfoResponse.Result.PeerId.Should().Be(expectedPeer.Key);
        collectionPeerInfoResponse.Result.OtherPeerIds.Count.Should().Be(1);
        collectionPeerInfoResponse.Result.OtherPeerIds.Single().Should().Be(otherPeer.Key);

        collectionPeerInfoResponse.Result.PeerUriPerPeerIds.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task GetPeerInfoByPeerId()
    {
        await CreateTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U);

        var clusterInfo =
            (await _qdrantHttpClient.GetClusterInfo(CancellationToken.None)).EnsureSuccess();

        var peerIdToFind = clusterInfo.ParsedPeers.First().Key;
        var otherPeerIds = clusterInfo.ParsedPeers
            .Where(p => p.Key != peerIdToFind)
            .Select(pi => pi.Key).ToList();

        var collectionPeerInfoResponse =
            await _qdrantHttpClient.GetPeerInfo(peerIdToFind, CancellationToken.None);

        collectionPeerInfoResponse.Status.IsSuccess.Should().BeTrue();

        collectionPeerInfoResponse.Result.PeerId.Should().Be(peerIdToFind);
        collectionPeerInfoResponse.Result.OtherPeerIds.Count.Should().Be(1);
        collectionPeerInfoResponse.Result.OtherPeerIds.Should().BeEquivalentTo(otherPeerIds);

        collectionPeerInfoResponse.Result.PeerUriPerPeerIds.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task GetCollectionClusteringInfo()
    {
        var shardNumber = 4;

        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: 2,
            shardNumber: (uint)shardNumber);

        var collectionClusteringInfo = (
            await _qdrantHttpClient.GetCollectionClusteringInfo(
                TestCollectionName,
                CancellationToken.None,
                isTranslatePeerIdsToUris: true
            )
        ).EnsureSuccess();

        var clusterInfo = (
            await _qdrantHttpClient.GetClusterInfo(CancellationToken.None)
        ).EnsureSuccess();

        collectionClusteringInfo.ShardCount.Should().Be((uint)shardNumber);
        collectionClusteringInfo.PartialShardCount.Should().Be(0);
        collectionClusteringInfo.DeadShardCount.Should().Be(0);

        collectionClusteringInfo.ShardsByPeers.Should().HaveCount(clusterInfo.AllPeerIds.Count);
        collectionClusteringInfo.PeersByShards.Should().HaveCount(shardNumber); // 4 shards

        collectionClusteringInfo.PeerUri.Should().NotBeNullOrEmpty();

        foreach (var remoteShard in collectionClusteringInfo.RemoteShards)
        {
            remoteShard.PeerUri.Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public async Task IsPeerEmpty_Success()
    {
        const uint vectorSize = 10;

        var clusterInfo = (await _qdrantHttpClient.GetClusterInfo(CancellationToken.None)).EnsureSuccess();

        var firstPeerUri = clusterInfo.ParsedPeers.First().Value.Uri;
        var firstPeerId = clusterInfo.ParsedPeers.First().Key;

        var checkPeerEmptyResult = await _qdrantHttpClient.CheckIsPeerEmpty(firstPeerUri, CancellationToken.None);

        checkPeerEmptyResult.Status.IsSuccess.Should().BeTrue();
        checkPeerEmptyResult.Result.Should().BeTrue();

        (await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None)).EnsureSuccess();

        var vectorCount = 10;

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

        (await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest()
            {
                Points = upsertPoints
            },
            CancellationToken.None,
            isWaitForResult: true,
            ordering: OrderingType.Strong)).EnsureSuccess();

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        checkPeerEmptyResult = await _qdrantHttpClient.CheckIsPeerEmpty(firstPeerId, CancellationToken.None);

        checkPeerEmptyResult.Status.IsSuccess.Should().BeTrue();
        checkPeerEmptyResult.Result.Should().BeFalse();
    }

    [Test]
    public async Task DrainPeer_CollectionDoesNotExist()
    {
        var drainPeerResponse =
            await _qdrantHttpClient.DrainPeer(
                "http://qdrant-1",
                CancellationToken.None,
                logger: _logger,
                collectionNamesToMove: TestCollectionName);

        drainPeerResponse.Status.IsSuccess.Should().BeFalse();
        drainPeerResponse.Status.GetErrorMessage().Contains("does not exist").Should().BeTrue();
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public async Task DrainPeer_SelectedCollection(int replicationFactor)
    {
        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: (uint)replicationFactor);

        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName2,
            10U,
            replicationFactor: (uint)replicationFactor);

        var peerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();

        var peerIdToDrain = peerInfo.PeerId;

        var drainPeerResponse =
            await _qdrantHttpClient.DrainPeer(
                peerIdToDrain,
                CancellationToken.None,
                logger: _logger,
                collectionNamesToMove: TestCollectionName);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        drainPeerResponse.Status.IsSuccess.Should().BeTrue();
        drainPeerResponse.Result.Should().BeTrue();

        var drainedCollectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var notDrainedCollectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName2, CancellationToken.None))
            .EnsureSuccess();

        if (drainedCollectionClusteringInfo.PeerId == peerIdToDrain)
        {
            // means we got response from drained peer
            drainedCollectionClusteringInfo.LocalShards.Length.Should().Be(0);

            if (replicationFactor == 1)
            {
                // when replication factor is 1, the shard gets transferred to non-drained peer giving total number of shards of 2
                drainedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor + 1);
            }
            else
            {
                // when replication factor is 2 (which is equals to the number of nodes in the cluster),
                // the shard gets copied to non-drained peer, but that peer already contains
                // that shard so the number of shards on the target peer does not change
                drainedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor);
            }
        }
        else
        {
            // means we got response from non-drained peer
            drainedCollectionClusteringInfo.RemoteShards.Length.Should().Be(0);

            if (replicationFactor == 1)
            {
                // when replication factor is 1, the shard gets transferred to non-drained peer giving total number of shards of 2
                drainedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor + 1);
            }
            else
            {
                // when replication factor is 2 (which is equals to the number of nodes in the cluster),
                // the shard gets copied to non-drained peer, but that peer already contains
                // that shard so the number of shards on the target peer does not change
                drainedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor);
            }
        }

        notDrainedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor);
        notDrainedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public async Task DrainPeer_AllCollections(int replicationFactor)
    {
        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: (uint)replicationFactor);

        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName2,
            10U,
            replicationFactor: (uint)replicationFactor);

        var peerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();

        var peerIdToDrain = peerInfo.PeerId;

        var drainPeerResponse =
            await _qdrantHttpClient.DrainPeer(
                "http://qdrant-1",
                CancellationToken.None,
                logger: _logger);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName2,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        drainPeerResponse.Status.IsSuccess.Should().BeTrue();
        drainPeerResponse.Result.Should().BeTrue();

        var firstDrainedCollectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var secondDrainedCollectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName2, CancellationToken.None))
            .EnsureSuccess();

        List<GetCollectionClusteringInfoResponse.CollectionClusteringInfo> drainedCollectionClusteringInfos =
            [firstDrainedCollectionClusteringInfo, secondDrainedCollectionClusteringInfo];

        foreach (var drainedCollectionClusteringInfo in drainedCollectionClusteringInfos)
        {
            if (drainedCollectionClusteringInfo.PeerId == peerIdToDrain)
            {
                // means we got response from drained peer
                drainedCollectionClusteringInfo.LocalShards.Length.Should().Be(0);

                if (replicationFactor == 1)
                {
                    // when replication factor is 1, the shard gets transferred to non-drained peer giving total number of shards of 2
                    drainedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor + 1);
                }
                else
                {
                    // when replication factor is 2 (which is equals to the number of nodes in the cluster),
                    // the shard gets copied to non-drained peer, but that peer already contains
                    // that shard so the number of shards on the target peer does not change
                    drainedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor);
                }
            }
            else
            {
                // means we got response from non-drained peer
                drainedCollectionClusteringInfo.RemoteShards.Length.Should().Be(0);

                if (replicationFactor == 1)
                {
                    // when replication factor is 1, the shard gets transferred to non-drained peer giving total number of shards of 2
                    drainedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor + 1);
                }
                else
                {
                    // when replication factor is 2 (which is equals to the number of nodes in the cluster),
                    // the shard gets copied to non-drained peer, but that peer already contains
                    // that shard so the number of shards on the target peer does not change
                    drainedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor);
                }
            }
        }
    }

    [Test]
    public async Task DropCollectionShards()
    {
        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: 2);

        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName2,
            10U,
            replicationFactor: 2);

        var peerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();

        var collection1ClusteringBeforeDrop = (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var peerIdToDrain = peerInfo.PeerId;

        var shardsOnPeer = collection1ClusteringBeforeDrop.ShardsByPeers[peerIdToDrain];

        var shardToDrop = shardsOnPeer.First();

        var dropOneShardResponse =
            await _qdrantHttpClient.DropCollectionShardsFromPeer(
                TestCollectionName,
                "qdrant-1",
                [shardToDrop],
                CancellationToken.None,
                logger: _logger);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        dropOneShardResponse.Status.IsSuccess.Should().BeTrue();
        dropOneShardResponse.Result.IsSuccess.Should().BeTrue();
        dropOneShardResponse.Result.DroppedShardIds.Should().BeEquivalentTo([shardToDrop]);

        var collection1ClusteringAfterDrop = (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var shardsOnPeerAfterDrop = collection1ClusteringAfterDrop.ShardsByPeers[peerIdToDrain];

        shardsOnPeerAfterDrop.Should().NotContain(shardToDrop);
    }

    [Test]
    public async Task ClearPeer_CollectionDoesNotExist()
    {
        var clearPeerResponse =
            await _qdrantHttpClient.ClearPeer(
                "http://qdrant-1",
                CancellationToken.None,
                logger: _logger,
                collectionNamesToClear: [TestCollectionName]);

        clearPeerResponse.Status.IsSuccess.Should().BeFalse();
        clearPeerResponse.Status.GetErrorMessage().Contains("does not exist").Should().BeTrue();
    }

    [Test]
    public async Task ClearPeer_NoReplicasLeft()
    {
        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: 1,
            shardNumber: 2);

        var clearPeerResponse =
            await _qdrantHttpClient.ClearPeer(
                "http://qdrant-2",
                CancellationToken.None,
                logger: _logger,
                collectionNamesToClear: [TestCollectionName]);

        clearPeerResponse.Status.IsSuccess.Should().BeFalse();
        clearPeerResponse.Status.GetErrorMessage().Contains("must have at least one active replica after removing").Should().BeTrue();
    }

    [Test]
    public async Task ClearPeer()
    {
        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: 2,
            shardNumber: 2);

        var unclearedPeerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();

        var clearPeerResponse =
            await _qdrantHttpClient.ClearPeer(
                "http://qdrant-2",
                CancellationToken.None,
                logger: _logger,
                collectionNamesToClear: [TestCollectionName]);

        clearPeerResponse.Status.IsSuccess.Should().BeTrue();

        var collectionStateAfterClear =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        if (collectionStateAfterClear.PeerId == unclearedPeerInfo.PeerId)
        {
            // means we got response from uncleared peer

            collectionStateAfterClear.LocalShards.Length.Should().Be(2);
            collectionStateAfterClear.RemoteShards.Length.Should().Be(0);
        }
        else
        {
            // means we got response from cleared peer

            collectionStateAfterClear.LocalShards.Length.Should().Be(0);
            collectionStateAfterClear.RemoteShards.Length.Should().Be(2);
        }
    }

    [Test]
    public async Task EqualizeReplication_CollectionDoesNotExist()
    {
        var equalizeShardReplicationResponse =
            await _qdrantHttpClient.EqualizeShardReplication(
                [TestCollectionName],
                "http://qdrant-1",
                "http://qdrant-2",
                CancellationToken.None,
                logger: _logger);

        equalizeShardReplicationResponse.Status.IsSuccess.Should().BeFalse();
        equalizeShardReplicationResponse.Status.GetErrorMessage().Contains("does not exist").Should().BeTrue();
    }

    [Test]
    public async Task EqualizeReplication_OneShard_NoEqualization()
    {
        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: 2,
            shardNumber: 1);

        (await _qdrantHttpClient.ClearPeer(
            "http://qdrant-2",
            CancellationToken.None,
            logger: _logger)).EnsureSuccess();

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        var equalizeShardReplicationResponse =
            await _qdrantHttpClient.EqualizeShardReplication(
                [TestCollectionName],
                "http://qdrant-1",
                "http://qdrant-2",
                CancellationToken.None,
                logger: _logger);

        equalizeShardReplicationResponse.Status.IsSuccess.Should().BeFalse();
        equalizeShardReplicationResponse.Status.GetErrorMessage().Contains("The source peer should have more than 1 shards for equalization").Should().BeTrue();
    }

    [Test]
    public async Task RestoreShardReplicationFactor()
    {
        /*
        This test checks whether the following transition happens when RestoreShardReplicationFactor is called:

        Node 1   Node 2
        S1 S4     S2 S1
        S3 S2     S4 S3
        --------------- Drop replicas until this state
        S1        S2 S1
        S3        S4
        --------------- Call RestoreShardReplicationFactor, the initial state should be recovered
        S1 S4     S2 S1
        S3 S2     S4 S3
        */

        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: 2,
            shardNumber: 4);

        var firstPeerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();

        var secondPeerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-2", CancellationToken.None))
            .EnsureSuccess();

        // Prepare initial unbalanced cluster state
        // Drop shards 2 and 4 from peer1
        // Drop shard 3 from peer2

        uint[] shardsToDropFromFirstPeer = [2, 4];
        uint[] shardsToDropFromSecondPeer = [3];

        var dropShardsResponse1 = await _qdrantHttpClient.DropCollectionShardsFromPeer(
            TestCollectionName,
            firstPeerInfo.PeerId,
            shardsToDropFromFirstPeer,
            CancellationToken.None
        );

        dropShardsResponse1.Status.IsSuccess.Should().BeTrue();
        dropShardsResponse1.Result.DroppedShardIds.Should().BeEquivalentTo(shardsToDropFromFirstPeer);

        var dropShardsResponse2 = await _qdrantHttpClient.DropCollectionShardsFromPeer(
            TestCollectionName,
            secondPeerInfo.PeerId,
            shardsToDropFromSecondPeer,
            CancellationToken.None
        );

        dropShardsResponse2.Status.IsSuccess.Should().BeTrue();
        dropShardsResponse2.Result.DroppedShardIds.Should().BeEquivalentTo(shardsToDropFromSecondPeer);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None, isCheckShardTransfersCompleted: true);

        // Restore replication factor

        var restoreReplicationFactorResponse = await _qdrantHttpClient.RestoreShardReplicationFactor(TestCollectionName, CancellationToken.None);

        restoreReplicationFactorResponse.Status.IsSuccess.Should().BeTrue();

        var shardReplicator = restoreReplicationFactorResponse.Result;

        shardReplicator.ShardsNeedReplication.Should().BeTrue();

        await foreach (var shardReplicationResult in shardReplicator.ExecuteReplications(CancellationToken.None))
        {
            shardReplicationResult.Status.IsSuccess.Should().BeTrue();

            shardReplicationResult.Result.ReplicatedShards.Count.Should().Be(1);

            var singleShardReplicationResult = shardReplicationResult.Result.ReplicatedShards[0];

            singleShardReplicationResult.CollectionName.Should().Be(TestCollectionName);

            if (singleShardReplicationResult.TargetPeerId == firstPeerInfo.PeerId)
            {
                singleShardReplicationResult.ShardId.Should().BeOneOf(shardsToDropFromFirstPeer);
            }

            if (singleShardReplicationResult.TargetPeerId == secondPeerInfo.PeerId)
            {
                singleShardReplicationResult.ShardId.Should().BeOneOf(shardsToDropFromSecondPeer);
            }

            singleShardReplicationResult.IsSuccess.Should().BeTrue();

            // Wait for replication to complete before moving to the next shard
            await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName, CancellationToken.None, isCheckShardTransfersCompleted: true);
        }

        var collectionClusteringInfo = (
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None, isTranslatePeerIdsToUris: true)
        ).EnsureSuccess();

        collectionClusteringInfo.LocalShards.Length.Should().Be(4);
        collectionClusteringInfo.RemoteShards.Length.Should().Be(4);

        foreach (var (_, peerIds) in collectionClusteringInfo.PeersByShards)
        {
            peerIds.Count.Should().Be(2); // 2 nodes per cluster
        }
    }

    [Test]
    public async Task EqualizeReplication()
    {
        /*
        This test checks whether the following transition happens when ReplicateShardsToPeer is called:

        Node 1   Node 2
          S1       S2
          S3       S4
        --------------- Drain node 2
          S1       
          S2
          S3
          S4
        --------------- Equalize replication as it was before drain (random shards may be moved to target peer)
          S1       S2
          S3       S4
        */

        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: 1,
            shardNumber: 4);

        var targetPeerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();

        (await _qdrantHttpClient.DrainPeer(
                "http://qdrant-2",
                CancellationToken.None,
                logger: _logger)).EnsureSuccess();

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        var collectionStateAfterDrain =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        if (collectionStateAfterDrain.PeerId == targetPeerInfo.PeerId)
        {
            // means we got response from target peer

            collectionStateAfterDrain.LocalShards.Length.Should().Be(4);
            collectionStateAfterDrain.RemoteShards.Length.Should().Be(0);
        }
        else
        {
            // means we got response from source peer

            collectionStateAfterDrain.LocalShards.Length.Should().Be(0);
            collectionStateAfterDrain.RemoteShards.Length.Should().Be(4);
        }

        var equalizeShardReplicationResponse =
            await _qdrantHttpClient.EqualizeShardReplication(
                [TestCollectionName],
                "http://qdrant-1",
                "http://qdrant-2",
                CancellationToken.None,
                logger: _logger);

        equalizeShardReplicationResponse.Status.IsSuccess.Should().BeTrue();

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        var collectionStateAfterRestoreReplication =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        collectionStateAfterRestoreReplication.LocalShards.Length.Should().Be(2);
        collectionStateAfterRestoreReplication.RemoteShards.Length.Should().Be(2);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public async Task ReplicateShardsToClusterNode_SelectedCollection(int replicationFactor)
    {
        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: (uint)replicationFactor);

        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName2,
            10U,
            replicationFactor: (uint)replicationFactor);

        var peerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();

        var targetPeerId = peerInfo.PeerId;

        var replicateCollectionResponse =
            await _qdrantHttpClient.ReplicateShardsToPeer(
                "http://qdrant-1",
                CancellationToken.None,
                logger: _logger,
                collectionNamesToReplicate: TestCollectionName);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        replicateCollectionResponse.Status.IsSuccess.Should().BeTrue();

        replicateCollectionResponse.Result.AlreadyReplicatedShards.Count.Should().Be(1);
        replicateCollectionResponse.Result.AlreadyReplicatedShards.Should().ContainKey(TestCollectionName);
        replicateCollectionResponse.Result.AlreadyReplicatedShards[TestCollectionName].Count.Should().Be(1);
        replicateCollectionResponse.Result.AlreadyReplicatedShards[TestCollectionName][targetPeerId].Count.Should()
            .Be(replicationFactor);

        var replicatedCollectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var notReplicatedCollectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName2, CancellationToken.None))
            .EnsureSuccess();

        if (replicatedCollectionClusteringInfo.PeerId == targetPeerId)
        {
            // means we got response from target peer

            replicatedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor);

            if (replicationFactor == 1)
            {
                // when replication factor is 1, the shard gets replicated to a target peer giving total number of shards of 2
                replicatedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor + 1);
            }
            else
            {
                // when replication factor is 2 (which is equals to the number of nodes in the cluster),
                // the shard gets replicated to a target peer, but that peer already contains
                // that shard so the number of shards on the target peer does not change
                replicatedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor);
            }
        }
        else
        {
            replicatedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor);

            if (replicationFactor == 1)
            {
                // when replication factor is 1, the shard gets replicated to a target peer giving total number of shards of 2
                replicatedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor + 1);
            }
            else
            {
                // when replication factor is 2 (which is equals to the number of nodes in the cluster),
                // the shard gets replicated to a target peer, but that peer already contains
                // that shard so the number of shards on the target peer does not change
                replicatedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor);
            }
        }

        notReplicatedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor);
        notReplicatedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor);
    }

    [Test]
    public async Task ReplicateShardsToClusterNode_FullReplication()
    {
        /*
        This test checks whether the following transition happens when ReplicateShardsToPeer is called:
        
        Node 1   Node 2
          S1       S2
        --------------- ReplicateShardsToPeer from Node 2 to Node 1
          S1       S2
          S2
          
        This transition is possible only when replication factor is 1 because if it's 2 (which is equal to the number of nodes in the cluster)
        then each node already contains both shards and no replication is needed.
        */

        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: 1);

        var replicatedCollectionInitialClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        // Initially each node has one shard

        replicatedCollectionInitialClusteringInfo.LocalShards.Length.Should().Be(1);
        replicatedCollectionInitialClusteringInfo.RemoteShards.Length.Should().Be(1);

        var targetPeerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();

        var sourcePeerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-2", CancellationToken.None))
            .EnsureSuccess();

        var targetPeerId = targetPeerInfo.PeerId;
        var sourcePeerId = sourcePeerInfo.PeerId;

        var expectedReplicatedShardId =
            replicatedCollectionInitialClusteringInfo.ShardsByPeers[sourcePeerId].Single();

        var replicateCollectionResponse =
            await _qdrantHttpClient.ReplicateShardsToPeer(
                "http://qdrant-1",
                CancellationToken.None,
                logger: _logger,
                collectionNamesToReplicate: TestCollectionName);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        replicateCollectionResponse.Status.IsSuccess.Should().BeTrue();

        replicateCollectionResponse.Result.AlreadyReplicatedShards.Count.Should().Be(1);
        replicateCollectionResponse.Result.AlreadyReplicatedShards.Should().ContainKey(TestCollectionName);
        replicateCollectionResponse.Result.AlreadyReplicatedShards[TestCollectionName].Count.Should().Be(1);
        replicateCollectionResponse.Result.AlreadyReplicatedShards[TestCollectionName][targetPeerId].Count.Should().Be(1);

        var singleShardReplicationResult = replicateCollectionResponse.Result.ReplicatedShards[0];

        singleShardReplicationResult.IsSuccess.Should().BeTrue();

        singleShardReplicationResult.CollectionName.Should().Be(TestCollectionName);
        singleShardReplicationResult.ShardId.Should().Be(expectedReplicatedShardId);
        singleShardReplicationResult.TargetPeerId.Should().Be(targetPeerId);
        singleShardReplicationResult.SourcePeerId.Should().Be(sourcePeerId);

        var replicatedCollectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        if (replicatedCollectionClusteringInfo.PeerId == targetPeerId)
        {
            // means we got response from target peer

            replicatedCollectionClusteringInfo.RemoteShards.Length.Should().Be(1);
            replicatedCollectionClusteringInfo.LocalShards.Length.Should().Be(2);
        }
        else
        {
            // means we got response from source peer

            replicatedCollectionClusteringInfo.RemoteShards.Length.Should().Be(2);
            replicatedCollectionClusteringInfo.LocalShards.Length.Should().Be(1);
        }
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public async Task ReplicateShardsToClusterNode_AllCollections(int replicationFactor)
    {
        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: (uint)replicationFactor);

        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName2,
            10U,
            replicationFactor: (uint)replicationFactor);

        var replicatedCollection1InitialClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var replicatedCollection2InitialClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName2, CancellationToken.None))
            .EnsureSuccess();

        var targetPeerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();

        var sourcePeerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-2", CancellationToken.None))
            .EnsureSuccess();

        var targetPeerId = targetPeerInfo.PeerId;
        var sourcePeerId = sourcePeerInfo.PeerId;

        var replicateCollectionResponse =
            await _qdrantHttpClient.ReplicateShardsToPeer(
                targetPeerId,
                CancellationToken.None,
                logger: _logger);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName2,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        replicateCollectionResponse.Status.IsSuccess.Should().BeTrue();

        if (replicationFactor == 1)
        {
            // 2 collections
            replicateCollectionResponse.Result.AlreadyReplicatedShards.Count.Should().Be(2);

            foreach (var alreadyReplicatedShard in replicateCollectionResponse.Result.AlreadyReplicatedShards)
            {
                var collectionShardsByPeers = alreadyReplicatedShard.Value;
                
                // 1 target peer
                collectionShardsByPeers.Count.Should().Be(1);
                var peerShards = collectionShardsByPeers.First().Value;

                // one shard for each collection is already replicated to target
                peerShards.Count.Should().Be(replicationFactor);
            }

            replicateCollectionResponse.Result.ReplicatedShards.Count.Should().Be(2);

            foreach (var replicatedCollectionShardsResult in replicateCollectionResponse.Result.ReplicatedShards)
            {
                replicatedCollectionShardsResult.IsSuccess.Should().BeTrue();

                replicatedCollectionShardsResult.CollectionName.Should()
                    .BeOneOf(TestCollectionName, TestCollectionName2);

                replicatedCollectionShardsResult.SourcePeerId.Should().Be(sourcePeerId);
                replicatedCollectionShardsResult.TargetPeerId.Should().Be(targetPeerId);

                if (replicatedCollectionShardsResult.CollectionName == TestCollectionName)
                {
                    var expectedReplicatedShardId =
                        replicatedCollection1InitialClusteringInfo.ShardsByPeers[sourcePeerId].Single();

                    replicatedCollectionShardsResult.ShardId.Should().Be(expectedReplicatedShardId);
                }

                if (replicatedCollectionShardsResult.CollectionName == TestCollectionName2)
                {
                    var expectedReplicatedShardId =
                        replicatedCollection2InitialClusteringInfo.ShardsByPeers[sourcePeerId].Single();

                    replicatedCollectionShardsResult.ShardId.Should().Be(expectedReplicatedShardId);
                }
            }
        }

        if (replicationFactor == 2)
        {
            // If replication factor is 2 we don't actually replicate anything since all shards are already replicated
            replicateCollectionResponse.Result.ReplicatedShards.Count.Should().Be(0);
            
            // 2 collections
            replicateCollectionResponse.Result.AlreadyReplicatedShards.Count.Should().Be(2);
            
            foreach (var alreadyReplicatedShard in replicateCollectionResponse.Result.AlreadyReplicatedShards)
            {
                var collectionShardsByPeers = alreadyReplicatedShard.Value;

                // 1 target peer
                collectionShardsByPeers.Count.Should().Be(1);
                var peerShards = collectionShardsByPeers.First().Value;

                // all shards for each collection are already replicated to target
                peerShards.Count.Should().Be(replicationFactor);
            }
        }

        var firstReplicatedCollectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var secondReplicatedCollectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName2, CancellationToken.None))
            .EnsureSuccess();

        List<GetCollectionClusteringInfoResponse.CollectionClusteringInfo> replicatedCollectionClusteringInfos =
            [firstReplicatedCollectionClusteringInfo, secondReplicatedCollectionClusteringInfo];

        foreach (var replicatedCollectionClusteringInfo in replicatedCollectionClusteringInfos)
        {
            if (replicatedCollectionClusteringInfo.PeerId == targetPeerId)
            {
                // means we got response from target peer

                replicatedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor);

                if (replicationFactor == 1)
                {
                    // when replication factor is 1, the shard gets replicated to a target peer giving total number of shards of 2
                    replicatedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor + 1);
                }
                else
                {
                    // when replication factor is 2 (which is equals to the number of nodes in the cluster),
                    // the shard gets replicated to a target peer, but that peer already contains
                    // that shard so the number of shards on the target peer does not change
                    replicatedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor);
                }
            }
            else
            {
                replicatedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor);

                if (replicationFactor == 1)
                {
                    // when replication factor is 1, the shard gets replicated to a target peer giving total number of shards of 2
                    replicatedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor + 1);
                }
                else
                {
                    // when replication factor is 2 (which is equals to the number of nodes in the cluster),
                    // the shard gets replicated to a target peer, but that peer already contains
                    // that shard so the number of shards on the target peer does not change
                    replicatedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor);
                }
            }
        }
    }

    [Test]
    public async Task ReplicateShardsFromSpecificClusterNode_SpecifiedCollection()
    {
        /*
        This test checks whether the following transition happens when ReplicateShards is called:

        TestCollectionName:

        Node 1   Node 2
          S1       S2
          S3       S4
        --------------- ReplicateShards from Node 2 to Node 1 (one shard)
          S1       S2
          S2       S4
          S3
        --------------- ReplicateShards move one shard from Node 2 to Node 1 (one shard)
          S1       S2
          S2       
          S3       
          S4
        --------------- ReplicateShards from Node 1 to Node 2 (all shards). Untouched shard is S2.
          S1       S1
          S2       S2
          S3       S3
          S4       S4
          
        TestCollectionName2:
        
         Node 1   Node 2
          S1       S2
          S3       S4
        --------------- ReplicateShards all shards for all collections 
          S1       S1
          S2       S2
          S3       S3
          S4       S4
          
        Both collections should have full replication on both shards the end.
        
        */

        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            shardNumber: 4,
            replicationFactor: 1);

        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName2,
            10U,
            shardNumber: 4,
            replicationFactor: 1);

        var sourcePeerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-2", CancellationToken.None))
            .EnsureSuccess();

        var targetPeerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();
        
        var initialSubjectCollectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        // First we replicate one shard for one collection

        var shardToReplicate = initialSubjectCollectionClusteringInfo.PeerId == sourcePeerInfo.PeerId
            ? initialSubjectCollectionClusteringInfo.LocalShards[0].ShardId
            : initialSubjectCollectionClusteringInfo.RemoteShards[0].ShardId;

        var shardToMove = initialSubjectCollectionClusteringInfo.PeerId == sourcePeerInfo.PeerId
            ? initialSubjectCollectionClusteringInfo.LocalShards[1].ShardId
            : initialSubjectCollectionClusteringInfo.RemoteShards[1].ShardId;

        var replicateOneCollectionOneShardResponse =
            await _qdrantHttpClient.ReplicateShards(
                "http://qdrant-2",
                "http://qdrant-1",
                CancellationToken.None,
                logger: _logger,
                collectionNamesToReplicate: [TestCollectionName],
                shardIdsToReplicate: [shardToReplicate]);

        replicateOneCollectionOneShardResponse.Status.IsSuccess.Should().BeTrue();

        var replicatedShardResult = replicateOneCollectionOneShardResponse.Result.ReplicatedShards.Single();
        replicatedShardResult.IsSuccess.Should().BeTrue();
        replicatedShardResult.CollectionName.Should().Be(TestCollectionName);
        replicatedShardResult.SourcePeerId.Should().Be(sourcePeerInfo.PeerId);
        replicatedShardResult.TargetPeerId.Should().Be(targetPeerInfo.PeerId);
        replicatedShardResult.ShardId.Should().Be(shardToReplicate);
        
        replicateOneCollectionOneShardResponse.Result.AlreadyReplicatedShards.Should().BeEmpty();

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        var collectionClusteringInfoAfterShardReplicate =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        if (collectionClusteringInfoAfterShardReplicate.PeerId == sourcePeerInfo.PeerId)
        {
            // means we got response from source peer
            collectionClusteringInfoAfterShardReplicate.LocalShards.Length.Should().Be(2);
            collectionClusteringInfoAfterShardReplicate.RemoteShards.Length.Should().Be(3); // Shard 3 should be replicated

            collectionClusteringInfoAfterShardReplicate.RemoteShards.Select(s => s.ShardId).Should()
                .Contain(shardToReplicate);
        }
        else
        {
            // means we got response from target peer

            collectionClusteringInfoAfterShardReplicate.LocalShards.Length.Should().Be(3); // Shard 3 should be replicated
            collectionClusteringInfoAfterShardReplicate.RemoteShards.Length.Should()
                .Be(2);

            collectionClusteringInfoAfterShardReplicate.LocalShards.Select(s => s.ShardId).Should()
                .Contain(shardToReplicate);
        }

        // The other collection should remain untouched

        var untouchedCollectionClusteringInfoAfterShardReplicate =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName2, CancellationToken.None))
            .EnsureSuccess();

        untouchedCollectionClusteringInfoAfterShardReplicate.LocalShards.Length.Should().Be(2);
        untouchedCollectionClusteringInfoAfterShardReplicate.RemoteShards.Length.Should().Be(2);

        // Then we move another shard for the same collection

        var moveOneCollectionOneShardResponse =
            await _qdrantHttpClient.ReplicateShards(
                "http://qdrant-2",
                "http://qdrant-1",
                CancellationToken.None,
                logger: _logger,
                collectionNamesToReplicate: [TestCollectionName],
                shardIdsToReplicate: [shardToMove],
                isMoveShards: true);

        moveOneCollectionOneShardResponse.Status.IsSuccess.Should().BeTrue();

        var moveShardResult = moveOneCollectionOneShardResponse.Result.ReplicatedShards.Single();
        moveShardResult.IsSuccess.Should().BeTrue();
        moveShardResult.CollectionName.Should().Be(TestCollectionName);
        moveShardResult.SourcePeerId.Should().Be(sourcePeerInfo.PeerId);
        moveShardResult.TargetPeerId.Should().Be(targetPeerInfo.PeerId);
        moveShardResult.ShardId.Should().Be(shardToMove);
        
        moveOneCollectionOneShardResponse.Result.AlreadyReplicatedShards.Should().BeEmpty();

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        var collectionClusteringInfoAfterShardMove =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        if (collectionClusteringInfoAfterShardMove.PeerId == sourcePeerInfo.PeerId)
        {
            // means we got response from source peer
            collectionClusteringInfoAfterShardMove.LocalShards.Length.Should().Be(1); // Shard 4 should be moved
            collectionClusteringInfoAfterShardMove.RemoteShards.Length.Should().Be(4);

            collectionClusteringInfoAfterShardMove.LocalShards.Select(s => s.ShardId).Should()
                .NotContain(shardToMove);
            collectionClusteringInfoAfterShardMove.RemoteShards.Select(s => s.ShardId).Should()
                .Contain(shardToMove);
        }
        else
        {
            // means we got response from target peer

            collectionClusteringInfoAfterShardMove.LocalShards.Length.Should().Be(4); // Shard 4 should be moved
            collectionClusteringInfoAfterShardMove.RemoteShards.Length.Should()
                .Be(1);

            collectionClusteringInfoAfterShardMove.RemoteShards.Select(s => s.ShardId).Should()
                .NotContain(shardToMove);
            collectionClusteringInfoAfterShardMove.LocalShards.Select(s => s.ShardId).Should()
                .Contain(shardToMove);
        }

        // The other collection should remain untouched

        var untouchedCollectionClusteringInfoAfterShardMove =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName2, CancellationToken.None))
            .EnsureSuccess();

        untouchedCollectionClusteringInfoAfterShardMove.LocalShards.Length.Should().Be(2);
        untouchedCollectionClusteringInfoAfterShardMove.RemoteShards.Length.Should().Be(2);

        // Then we replicate all shards for one collection back from target to source peer

        var replicateOneCollectionAllShardsResponse =
            await _qdrantHttpClient.ReplicateShards(
                "http://qdrant-1",
                "http://qdrant-2",
                CancellationToken.None,
                logger: _logger,
                collectionNamesToReplicate: [TestCollectionName],
                shardIdsToReplicate: null,
                isMoveShards: false);

        replicateOneCollectionAllShardsResponse.Status.IsSuccess.Should().BeTrue();

        var replicateAllShardsResult = replicateOneCollectionAllShardsResponse.Result.ReplicatedShards;

        foreach (var replicateShardResult in replicateAllShardsResult)
        {
            replicateShardResult.IsSuccess.Should().BeTrue();
            replicateShardResult.CollectionName.Should().Be(TestCollectionName);
            
            // In this case source and target peers are swapped
            replicateShardResult.SourcePeerId.Should().Be(targetPeerInfo.PeerId);
            replicateShardResult.TargetPeerId.Should().Be(sourcePeerInfo.PeerId);
        }

        // 1 collection
        replicateOneCollectionAllShardsResponse.Result.AlreadyReplicatedShards.Count.Should().Be(1);
        replicateOneCollectionAllShardsResponse.Result.AlreadyReplicatedShards.Should().ContainKey(TestCollectionName);

        //S2 already exists on the target node (which is source since target and source are swapped)
        replicateOneCollectionAllShardsResponse.Result
            .AlreadyReplicatedShards[TestCollectionName][sourcePeerInfo.PeerId].Count.Should().Be(1);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        var collectionClusteringInfoAfterAllShardsReplicate =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        collectionClusteringInfoAfterAllShardsReplicate.LocalShards.Length.Should().Be(4);
        collectionClusteringInfoAfterAllShardsReplicate.RemoteShards.Length.Should().Be(4);

        // The other collection should remain untouched

        var untouchedCollectionClusteringInfoAfterFinalReplication =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName2, CancellationToken.None))
            .EnsureSuccess();

        untouchedCollectionClusteringInfoAfterFinalReplication.LocalShards.Length.Should().Be(2);
        untouchedCollectionClusteringInfoAfterFinalReplication.RemoteShards.Length.Should().Be(2);
    }

    [Test]
    public async Task ReplicateShardsFromSpecificClusterNode_AllCollections()
    {
        /*
        This test checks whether the following transition happens when ReplicateShards is called:

        All collections:

        Node 1   Node 2
          S1       S2
          S3       S4
        --------------- ReplicateShards from Node 2 to Node 1 (all shards)
          S1       S2
          S2       S4
          S3
          S4      
        --------------- ReplicateShards from Node 1 to Node 2 (all shards)
          S1       S1
          S2       S2
          S3       S3
          S4       S4

        Both collections should have full replication on both shards the end.

        S2 and S4 should stay untouched here since they are already on Node2.

        */

        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            shardNumber: 4,
            replicationFactor: 1);

        await CreateTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName2,
            10U,
            shardNumber: 4,
            replicationFactor: 1);

        var sourcePeerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-2", CancellationToken.None))
            .EnsureSuccess();

        var targetPeerInfo =
            (await _qdrantHttpClient.GetPeerInfo("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();
        
        // Replicate all shards for all collections from node 2 to node 1

        var replicateAllCollectionAllShardResponse =
            await _qdrantHttpClient.ReplicateShards(
                "http://qdrant-2",
                "http://qdrant-1",
                CancellationToken.None,
                logger: _logger,
                collectionNamesToReplicate: null,
                shardIdsToReplicate: null);

        replicateAllCollectionAllShardResponse.Status.IsSuccess.Should().BeTrue();

        var replicateAllShardsResult = replicateAllCollectionAllShardResponse.Result.ReplicatedShards;
        
        foreach(var replicateShardResult in replicateAllShardsResult)
        {
            replicateShardResult.IsSuccess.Should().BeTrue();
            replicateShardResult.CollectionName.Should().BeOneOf(TestCollectionName, TestCollectionName2);
            
            replicateShardResult.SourcePeerId.Should().Be(sourcePeerInfo.PeerId);
            replicateShardResult.TargetPeerId.Should().Be(targetPeerInfo.PeerId);
        }
        
        replicateAllCollectionAllShardResponse.Result.AlreadyReplicatedShards.Should().BeEmpty();

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName2,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        foreach (var collectionName in new[] { TestCollectionName, TestCollectionName2 })
        {
            var collectionClusteringInfoAfterFirstReplication =
                (await _qdrantHttpClient.GetCollectionClusteringInfo(collectionName, CancellationToken.None))
                .EnsureSuccess();

            if (collectionClusteringInfoAfterFirstReplication.PeerId == sourcePeerInfo.PeerId)
            {
                // means we got response from source peer
                collectionClusteringInfoAfterFirstReplication.LocalShards.Length.Should().Be(2);
                collectionClusteringInfoAfterFirstReplication.RemoteShards.Length.Should().Be(4);
            }
            else
            {
                // means we got response from target peer
                collectionClusteringInfoAfterFirstReplication.LocalShards.Length.Should().Be(4);
                collectionClusteringInfoAfterFirstReplication.RemoteShards.Length.Should().Be(2);
            }
        }

        // Replicate all shards for all collections back from target to source

        var replicateAllCollectionAllShardBackResponse =
            await _qdrantHttpClient.ReplicateShards(
                "http://qdrant-1",
                "http://qdrant-2",
                CancellationToken.None,
                logger: _logger,
                collectionNamesToReplicate: null,
                shardIdsToReplicate: null);

        replicateAllCollectionAllShardBackResponse.Status.IsSuccess.Should().BeTrue();

        var replicateAllShardsBackResult = replicateAllCollectionAllShardBackResponse.Result.ReplicatedShards;

        foreach (var replicateShardResult in replicateAllShardsBackResult)
        {
            replicateShardResult.IsSuccess.Should().BeTrue();
            replicateShardResult.CollectionName.Should().BeOneOf(TestCollectionName, TestCollectionName2);

            // In this case source and target peers are swapped
            replicateShardResult.SourcePeerId.Should().Be(targetPeerInfo.PeerId);
            replicateShardResult.TargetPeerId.Should().Be(sourcePeerInfo.PeerId);
        }

        // 2 collections
        replicateAllCollectionAllShardBackResponse.Result.AlreadyReplicatedShards.Count.Should().Be(2);
        replicateAllCollectionAllShardBackResponse.Result.AlreadyReplicatedShards
            .Should().ContainKeys(TestCollectionName, TestCollectionName2);

        replicateAllCollectionAllShardBackResponse.Result
            .AlreadyReplicatedShards[TestCollectionName][sourcePeerInfo.PeerId].Count.Should().Be(2);
        
        replicateAllCollectionAllShardBackResponse.Result
            .AlreadyReplicatedShards[TestCollectionName2][sourcePeerInfo.PeerId].Count.Should().Be(2);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName2,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        foreach (var collectionName in new[] { TestCollectionName, TestCollectionName2 })
        {
            var collectionClusteringInfoAfterFirstReplication =
                (await _qdrantHttpClient.GetCollectionClusteringInfo(collectionName, CancellationToken.None))
                .EnsureSuccess();

            collectionClusteringInfoAfterFirstReplication.LocalShards.Length.Should().Be(4);
            collectionClusteringInfoAfterFirstReplication.RemoteShards.Length.Should().Be(4);
        }
    }
}
