using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Infrastructure;
using Aer.QdrantClient.Tests.Model;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

#if !DEBUG
[Ignore("I didn't find a way to configure both single-node and a multi-node cluster in "
+"GitHub actions so these tests will run only locally")]
#endif
public class ClusterCompoundOperationsTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;
    private ILogger _logger;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();
        _qdrantHttpClient = GetClusterClient();

        _logger = ServiceProvider.GetRequiredService<ILogger<ClusterCompoundOperationsTests>>();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage(_qdrantHttpClient, isDeleteCollectionFiles: false);
    }

    [Test]
    public async Task GetPeerInfoByUriSubstring_MoreThanOneNodeFound()
    {
        await CreateSmallTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U);

        var collectionPeerInfoAct =
            async () => await _qdrantHttpClient.GetPeerInfoByUriSubstring("://", CancellationToken.None);

        await collectionPeerInfoAct.Should().ThrowAsync<QdrantMoreThanOnePeerFoundForUriSubstringException>();
    }

    [Test]
    public async Task GetPeerInfoByUriSubstring_Success()
    {
        await CreateSmallTestShardedCollection(_qdrantHttpClient, TestCollectionName, 10U);

        var clusterInfo =
            (await _qdrantHttpClient.GetClusterInfo(CancellationToken.None)).EnsureSuccess();

        var expectedPeer = clusterInfo.ParsedPeers
            .Single(p => p.Value.Uri.Contains("http://qdrant-1"));

        var otherPeer = clusterInfo.ParsedPeers
            .Single(p => p.Key != expectedPeer.Key);

        var collectionPeerInfoResponse =
            await _qdrantHttpClient.GetPeerInfoByUriSubstring("http://qdrant-1", CancellationToken.None);

        collectionPeerInfoResponse.Status.IsSuccess.Should().BeTrue();

        collectionPeerInfoResponse.Result.PeerId.Should().Be(expectedPeer.Key);
        collectionPeerInfoResponse.Result.OtherPeerIds.Count.Should().Be(1);
        collectionPeerInfoResponse.Result.OtherPeerIds.Single().Should().Be(otherPeer.Key);
    }

    [Test]
    public async Task IsPeerEmpty_Success()
    {
        const uint vectorSize = 10;

        var clusterInfo = (await _qdrantHttpClient.GetClusterInfo(CancellationToken.None)).EnsureSuccess();

        var firstPeerUri = clusterInfo.Peers.First().Value.Uri;

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

        (await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = upsertPoints
            },
            CancellationToken.None,
            isWaitForResult: true,
            ordering: OrderingType.Strong)).EnsureSuccess();

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        checkPeerEmptyResult = await _qdrantHttpClient.CheckIsPeerEmpty(firstPeerUri, CancellationToken.None);

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
        await CreateSmallTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: (uint) replicationFactor);

        await CreateSmallTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName2,
            10U,
            replicationFactor: (uint) replicationFactor);

        var peerInfo =
            (await _qdrantHttpClient.GetPeerInfoByUriSubstring("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();

        var peerIdToDrain = peerInfo.PeerId;

        var drainPeerResponse =
            await _qdrantHttpClient.DrainPeer(
                "http://qdrant-1",
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
        await CreateSmallTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: (uint) replicationFactor);

        await CreateSmallTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName2,
            10U,
            replicationFactor: (uint) replicationFactor);

        var peerInfo =
            (await _qdrantHttpClient.GetPeerInfoByUriSubstring("http://qdrant-1", CancellationToken.None))
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
    [TestCase(1)]
    [TestCase(2)]
    public async Task ReplicateShardsToClusterNode_SelectedCollection(int replicationFactor)
    {
        await CreateSmallTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: (uint) replicationFactor);

        await CreateSmallTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName2,
            10U,
            replicationFactor: (uint) replicationFactor);

        var peerInfo =
            (await _qdrantHttpClient.GetPeerInfoByUriSubstring("http://qdrant-1", CancellationToken.None))
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
        replicateCollectionResponse.Result.Should().BeTrue();

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
    [TestCase(1)]
    [TestCase(2)]
    public async Task ReplicateShardsToClusterNode_AllCollections(int replicationFactor)
    {
        await CreateSmallTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: (uint) replicationFactor);

        await CreateSmallTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName2,
            10U,
            replicationFactor: (uint) replicationFactor);

        var peerInfo =
            (await _qdrantHttpClient.GetPeerInfoByUriSubstring("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();

        var targetPeerId = peerInfo.PeerId;

        var replicateCollectionResponse =
            await _qdrantHttpClient.ReplicateShardsToPeer(
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

        replicateCollectionResponse.Status.IsSuccess.Should().BeTrue();
        replicateCollectionResponse.Result.Should().BeTrue();

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
    public async Task ReplicateShardsToClusterNode_RespectReplicationFactor()
    {
        var replicationFactor = 2;
        await CreateSmallTestShardedCollection(
            _qdrantHttpClient,
            TestCollectionName,
            10U,
            replicationFactor: (uint) replicationFactor);

        var peerInfo =
            (await _qdrantHttpClient.GetPeerInfoByUriSubstring("http://qdrant-1", CancellationToken.None))
            .EnsureSuccess();

        var targetPeerId = peerInfo.PeerId;

        var testLogger = new TestMicrosoftLogger();

        var replicateCollectionResponse =
            await _qdrantHttpClient.ReplicateShardsToPeer(
                "http://qdrant-1",
                CancellationToken.None,
                logger: testLogger,
                collectionNamesToReplicate: TestCollectionName,
                isIgnoreReplicationFactor: false);

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        replicateCollectionResponse.Status.IsSuccess.Should().BeTrue();
        replicateCollectionResponse.Result.Should().BeTrue();

        var replicatedCollectionClusteringInfo =
            (await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        // when isIgnoreReplicationFactor is false the shard does not get replicated
        // so the number of shards on the target peer does not change

        if (replicatedCollectionClusteringInfo.PeerId == targetPeerId)
        {
            // means we got response from target peer

            replicatedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor);
            replicatedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor);
        }
        else
        {
            // means we got response from source peer

            replicatedCollectionClusteringInfo.LocalShards.Length.Should().Be(replicationFactor);
            replicatedCollectionClusteringInfo.RemoteShards.Length.Should().Be(replicationFactor);
        }

        testLogger.WrittenEvents.Should().Contain(
            m => m.message.Contains("Collection 'test_collection' shard 1 already replicated 2 times")
        );

        testLogger.WrittenEvents.Should().Contain(
            m => m.message.Contains("Collection 'test_collection' shard 2 already replicated 2 times")
        );
    }
}
