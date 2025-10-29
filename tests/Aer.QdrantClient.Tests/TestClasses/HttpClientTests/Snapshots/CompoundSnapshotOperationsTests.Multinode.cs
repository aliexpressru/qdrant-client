using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots;

#if !DEBUG
[Ignore("Multi-node tests are ignored since we don't have multi-node test environment in CI/CD")]
#endif
public class CompoundSnapshotOperationsTestsMultiNode : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClientClusterNode1;
    private QdrantHttpClient _qdrantHttpClientClusterNode2;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();

        _qdrantHttpClientClusterNode1 = GetClusterClient(ClusterNode.First);
        _qdrantHttpClientClusterNode2 = GetClusterClient(ClusterNode.Second);
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage(_qdrantHttpClientClusterNode1);
        await ResetStorage(_qdrantHttpClientClusterNode2);
    }

    [Test]
    public async Task ListSnapshots()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClientClusterNode1, TestCollectionName);
        await PrepareCollection<TestPayload>(_qdrantHttpClientClusterNode1, TestCollectionName2);

        var collection1Node1Clustering = (await _qdrantHttpClientClusterNode1.GetCollectionClusteringInfo(
            TestCollectionName,
            CancellationToken.None)).EnsureSuccess();

        var collection1Node2Clustering = (await _qdrantHttpClientClusterNode2.GetCollectionClusteringInfo(
            TestCollectionName,
            CancellationToken.None)).EnsureSuccess();

        await _qdrantHttpClientClusterNode2.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None);

        var collection2Node1Clustering = (await _qdrantHttpClientClusterNode1.GetCollectionClusteringInfo(
            TestCollectionName2,
            CancellationToken.None)).EnsureSuccess();

        var collection2Node2Clustering = (await _qdrantHttpClientClusterNode2.GetCollectionClusteringInfo(
            TestCollectionName2,
            CancellationToken.None)).EnsureSuccess();

        // Shard snapshots on both nodes

        var createShardSnapshotResult11 = (await _qdrantHttpClientClusterNode1.CreateShardSnapshot(
            TestCollectionName,
            collection1Node1Clustering.LocalShards[0].ShardId,
            CancellationToken.None)).EnsureSuccess();

        var createShardSnapshotResult12 = (await _qdrantHttpClientClusterNode1.CreateShardSnapshot(
            TestCollectionName2,
            collection2Node1Clustering.LocalShards[0].ShardId,
            CancellationToken.None)).EnsureSuccess();

        var createShardSnapshotResult21 = (await _qdrantHttpClientClusterNode2.CreateShardSnapshot(
            TestCollectionName,
            collection1Node2Clustering.LocalShards[0].ShardId,
            CancellationToken.None)).EnsureSuccess();

        var createShardSnapshotResult22 = (await _qdrantHttpClientClusterNode2.CreateShardSnapshot(
            TestCollectionName2,
            collection2Node2Clustering.LocalShards[0].ShardId,
            CancellationToken.None)).EnsureSuccess();

        // Collection snapshots on both nodes

        var createCollectionSnapshotResult11 =
            (await _qdrantHttpClientClusterNode1.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var createCollectionSnapshotResult12 =
            (await _qdrantHttpClientClusterNode1.CreateCollectionSnapshot(TestCollectionName2, CancellationToken.None))
            .EnsureSuccess();

        var createCollectionSnapshotResult21 =
            (await _qdrantHttpClientClusterNode2.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var createCollectionSnapshotResult22 =
            (await _qdrantHttpClientClusterNode2.CreateCollectionSnapshot(TestCollectionName2, CancellationToken.None))
            .EnsureSuccess();

        // List snapshots on both nodes. We don't include storage snapshots since they are not supported for multi-node clusters

        var listAllSnapshotsResult1 = await _qdrantHttpClientClusterNode1.ListAllSnapshots(
            CancellationToken.None,
            includeStorageSnapshots: false);
        
        var listAllSnapshotsResult2 = await _qdrantHttpClientClusterNode2.ListAllSnapshots(
            CancellationToken.None,
            includeStorageSnapshots: false);

        AssertSnapshots(
            listAllSnapshotsResult1,
            createShardSnapshotResult11,
            createShardSnapshotResult12,
            createCollectionSnapshotResult11,
            createCollectionSnapshotResult12);

        AssertSnapshots(
            listAllSnapshotsResult2,
            createShardSnapshotResult21,
            createShardSnapshotResult22,
            createCollectionSnapshotResult21,
            createCollectionSnapshotResult22);


        static void AssertSnapshots(
            ListSnapshotsResponse listAllSnapshotsResult,

            SnapshotInfo collection1ShardSnapshot,
            SnapshotInfo collection2ShardSnapshot,

            SnapshotInfo collection1Snapshot,
            SnapshotInfo collection2Snapshot
        )
        {
            listAllSnapshotsResult.Status.IsSuccess.Should().BeTrue();
            listAllSnapshotsResult.Result.Should().NotBeNull();

            listAllSnapshotsResult.Result.Should().HaveCount(4); // 2 collection + 2 shard

            // check shard snapshots

            var shardSnapshots =
                listAllSnapshotsResult.Result.Where(s => s.SnapshotType == SnapshotType.Shard).ToList();
            shardSnapshots.Should().HaveCount(2);

            shardSnapshots.Should().ContainSingle(s =>
                s.Name == collection1ShardSnapshot.Name && s.Checksum == collection1ShardSnapshot.Checksum);

            shardSnapshots.Should().ContainSingle(s =>
                s.Name == collection2ShardSnapshot.Name && s.Checksum == collection2ShardSnapshot.Checksum);

            // check collection snapshots

            var collectionSnapshots =
                listAllSnapshotsResult.Result.Where(s => s.SnapshotType == SnapshotType.Collection).ToList();
            collectionSnapshots.Should().HaveCount(2);

            collectionSnapshots.Should().ContainSingle(s =>
                s.Name == collection1Snapshot.Name
                && s.Checksum == collection1Snapshot.Checksum);

            collectionSnapshots.Should().ContainSingle(s =>
                s.Name == collection2Snapshot.Name
                && s.Checksum == collection2Snapshot.Checksum);
        }

        // Cleanup snapshots

        (await _qdrantHttpClientClusterNode1.DeleteAllCollectionShardSnapshots(
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClientClusterNode2.DeleteAllCollectionShardSnapshots(
                CancellationToken.None)).EnsureSuccess();
        
        (await _qdrantHttpClientClusterNode1.DeleteAllCollectionSnapshots(CancellationToken.None)).EnsureSuccess();
        (await _qdrantHttpClientClusterNode2.DeleteAllCollectionSnapshots(CancellationToken.None)).EnsureSuccess();
    }

    [Test]
    public async Task DeleteSnapshots()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClientClusterNode1, TestCollectionName);
        await PrepareCollection<TestPayload>(_qdrantHttpClientClusterNode1, TestCollectionName2);
        
        var collection1Node1Clustering = (await _qdrantHttpClientClusterNode1.GetCollectionClusteringInfo(
            TestCollectionName,
            CancellationToken.None)).EnsureSuccess();

        var collection1Node2Clustering = (await _qdrantHttpClientClusterNode2.GetCollectionClusteringInfo(
            TestCollectionName,
            CancellationToken.None)).EnsureSuccess();

        await _qdrantHttpClientClusterNode2.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None);

        var collection2Node1Clustering = (await _qdrantHttpClientClusterNode1.GetCollectionClusteringInfo(
            TestCollectionName2,
            CancellationToken.None)).EnsureSuccess();

        var collection2Node2Clustering = (await _qdrantHttpClientClusterNode2.GetCollectionClusteringInfo(
            TestCollectionName2,
            CancellationToken.None)).EnsureSuccess();

        // Shard snapshots on both nodes

        (await _qdrantHttpClientClusterNode1.CreateShardSnapshot(
            TestCollectionName,
            collection1Node1Clustering.LocalShards[0].ShardId,
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClientClusterNode1.CreateShardSnapshot(
            TestCollectionName2,
            collection2Node1Clustering.LocalShards[0].ShardId,
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClientClusterNode2.CreateShardSnapshot(
            TestCollectionName,
            collection1Node2Clustering.LocalShards[0].ShardId,
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClientClusterNode2.CreateShardSnapshot(
            TestCollectionName2,
            collection2Node2Clustering.LocalShards[0].ShardId,
            CancellationToken.None)).EnsureSuccess();

        // Collection snapshots on both nodes

        (await _qdrantHttpClientClusterNode1.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
        .EnsureSuccess();

        (await _qdrantHttpClientClusterNode1.CreateCollectionSnapshot(TestCollectionName2, CancellationToken.None))
        .EnsureSuccess();

        (await _qdrantHttpClientClusterNode2.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
        .EnsureSuccess();

        (await _qdrantHttpClientClusterNode2.CreateCollectionSnapshot(TestCollectionName2, CancellationToken.None))
        .EnsureSuccess();

        // delete shard snapshots

        var deleteFirstNodeShardSnapshotsResult = await _qdrantHttpClientClusterNode1.DeleteAllCollectionShardSnapshots(
            CancellationToken.None);

        var deleteSecondNodeShardSnapshotsResult = await _qdrantHttpClientClusterNode2.DeleteAllCollectionShardSnapshots(
            CancellationToken.None);

        deleteFirstNodeShardSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        deleteFirstNodeShardSnapshotsResult.Result.Should().BeTrue();

        deleteSecondNodeShardSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        deleteSecondNodeShardSnapshotsResult.Result.Should().BeTrue();

        await Task.Delay(TimeSpan.FromMilliseconds(500)); // wait for deletion to propagate
        
        var listFirstNodeAllSnapshotsResult =
            (await _qdrantHttpClientClusterNode1.ListAllSnapshots(CancellationToken.None)).EnsureSuccess();

        listFirstNodeAllSnapshotsResult.Should().HaveCount(2); // 2 collection
        listFirstNodeAllSnapshotsResult.Count(s => s.SnapshotType == SnapshotType.Storage)
            .Should().Be(0);
        listFirstNodeAllSnapshotsResult.Count(s => s.SnapshotType == SnapshotType.Shard)
            .Should().Be(0);

        var listSecondNodeAllSnapshotsResult =
            (await _qdrantHttpClientClusterNode2.ListAllSnapshots(CancellationToken.None))
            .EnsureSuccess();

        listSecondNodeAllSnapshotsResult.Should().HaveCount(2); // 2 collection
        listSecondNodeAllSnapshotsResult.Count(s => s.SnapshotType == SnapshotType.Storage)
            .Should().Be(0);
        listSecondNodeAllSnapshotsResult.Count(s => s.SnapshotType == SnapshotType.Shard)
            .Should().Be(0);

        // delete collection snapshots

        var deleteFirstNodeCollectionSnapshotsResult = 
            await _qdrantHttpClientClusterNode1.DeleteAllCollectionSnapshots(
            CancellationToken.None);

        var deleteSecondNodeCollectionSnapshotsResult =
            await _qdrantHttpClientClusterNode2.DeleteAllCollectionSnapshots(
                CancellationToken.None);

        deleteFirstNodeCollectionSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        deleteFirstNodeCollectionSnapshotsResult.Result.Should().BeTrue();

        deleteSecondNodeCollectionSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        deleteSecondNodeCollectionSnapshotsResult.Result.Should().BeTrue();

        await Task.Delay(TimeSpan.FromMilliseconds(500)); // wait for deletion to propagate

        // Check no snapshots left
        
        listFirstNodeAllSnapshotsResult =
            (await _qdrantHttpClientClusterNode1.ListAllSnapshots(CancellationToken.None)).EnsureSuccess();

        listSecondNodeAllSnapshotsResult =
            (await _qdrantHttpClientClusterNode2.ListAllSnapshots(CancellationToken.None))
            .EnsureSuccess();

        listFirstNodeAllSnapshotsResult.Should().HaveCount(0);
        listSecondNodeAllSnapshotsResult.Should().HaveCount(0);
    }
}
