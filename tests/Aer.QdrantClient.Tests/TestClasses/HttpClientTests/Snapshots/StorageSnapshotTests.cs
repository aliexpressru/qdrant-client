using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Configuration;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots;

public class StorageSnapshotTests : SnapshotTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;
    private QdrantClientSettings _clientSettings;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();
        
        _clientSettings = GetQdrantClientSettings();
        _qdrantHttpClient = ServiceProvider.GetRequiredService<QdrantHttpClient>();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage(_qdrantHttpClient);
    }

    [Test]
    public async Task NonExistentCollectionSnapshotsOperations()
    {
        // list
        var listSnapshotsResult = await _qdrantHttpClient.ListStorageSnapshots(
            CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Count.Should().Be(0);

        // delete

        var deleteSnapshotResult = await _qdrantHttpClient.DeleteStorageSnapshot(
            "non_existent_snapshot_name",
            CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeFalse();
        deleteSnapshotResult.Status.Error.Should().ContainAll("Not found", "non_existent_snapshot_name");

        // download

        var downloadSnapshotResult = await _qdrantHttpClient.DownloadStorageSnapshot(
            "non_existent_snapshot_name",
            CancellationToken.None);

        downloadSnapshotResult.Status.IsSuccess.Should().BeFalse();
        downloadSnapshotResult.Status.Error.Should().ContainAll("Not found", "non_existent_snapshot_name");

        downloadSnapshotResult.Result.Should().NotBeNull();
        downloadSnapshotResult.Result.SnapshotDataStream.Should().BeNull();
        downloadSnapshotResult.Result.SnapshotName.Should().Be("non_existent_snapshot_name");
        downloadSnapshotResult.Result.SnapshotSizeBytes.Should().Be(-1);
        downloadSnapshotResult.Result.SnapshotSizeMegabytes.Should().Be(-1);
        downloadSnapshotResult.Result.SnapshotType.Should().Be(SnapshotType.Storage);
    }

    [Test]
    public async Task CreateSnapshot_NoCollections()
    {
        var createSnapshotResult = await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None);

        createSnapshotResult.Status.IsSuccess.Should().BeTrue();
        createSnapshotResult.Result.Name.Should().NotBeNullOrEmpty();
        createSnapshotResult.Result.Size.Should().BeGreaterThan(0);
        createSnapshotResult.Result.SizeMegabytes.Should().BeGreaterThan(0);
        createSnapshotResult.Result.SnapshotType.Should().Be(SnapshotType.Storage);
    }

    [Test]
    public async Task CreateSnapshot()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);
        await PrepareCollection(_qdrantHttpClient, TestCollectionName2);

        var createSnapshotResult = await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None);

        createSnapshotResult.Status.IsSuccess.Should().BeTrue();
        createSnapshotResult.Result.Name.Should().NotBeNullOrEmpty();
        createSnapshotResult.Result.Size.Should().BeGreaterThan(0);
        createSnapshotResult.Result.SizeMegabytes.Should().BeGreaterThan(0);
        createSnapshotResult.Result.SnapshotType.Should().Be(SnapshotType.Storage);
    }

    [Test]
    public async Task ListSnapshots()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);

        // create first snapshot

        var createFirstSnapshotResult =
            (await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None)).EnsureSuccess();

        var listSnapshotsResult = await _qdrantHttpClient.ListStorageSnapshots(CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Count.Should().Be(1); // one snapshot created

        var createdSnapshot = listSnapshotsResult.Result.Single();

        createdSnapshot.Name.Should().Be(createFirstSnapshotResult.Name);
        createdSnapshot.Size.Should().Be(createFirstSnapshotResult.Size);
        createdSnapshot.SnapshotType.Should().Be(SnapshotType.Storage);
        createdSnapshot.Checksum.Should().Be(createFirstSnapshotResult.Checksum);
        createdSnapshot.CreationTime.Should().Be(createFirstSnapshotResult.CreationTime);

        // If we ask qdrant to create two snapshots one by one with delay less than a second
        // API will just return previously created snapshot as a result for the second call

        await Task.Delay(TimeSpan.FromSeconds(1));

        // create second snapshot

        var createSecondSnapshotResult =
            (await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None)).EnsureSuccess();

        listSnapshotsResult = await _qdrantHttpClient.ListStorageSnapshots(CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Count.Should().Be(2); // two snapshots so far

        var newSnapshot = listSnapshotsResult.Result.Single(n => n.Name != createFirstSnapshotResult.Name);

        newSnapshot.Name.Should().Be(createSecondSnapshotResult.Name);
        newSnapshot.Size.Should().Be(createSecondSnapshotResult.Size);
        newSnapshot.SnapshotType.Should().Be(SnapshotType.Storage);
        newSnapshot.Checksum.Should().Be(createSecondSnapshotResult.Checksum);
        newSnapshot.CreationTime.Should().Be(createSecondSnapshotResult.CreationTime);
    }

    [Test]
    public async Task DeleteSnapshot()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);

        // create first snapshot

        var createSnapshotResult =
            (await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None)).EnsureSuccess();
        var listSnapshotsResult =
            (await _qdrantHttpClient.ListStorageSnapshots(CancellationToken.None)).EnsureSuccess();

        listSnapshotsResult.Count.Should().Be(1);

        var deleteSnapshotResult =
            await _qdrantHttpClient.DeleteStorageSnapshot(
                createSnapshotResult.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        listSnapshotsResult = (await _qdrantHttpClient.ListStorageSnapshots(CancellationToken.None)).EnsureSuccess();

        listSnapshotsResult.Count.Should().Be(0);

        // create two snapshots and delete one

        var createFirstSnapshotResult =
            (await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None)).EnsureSuccess();

        // Delay to ensure that qdrant won't just return previous snapshot
        await Task.Delay(TimeSpan.FromSeconds(1));

        var createSecondSnapshotResult =
            (await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None)).EnsureSuccess();

        listSnapshotsResult =
            (await _qdrantHttpClient.ListStorageSnapshots(CancellationToken.None))
            .EnsureSuccess();

        listSnapshotsResult.Count.Should().Be(2);

        deleteSnapshotResult = await _qdrantHttpClient.DeleteStorageSnapshot(
            createFirstSnapshotResult.Name,
            CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        listSnapshotsResult = (await _qdrantHttpClient.ListStorageSnapshots(CancellationToken.None)).EnsureSuccess();

        listSnapshotsResult.Count.Should().Be(1);

        listSnapshotsResult.Single().Name.Should().Be(createSecondSnapshotResult.Name);
    }

    [Test]
    public async Task DownloadSnapshot()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult =
            (await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None)).EnsureSuccess();

        var downloadSnapshotResponse = await _qdrantHttpClient.DownloadStorageSnapshot(
            createSnapshotResult.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeTrue();
        downloadSnapshotResponse.Result.SnapshotName.Should().Be(createSnapshotResult.Name);

        downloadSnapshotResponse.Result.SnapshotSizeBytes.Should().Be(
            !_clientSettings.EnableCompression
                ? createSnapshotResult.Size
                : 0
        );

        downloadSnapshotResponse.Result.SnapshotDataStream.Should().NotBeNull();

        downloadSnapshotResponse.Result.SnapshotType.Should().Be(SnapshotType.Storage);

        await AssertSnapshotActualSize(downloadSnapshotResponse.Result.SnapshotDataStream, createSnapshotResult.Size);
    }

    [Test]
    public async Task DownloadSnapshot_AfterAllCollectionsDeleted()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult =
            (await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None)).EnsureSuccess();

        // Delete collection

        (await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        // After all collections are deleted the storage snapshot download should still work

        var downloadSnapshotResponse = await _qdrantHttpClient.DownloadStorageSnapshot(
            createSnapshotResult.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeTrue();
        downloadSnapshotResponse.Result.SnapshotName.Should().Be(createSnapshotResult.Name);

        downloadSnapshotResponse.Result.SnapshotSizeBytes.Should().Be(
            !_clientSettings.EnableCompression
                ? createSnapshotResult.Size
                : 0
        );

        downloadSnapshotResponse.Result.SnapshotDataStream.Should().NotBeNull();

        downloadSnapshotResponse.Result.SnapshotType.Should().Be(SnapshotType.Storage);

        await AssertSnapshotActualSize(downloadSnapshotResponse.Result.SnapshotDataStream, createSnapshotResult.Size);
    }

    [Test]
    [Ignore("Method RecoverStorageFromSnapshot is not implemented in Qdrant client yet")]
    public async Task RecoverFromSnapshot()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult =
            (await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None)).EnsureSuccess();

        // delete collection

        (await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        var listCollectionsResult = (await _qdrantHttpClient.ListCollections(CancellationToken.None)).EnsureSuccess();

        listCollectionsResult.Collections.Length.Should().Be(0);

        // recover collection from local snapshot

        var recoverCollectionResult = await _qdrantHttpClient.RecoverStorageFromSnapshot(
            createSnapshotResult.Name,
            CancellationToken.None);

        recoverCollectionResult.Status.IsSuccess.Should().BeTrue();
        recoverCollectionResult.Result.Should().BeTrue();

        // check collection recovered

        listCollectionsResult = (await _qdrantHttpClient.ListCollections(CancellationToken.None)).EnsureSuccess();

        listCollectionsResult.Collections.Length.Should().Be(1);
    }

    [Test]
    [Ignore("Method RecoverStorageFromSnapshot is not implemented in Qdrant client yet")]
    public async Task RecoverFromUploadedSnapshot()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);
        await PrepareCollection(_qdrantHttpClient, TestCollectionName2);

        // Create and download snapshot

        var createSnapshotResult =
            (await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None)).EnsureSuccess();

        // this method call is here since when collection is deleted it's impossible to download its snapshot
        var downloadedSnapshotResult = (await _qdrantHttpClient.DownloadStorageSnapshot(
            createSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();

        // Copy snapshot to memory

        MemoryStream downloadedSnapshotStream = new MemoryStream();
        await downloadedSnapshotResult.SnapshotDataStream.CopyToAsync(downloadedSnapshotStream);
        downloadedSnapshotStream.Position = 0;

        // delete collections

        (await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None)).EnsureSuccess();
        (await _qdrantHttpClient.DeleteCollection(TestCollectionName2, CancellationToken.None)).EnsureSuccess();

        var listCollectionsResult = (await _qdrantHttpClient.ListCollections(CancellationToken.None)).EnsureSuccess();
        listCollectionsResult.Collections.Length.Should().Be(0);

        // recover storage from downloaded snapshot

        var recoverStorageResult = await _qdrantHttpClient.RecoverStorageFromUploadedSnapshot(
            downloadedSnapshotStream,
            CancellationToken.None,
            isWaitForResult: true,
            SnapshotPriority.Snapshot,
            snapshotChecksum: createSnapshotResult.Checksum);

        recoverStorageResult.Status.IsSuccess.Should().Be(true);
        recoverStorageResult.Result.Should().BeTrue();

        // check collection recovered

        listCollectionsResult = (await _qdrantHttpClient.ListCollections(CancellationToken.None)).EnsureSuccess();

        listCollectionsResult.Collections.Length.Should().Be(2);

        var collectionNames = listCollectionsResult.Collections.Select(c => c.Name).ToArray();

        collectionNames.Should().Contain([TestCollectionName, TestCollectionName2]);
    }
}
