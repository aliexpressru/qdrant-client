using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots;

public class CompoundSnapshotOperationsTests : QdrantTestsBase
{
    // NOTE: since we don't have a cluster in test and thus have only one shard
    // these tests basically repeat the tests from CollectionSnapshotTests but using shard methods
    private QdrantHttpClient _qdrantHttpClient;

    // since we don't have a cluster in test and thus have only one shard which is always 0
    private const int SINGLE_SHARD_ID = 0;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();

        _qdrantHttpClient = ServiceProvider.GetRequiredService<QdrantHttpClient>();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage(_qdrantHttpClient);
    }

    [Test]
    public async Task ListSnapshots()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName2);

        var createStorageSnapshotResult =
            (await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None)).EnsureSuccess();

        var createShardSnapshotResult1 = (await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None)).EnsureSuccess();

        var createShardSnapshotResult2 = (await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName2,
            SINGLE_SHARD_ID,
            CancellationToken.None)).EnsureSuccess();

        var createCollectionSnapshotResult1 =
            (await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var createCollectionSnapshotResult2 =
            (await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName2, CancellationToken.None))
            .EnsureSuccess();

        var listAllSnapshotsResult = await _qdrantHttpClient.ListAllSnapshots(CancellationToken.None);

        listAllSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listAllSnapshotsResult.Result.Should().NotBeNull();

        listAllSnapshotsResult.Result.Should().HaveCount(5); // 2 collection + 2 shard + 1 storage

        listAllSnapshotsResult.Result.Count(s => s.SnapshotType == SnapshotType.Storage)
            .Should().Be(1);

        // check storage snapshot

        var storageSnapshot = listAllSnapshotsResult.Result.Single(s => s.SnapshotType == SnapshotType.Storage);

        storageSnapshot.Name.Should().Be(createStorageSnapshotResult.Name);
        storageSnapshot.Checksum.Should().Be(createStorageSnapshotResult.Checksum);

        // check shard snapshots

        var shardSnapshots = listAllSnapshotsResult.Result.Where(s => s.SnapshotType == SnapshotType.Shard).ToList();
        shardSnapshots.Should().HaveCount(2);

        shardSnapshots.Should().ContainSingle(s =>
            s.Name == createShardSnapshotResult1.Name && s.Checksum == createShardSnapshotResult1.Checksum);
        shardSnapshots.Should().ContainSingle(s =>
            s.Name == createShardSnapshotResult2.Name && s.Checksum == createShardSnapshotResult2.Checksum);

        // check collection snapshots

        var collectionSnapshots =
            listAllSnapshotsResult.Result.Where(s => s.SnapshotType == SnapshotType.Collection).ToList();
        collectionSnapshots.Should().HaveCount(2);

        collectionSnapshots.Should().ContainSingle(s =>
            s.Name == createCollectionSnapshotResult1.Name && s.Checksum == createCollectionSnapshotResult1.Checksum);
        collectionSnapshots.Should().ContainSingle(s =>
            s.Name == createCollectionSnapshotResult2.Name && s.Checksum == createCollectionSnapshotResult2.Checksum);
    }

    [Test]
    public async Task DeleteSnapshots()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName2);

        (await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName2,
            SINGLE_SHARD_ID,
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        (await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName2, CancellationToken.None))
            .EnsureSuccess();

        var listAllSnapshotsResult = (await _qdrantHttpClient.ListAllSnapshots(CancellationToken.None)).EnsureSuccess();

        listAllSnapshotsResult.Should().HaveCount(5); // 2 collection + 2 shard + 1 storage

        listAllSnapshotsResult.Count(s => s.SnapshotType == SnapshotType.Storage)
            .Should().Be(1);
        listAllSnapshotsResult.Count(s => s.SnapshotType == SnapshotType.Shard)
            .Should().Be(2);
        listAllSnapshotsResult.Count(s => s.SnapshotType == SnapshotType.Collection)
            .Should().Be(2);

        // delete storage snapshot

        var deleteStorageSnapshotResult = await _qdrantHttpClient.DeleteAllStorageSnapshots(
            CancellationToken.None);

        deleteStorageSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteStorageSnapshotResult.Result.Should().BeTrue();

        listAllSnapshotsResult = (await _qdrantHttpClient.ListAllSnapshots(CancellationToken.None)).EnsureSuccess();

        listAllSnapshotsResult.Should().HaveCount(4); // 2 collection + 2 shard
        listAllSnapshotsResult.Count(s => s.SnapshotType == SnapshotType.Storage)
            .Should().Be(0);

        // delete shard snapshots

        var deleteShardSnapshotsResult = await _qdrantHttpClient.DeleteAllCollectionShardSnapshots(
            CancellationToken.None);

        deleteShardSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        deleteShardSnapshotsResult.Result.Should().BeTrue();

        listAllSnapshotsResult = (await _qdrantHttpClient.ListAllSnapshots(CancellationToken.None)).EnsureSuccess();

        listAllSnapshotsResult.Should().HaveCount(2); // 2 collection
        listAllSnapshotsResult.Count(s => s.SnapshotType == SnapshotType.Storage)
            .Should().Be(0);
        listAllSnapshotsResult.Count(s => s.SnapshotType == SnapshotType.Shard)
            .Should().Be(0);

        // delete collection snapshots
        var deleteCollectionSnapshotsResult = await _qdrantHttpClient.DeleteAllCollectionSnapshots(
            CancellationToken.None);

        deleteCollectionSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        deleteCollectionSnapshotsResult.Result.Should().BeTrue();

        listAllSnapshotsResult = (await _qdrantHttpClient.ListAllSnapshots(CancellationToken.None)).EnsureSuccess();

        listAllSnapshotsResult.Should().HaveCount(0);
    }
}
