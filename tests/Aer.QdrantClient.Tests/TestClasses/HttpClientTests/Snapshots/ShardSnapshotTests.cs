using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots;

public class ShardSnapshotTests : SnapshotTestsBase
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
        
        // shard recovery from file urls is not supported
        var recoverFromSnapshotNonExistentSnapshotLocalUriResultAct =
            async () => await _qdrantHttpClient.RecoverShardFromSnapshot(
                TestCollectionName,
                SINGLE_SHARD_ID,
                new Uri("file:///qdrant/snapshots/test_collection-2022-08-04-10-49-10.snapshot"),
                CancellationToken.None);

        await recoverFromSnapshotNonExistentSnapshotLocalUriResultAct.Should().ThrowAsync<NotSupportedException>();

        // Collection shard can't be recovered from snapshot if collection does not exist

        (await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None)).EnsureSuccess();
        
        var recoverFromSnapshotNonExistentSnapshotUriResult =
            await _qdrantHttpClient.RecoverShardFromSnapshot(
                TestCollectionName,
                SINGLE_SHARD_ID,
                new Uri("https://non-exitent-address-12345.com/test_collection-2022-08-04-10-49-10.snapshot"),
                CancellationToken.None);

        recoverFromSnapshotNonExistentSnapshotUriResult.Status.IsSuccess.Should().BeFalse();
        recoverFromSnapshotNonExistentSnapshotUriResult.Status.Error.Should()
            .ContainAll("Not Found", "Failed to download");

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
        createSnapshotResult.Result.SnapshotType.Should().Be(SnapshotType.Shard);
    }

    [Test]
    public async Task ListSnapshots()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        // create first snapshot

        var createSnapshotResult = (await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None)).EnsureSuccess();

        var listSnapshotsResult = await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();

        listSnapshotsResult.Result.Length.Should().Be(1); // one snapshot created

        listSnapshotsResult.Result.Single().Name.Should().Be(createSnapshotResult.Name);
        listSnapshotsResult.Result.Single().Size.Should().Be(createSnapshotResult.Size);
        listSnapshotsResult.Result.Single().SnapshotType.Should().Be(SnapshotType.Shard);

        // If requesting to create snapshot in less than a second after the previous one
        // Qdrant will just return the first snapshot info again instead of creating a new one
        // so we need to wait a bit to ensure that creation time will be different
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        // create second snapshot

        var createSecondSnapshotResult =
            (await _qdrantHttpClient.CreateShardSnapshot(
                TestCollectionName,
                SINGLE_SHARD_ID,
                CancellationToken.None)).EnsureSuccess();

        listSnapshotsResult = await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Length.Should().Be(2); // two snapshots so far

        var newSnapshotInfo = listSnapshotsResult.Result.Single(n => n.Name != createSnapshotResult.Name);

        newSnapshotInfo.Name.Should().Be(createSecondSnapshotResult.Name);
        newSnapshotInfo.Size.Should().Be(createSecondSnapshotResult.Size);
        newSnapshotInfo.Checksum.Should().Be(createSecondSnapshotResult.Checksum);
        newSnapshotInfo.CreationTime.Should().Be(createSecondSnapshotResult.CreationTime);
        newSnapshotInfo.SnapshotType.Should().Be(SnapshotType.Shard);
    }

    [Test]
    public async Task DeleteSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        // create first snapshot

        var createSnapshotResult = (await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None)).EnsureSuccess();

        var listSnapshotsResult = (await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None)).EnsureSuccess();

        listSnapshotsResult.Length.Should().Be(1);

        var deleteSnapshotResult =
            await _qdrantHttpClient.DeleteShardSnapshot(
                TestCollectionName,
                SINGLE_SHARD_ID,
                createSnapshotResult.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        listSnapshotsResult = (await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None)).EnsureSuccess();

        listSnapshotsResult.Length.Should().Be(0);

        // create two snapshots and delete one

        var createFirstSnapshotResult =
            (await _qdrantHttpClient.CreateShardSnapshot(
                TestCollectionName,
                SINGLE_SHARD_ID,
                CancellationToken.None)).EnsureSuccess();
        
        await Task.Delay(TimeSpan.FromSeconds(1));

        var createSecondSnapshotResult =
            (await _qdrantHttpClient.CreateShardSnapshot(
                TestCollectionName,
                SINGLE_SHARD_ID,
                CancellationToken.None)).EnsureSuccess();

        listSnapshotsResult = (await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None)).EnsureSuccess();

        listSnapshotsResult.Length.Should().Be(2);

        deleteSnapshotResult =
            await _qdrantHttpClient.DeleteShardSnapshot(
                TestCollectionName,
                SINGLE_SHARD_ID,
                createFirstSnapshotResult.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        listSnapshotsResult = (await _qdrantHttpClient.ListShardSnapshots(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None)).EnsureSuccess();

        listSnapshotsResult.Length.Should().Be(1);
        listSnapshotsResult.Single().Name.Should().Be(createSecondSnapshotResult.Name);
    }

    [Test]
    public async Task DownloadSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = (await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None)).EnsureSuccess();

        var downloadSnapshotResponse = await _qdrantHttpClient.DownloadShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            createSnapshotResult.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeTrue();
        downloadSnapshotResponse.Result.SnapshotName.Should().Be(createSnapshotResult.Name);
        downloadSnapshotResponse.Result.SnapshotSizeBytes.Should().Be(createSnapshotResult.Size);

        downloadSnapshotResponse.Result.SnapshotDataStream.Should().NotBeNull();

        downloadSnapshotResponse.Result.SnapshotType.Should().Be(SnapshotType.Shard);

        await AssertSnapshotActualSize(downloadSnapshotResponse.Result.SnapshotDataStream, createSnapshotResult.Size);
    }

    [Test]
    public async Task DownloadSnapshot_AfterCollectionIsDeleted()
    {
        // This test proves that deleting collection does not delete snapshots (which is fair enough)!
        // And if we create collection with the same name again we will be able to download previous snapshot
        
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = (await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        // after explicit collection delete the snapshot download will not be accessible with message saying that the collection does not exist

        var downloadSnapshotResponse = await _qdrantHttpClient.DownloadShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            createSnapshotResult.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeFalse();
        downloadSnapshotResponse.Status.Error.Should().Contain("Collection `test_collection` doesn't exist!");

        // create collection and see if we will be able to download previous snapshot

        (await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None)).EnsureSuccess();

        downloadSnapshotResponse = await _qdrantHttpClient.DownloadShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            createSnapshotResult.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeTrue();

        downloadSnapshotResponse.Result.SnapshotSizeBytes.Should().Be(createSnapshotResult.Size);
        downloadSnapshotResponse.Result.SnapshotName.Should().Be(createSnapshotResult.Name);
        downloadSnapshotResponse.Result.SnapshotDataStream.Should().NotBeNull();

        await AssertSnapshotActualSize(downloadSnapshotResponse.Result.SnapshotDataStream, createSnapshotResult.Size);
    }

    // [Test]
    // public async Task RecoverFromSnapshot()
    // {
    //     await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);
    //
    //     var createSnapshotResult = (await _qdrantHttpClient.CreateShardSnapshot(
    //         TestCollectionName,
    //         SINGLE_SHARD_ID,
    //         CancellationToken.None)).EnsureSuccess();
    //
    //     // delete collection
    //
    //     (await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None)).EnsureSuccess();
    //
    //     var listCollectionsResult = (await _qdrantHttpClient.ListCollections(CancellationToken.None)).EnsureSuccess();
    //     listCollectionsResult.Collections.Length.Should().Be(0);
    //
    //     // recover collection from local snapshot
    //
    //     var recoverCollectionResult = await _qdrantHttpClient.RecoverShardFromSnapshot(
    //         TestCollectionName,
    //         SINGLE_SHARD_ID,
    //         createSnapshotResult.Name,
    //         CancellationToken.None);
    //
    //     recoverCollectionResult.Status.IsSuccess.Should().BeTrue();
    //     recoverCollectionResult.Result.Should().BeTrue();
    //
    //     // check collection recovered
    //
    //     listCollectionsResult = (await _qdrantHttpClient.ListCollections(CancellationToken.None)).EnsureSuccess();
    //
    //     listCollectionsResult.Collections.Length.Should().Be(1);
    //     listCollectionsResult.Collections[0].Name.Should().Be(TestCollectionName);
    //
    //     // Check collection data is recovered
    //
    //     var countPointsResult = (await _qdrantHttpClient.CountPoints(
    //         TestCollectionName,
    //         new CountPointsRequest(),
    //         CancellationToken.None)).EnsureSuccess();
    //
    //     countPointsResult.Count.Should().Be(10);
    // }

    [Test]
    public async Task RecoverFromUploadedSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = (await _qdrantHttpClient.CreateShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            CancellationToken.None)).EnsureSuccess();

        // this method call is here since when collection is deleted it's impossible to download its snapshot for some reason
        var downloadedSnapshotResult = (await _qdrantHttpClient.DownloadShardSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            createSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();

        // Copy snapshot to memory

        MemoryStream downloadedSnapshotStream = new MemoryStream();
        await downloadedSnapshotResult.SnapshotDataStream.CopyToAsync(downloadedSnapshotStream);
        downloadedSnapshotStream.Position = 0;

        // delete collection

        (await _qdrantHttpClient.DeleteCollection(
            TestCollectionName,
            CancellationToken.None)).EnsureSuccess();

        var listCollectionsResult = (await _qdrantHttpClient.ListCollections(CancellationToken.None)).EnsureSuccess();
        listCollectionsResult.Collections.Length.Should().Be(0);

        // Collection shard can't be recovered from snapshot if collection does not exist

        (await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None)).EnsureSuccess();
        
        // recover collection from downloaded snapshot

        var recoverCollectionResult = await _qdrantHttpClient.RecoverShardFromUploadedSnapshot(
            TestCollectionName,
            SINGLE_SHARD_ID,
            downloadedSnapshotStream,
            CancellationToken.None);

        recoverCollectionResult.Status.IsSuccess.Should().BeTrue();
        
        // Shard snapshot recovery successful operation returns null Result
        recoverCollectionResult.Result.Should().BeNull();

        // check collection recovered

        listCollectionsResult = (await _qdrantHttpClient.ListCollections(CancellationToken.None)).EnsureSuccess();

        listCollectionsResult.Collections.Length.Should().Be(1);
        listCollectionsResult.Collections[0].Name.Should().Be(TestCollectionName);

        // Check collection data is recovered

        var countPointsResult = (await _qdrantHttpClient.CountPoints(
            TestCollectionName,
            new CountPointsRequest(),
            CancellationToken.None)).EnsureSuccess();

        countPointsResult.Count.Should().Be(10);
    }
}
