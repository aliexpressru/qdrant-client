﻿using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots;

// Replace attribute when shard snapshot api will be functional
// [Ignore("Since snapshot has a minimal size of roughly 100MB these tests are time-consuming"
// +" and we only run these tests on local machine, not in CI")]
[Ignore("Shard snapshot API seems to be buggy, so we skip testing it altogether")]
public class ShardSnapshotTests : QdrantTestsBase
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
        await ResetStorage();
    }

    #region Invalid cases and non-existent colelctions

    [Test]
    public async Task NonExistentCollectionSnapshotsOperations()
    {
        // list
        var listSnapshotsResult = await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeFalse();

        // create
        var createSnapshotResult = await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        createSnapshotResult.Status.IsSuccess.Should().BeFalse();

        // delete

        var deleteSnapshotResult = await _qdrantHttpClient.DeleteShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            "non_existent_snapshot_name",
            CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeFalse();

        // recover

        var recoverFromSnapshotInvalidUrlResult = await _qdrantHttpClient.RecoverShardFromSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            "non_existent_snapshot_uri",
            CancellationToken.None);

        recoverFromSnapshotInvalidUrlResult.Status.IsSuccess.Should().BeFalse();

        var recoverFromSnapshotNonExistentSnapshotResult = await _qdrantHttpClient.RecoverShardFromSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            "file:///qdrant/snapshots/test_collection-2022-08-04-10-49-10.snapshot",
            CancellationToken.None);

        recoverFromSnapshotNonExistentSnapshotResult.Status.IsSuccess.Should().BeFalse();

        // download

        var downloadSnapshotResult = await _qdrantHttpClient.DownloadShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            "non_existent_snapshot_name",
            CancellationToken.None);

        downloadSnapshotResult.Status.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task ListSnapshots_ExistingCollectionNoSnapshotsYet()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var listSnapshotsResult = await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Length.Should().Be(0);
    }

    #endregion

    [Test]
    public async Task CreateSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        createSnapshotResult.Status.IsSuccess.Should().BeTrue();
        createSnapshotResult.Result.Name.Should().Contain(TestCollectionName);
        createSnapshotResult.Result.Size.Should().BeGreaterThan(0);
        createSnapshotResult.Result.SizeMegabytes.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task ListSnapshots()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        // create first snapshot

        var createSnapshotResult = await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        var listSnapshotsResult = await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        createSnapshotResult.EnsureSuccess();

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();

        listSnapshotsResult.Result.Length.Should().Be(1); // one snapshot created

        listSnapshotsResult.Result.Single().Name.Should().Be(createSnapshotResult.Result.Name);
        listSnapshotsResult.Result.Single().Size.Should().Be(createSnapshotResult.Result.Size);

        // create second snapshot

        var createSecondSnapshotResult =
            await _qdrantHttpClient.CreateShardSnapshot(
                TestCollectionName,
                SINGLE_SHARD_ID,
                CancellationToken.None);

        createSecondSnapshotResult.EnsureSuccess();

        listSnapshotsResult = await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Length.Should().Be(2); // two snapshots so far

        var newSnapshot = listSnapshotsResult.Result.Single(n => n.Name != createSnapshotResult.Result.Name);

        newSnapshot.Name.Should().Be(createSecondSnapshotResult.Result.Name);
        newSnapshot.Size.Should().Be(createSecondSnapshotResult.Result.Size);
    }

    [Test]
    public async Task DeleteSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        // create first snapshot

        var createSnapshotResult = await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        var listSnapshotsResult = await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        createSnapshotResult.EnsureSuccess();
        listSnapshotsResult.EnsureSuccess();

        listSnapshotsResult.Result.Length.Should().Be(1);

        var deleteSnapshotResult =
            await _qdrantHttpClient.DeleteShardSnapshot(
                TestCollectionName,
                SINGLE_SHARD_ID,
                createSnapshotResult.Result.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        listSnapshotsResult = await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        listSnapshotsResult.EnsureSuccess();

        listSnapshotsResult.Result.Length.Should().Be(0);

        // create two snapshots and delete one

        var createFirstSnapshotResult =
            await _qdrantHttpClient.CreateShardSnapshot(
                TestCollectionName,
                SINGLE_SHARD_ID,
                CancellationToken.None);

        var createSecondSnapshotResult =
            await _qdrantHttpClient.CreateShardSnapshot(
                TestCollectionName,
                SINGLE_SHARD_ID,
                CancellationToken.None);

        createFirstSnapshotResult.EnsureSuccess();
        createSecondSnapshotResult.EnsureSuccess();

        listSnapshotsResult = await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        listSnapshotsResult.EnsureSuccess();

        listSnapshotsResult.Result.Length.Should().Be(2);

        deleteSnapshotResult =
            await _qdrantHttpClient.DeleteShardSnapshot(
                TestCollectionName,
                SINGLE_SHARD_ID,
                createFirstSnapshotResult.Result.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        listSnapshotsResult = await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        listSnapshotsResult.EnsureSuccess();

        listSnapshotsResult.Result.Length.Should().Be(1);

        listSnapshotsResult.Result.Single().Name.Should().Be(createSecondSnapshotResult.Result.Name);
    }

    [Test]
    public async Task DownloadSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        createSnapshotResult.EnsureSuccess();

        var downloadSnapshotResponse = await _qdrantHttpClient.DownloadShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            createSnapshotResult.Result.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeTrue();
        downloadSnapshotResponse.Result.SnapshotSizeBytes.Should().Be(createSnapshotResult.Result.Size);
    }

    [Test]
    public async Task DownloadSnapshot_AfterCollectionIsDeleted()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        createSnapshotResult.EnsureSuccess();

        var deleteCollectionResult = await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);
        deleteCollectionResult.EnsureSuccess();

        // after explicit collection delete the snapshot download will not be accessible with message saying that the collection does not exist

        var downloadSnapshotResponse = await _qdrantHttpClient.DownloadShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            createSnapshotResult.Result.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeFalse();
        downloadSnapshotResponse.Status.Error.Should().Contain("Collection `test_collection` doesn't exist!");

        // create collection and see if we will be able to download previous snapshot

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        downloadSnapshotResponse = await _qdrantHttpClient.DownloadShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            createSnapshotResult.Result.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeTrue();
        downloadSnapshotResponse.Result.SnapshotSizeBytes.Should().Be(createSnapshotResult.Result.Size);
    }

    [Test]
    public async Task RecoverFromSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        createSnapshotResult.EnsureSuccess();

        // delete collection

        var deleteCollectionResponse = await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);

        deleteCollectionResponse.EnsureSuccess();

        var listCollectionsResult = await _qdrantHttpClient.ListCollections(CancellationToken.None);

        listCollectionsResult.EnsureSuccess();

        listCollectionsResult.Result.Collections.Length.Should().Be(0);

        // recover collection from local snapshot

        var recoverCollectionResult = await _qdrantHttpClient.RecoverShardFromSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            createSnapshotResult.Result.Name,
            CancellationToken.None);

        recoverCollectionResult.Status.IsSuccess.Should().BeTrue();
        recoverCollectionResult.Result.Should().BeTrue();

        // check collection recovered

        listCollectionsResult = await _qdrantHttpClient.ListCollections(CancellationToken.None);

        listCollectionsResult.EnsureSuccess();

        listCollectionsResult.Result.Collections.Length.Should().Be(1);
    }

    [Test]
    public async Task RecoverFromUploadedSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        createSnapshotResult.EnsureSuccess();

        // this method call is here since when collection is deleted it's impossible to download its snapshot for some reason
        var downloadedSnapshot = await _qdrantHttpClient.DownloadShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            createSnapshotResult.Result.Name,
            CancellationToken.None);

        downloadedSnapshot.EnsureSuccess();

        // delete collection

        var deleteCollectionResponse = await _qdrantHttpClient.DeleteCollection(
            TestCollectionName,
            CancellationToken.None);

        deleteCollectionResponse.EnsureSuccess();

        var listCollectionsResult = await _qdrantHttpClient.ListCollections(CancellationToken.None);

        listCollectionsResult.EnsureSuccess();

        listCollectionsResult.Result.Collections.Length.Should().Be(0);

        // recover collection from downloaded snapshot

        // this recovery may take a lot of time!
        var recoverCollectionResult = await _qdrantHttpClient.RecoverShardFromUploadedSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            downloadedSnapshot.Result.SnapshotDataStream,
            CancellationToken.None);

        recoverCollectionResult.Status.IsSuccess.Should().Be(true);
        recoverCollectionResult.Result.Should().BeTrue();

        // check collection recovered

        listCollectionsResult = await _qdrantHttpClient.ListCollections(CancellationToken.None);

        listCollectionsResult.EnsureSuccess();

        listCollectionsResult.Result.Collections.Length.Should().Be(1);
    }
}
