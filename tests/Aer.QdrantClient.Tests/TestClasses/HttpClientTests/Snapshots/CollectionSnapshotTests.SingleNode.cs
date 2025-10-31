using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Configuration;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots;

public class CollectionSnapshotTestsSingleNode : SnapshotTestsBase
{
    private QdrantHttpClient _qdrantHttpClientSingleNode;
    private QdrantClientSettings _clientSettings;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();

        _clientSettings = GetQdrantClientSettings();
        _qdrantHttpClientSingleNode = ServiceProvider.GetRequiredService<QdrantHttpClient>();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage(_qdrantHttpClientSingleNode);
    }

    [Test]
    public async Task NonExistentCollectionSnapshotsOperations()
    {
        // list
        var listSnapshotsResult = await _qdrantHttpClientSingleNode.ListCollectionSnapshots(
            TestCollectionName,
            CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeFalse();
        listSnapshotsResult.Status.Error.Should().ContainAll("Not found", TestCollectionName);

        // create
        var createSnapshotResult = await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(
            TestCollectionName,
            CancellationToken.None);

        createSnapshotResult.Status.IsSuccess.Should().BeFalse();
        createSnapshotResult.Status.Error.Should().ContainAll("Not found", TestCollectionName);

        // delete

        var deleteSnapshotResult = await _qdrantHttpClientSingleNode.DeleteCollectionSnapshot(
            TestCollectionName,
            "non_existent_snapshot_name",
            CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeFalse();
        deleteSnapshotResult.Status.Error.Should().ContainAll("Not found", TestCollectionName);

        // recover

        var recoverFromSnapshotNonExistentSnapshotNameResult = await _qdrantHttpClientSingleNode.RecoverCollectionFromSnapshot(
            TestCollectionName,
            "non_existent_snapshot_uri",
            CancellationToken.None);

        recoverFromSnapshotNonExistentSnapshotNameResult.Status.IsSuccess.Should().BeFalse();

        var recoverFromSnapshotNonExistentSnapshotLocalUriResult =
            await _qdrantHttpClientSingleNode.RecoverCollectionFromSnapshot(
                TestCollectionName,
                new Uri("file:///qdrant/snapshots/test_collection-2022-08-04-10-49-10.snapshot"),
                CancellationToken.None);

        recoverFromSnapshotNonExistentSnapshotLocalUriResult.Status.IsSuccess.Should().BeFalse();
        recoverFromSnapshotNonExistentSnapshotLocalUriResult.Status.Error.Should().ContainAll("does not exist", "file");

        var recoverFromSnapshotNonExistentSnapshotUriResultAct = async () =>
            await _qdrantHttpClientSingleNode.RecoverCollectionFromSnapshot(
                TestCollectionName,
                new Uri("https://non-exitent-address-12345.com/test_collection-2022-08-04-10-49-10.snapshot"),
                CancellationToken.None);

        await recoverFromSnapshotNonExistentSnapshotUriResultAct.Should().ThrowAsync<QdrantCommunicationException>()
            .Where(e => e.Message.Contains(
                "error sending request for url (https://non-exitent-address-12345.com/test_collection-2022-08-04-10-49-10.snapshot)"));

        // download

        var downloadSnapshotResult = await _qdrantHttpClientSingleNode.DownloadCollectionSnapshot(
            TestCollectionName,
            "non_existent_snapshot_name",
            CancellationToken.None);

        downloadSnapshotResult.Status.IsSuccess.Should().BeFalse();
        downloadSnapshotResult.Status.Error.Should().ContainAll("Not found", TestCollectionName);

        downloadSnapshotResult.Result.Should().NotBeNull();
        downloadSnapshotResult.Result.SnapshotDataStream.Should().BeNull();
        downloadSnapshotResult.Result.SnapshotName.Should().Be("non_existent_snapshot_name");
        downloadSnapshotResult.Result.SnapshotSizeBytes.Should().Be(-1);
        downloadSnapshotResult.Result.SnapshotSizeMegabytes.Should().Be(-1);
        downloadSnapshotResult.Result.SnapshotType.Should().Be(SnapshotType.Collection);
    }

    [Test]
    public async Task ListSnapshots_ExistingCollectionNoSnapshotsYet()
    {
        await PrepareCollection(_qdrantHttpClientSingleNode, TestCollectionName);

        var listSnapshotsResult = await _qdrantHttpClientSingleNode.ListCollectionSnapshots(
            TestCollectionName,
            CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Count.Should().Be(0);
    }

    [Test]
    public async Task CreateSnapshot()
    {
        await PrepareCollection(_qdrantHttpClientSingleNode, TestCollectionName);

        var createSnapshotResult =
            await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        createSnapshotResult.Status.IsSuccess.Should().BeTrue();
        createSnapshotResult.Result.Name.Should().Contain(TestCollectionName);
        createSnapshotResult.Result.Size.Should().BeGreaterThan(0);
        createSnapshotResult.Result.SizeMegabytes.Should().BeGreaterThan(0);
        createSnapshotResult.Result.Checksum.Should().NotBeNullOrEmpty();
        createSnapshotResult.Result.SnapshotType.Should().Be(SnapshotType.Collection);
        
        // Clean up
        
        await _qdrantHttpClientSingleNode.DeleteCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Result.Name,
            CancellationToken.None);
    }

    [Test]
    public async Task ListSnapshots()
    {
        await PrepareCollection(_qdrantHttpClientSingleNode, TestCollectionName);

        // create first snapshot

        var createFirstSnapshotResult = (await _qdrantHttpClientSingleNode
            .CreateCollectionSnapshot(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        var listSnapshotsResult =
            await _qdrantHttpClientSingleNode.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();

        listSnapshotsResult.Result.Count.Should().Be(1); // one snapshot created

        var snapshotInfo = listSnapshotsResult.Result.Single();

        snapshotInfo.Name.Should().Be(createFirstSnapshotResult.Name);
        snapshotInfo.Size.Should().Be(createFirstSnapshotResult.Size);
        snapshotInfo.SizeMegabytes.Should().Be(createFirstSnapshotResult.SizeMegabytes);
        
        snapshotInfo.CreationTime.Should().Be(createFirstSnapshotResult.CreationTime);
        snapshotInfo.Checksum.Should().Be(createFirstSnapshotResult.Checksum);
        snapshotInfo.SnapshotType.Should().Be(SnapshotType.Collection);

        // create second snapshot

        // When creating two snapshots one by one in quick succession, Qdrant 1.14 seems to return last created snapshot while 1.15 returns first.
        // At least this is what happens in CI tests, on local machine both versions return first snapshot.

        // If requesting to create snapshot in less than a second after the previous one
        // Qdrant will just return the first snapshot info again instead of creating a new one
        // so we need to wait a bit to ensure that creation time will be different
        await Task.Delay(TimeSpan.FromSeconds(1));

        var createSecondSnapshotResult =
            (await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        listSnapshotsResult =
            await _qdrantHttpClientSingleNode.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Count.Should().Be(2); // two snapshots so far

        var newSnapshotInfo = listSnapshotsResult.Result.Single(n => n.Name != createFirstSnapshotResult.Name);

        newSnapshotInfo.Name.Should().Be(createSecondSnapshotResult.Name);
        newSnapshotInfo.Size.Should().Be(createSecondSnapshotResult.Size);
        newSnapshotInfo.Checksum.Should().Be(createSecondSnapshotResult.Checksum);
        newSnapshotInfo.CreationTime.Should().Be(createSecondSnapshotResult.CreationTime);
        newSnapshotInfo.SnapshotType.Should().Be(SnapshotType.Collection);

        // Clean up

        await _qdrantHttpClientSingleNode.DeleteCollectionSnapshot(
            TestCollectionName,
            createFirstSnapshotResult.Name,
            CancellationToken.None);

        await _qdrantHttpClientSingleNode.DeleteCollectionSnapshot(
            TestCollectionName,
            createSecondSnapshotResult.Name,
            CancellationToken.None);
    }

    [Test]
    public async Task DeleteSnapshot()
    {
        await PrepareCollection(_qdrantHttpClientSingleNode, TestCollectionName);

        // create first snapshot and delete it

        var createSnapshotResult =
            (await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();
        var listSnapshotsResult =
            (await _qdrantHttpClientSingleNode.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        listSnapshotsResult.Count.Should().Be(1);

        var deleteSnapshotResult =
            await _qdrantHttpClientSingleNode.DeleteCollectionSnapshot(
                TestCollectionName,
                createSnapshotResult.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        var listSnapshotsAfterDeletionResult =
            (await _qdrantHttpClientSingleNode.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        listSnapshotsAfterDeletionResult.Count.Should().Be(0);

        // create two snapshots and delete one

        var createFirstSnapshotResult =
            (await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        // If requesting to create snapshot in less than a second after the previous one
        // Qdrant will just return the first snapshot info again instead of creating a new one
        // so we need to wait a bit to ensure that creation time will be different
        await Task.Delay(TimeSpan.FromSeconds(1));

        var createSecondSnapshotResult =
            (await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var listTwoSnapshotsResults =
            (await _qdrantHttpClientSingleNode.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        listTwoSnapshotsResults.Count.Should().Be(2);

        deleteSnapshotResult =
            await _qdrantHttpClientSingleNode.DeleteCollectionSnapshot(
                TestCollectionName,
                createFirstSnapshotResult.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        var listLastRemainingSnapshotResult =
            (await _qdrantHttpClientSingleNode.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();
        listLastRemainingSnapshotResult.Count.Should().Be(1);

        listLastRemainingSnapshotResult.Single().Name.Should().Be(createSecondSnapshotResult.Name);
        
        // Clean up
        
        await _qdrantHttpClientSingleNode.DeleteCollectionSnapshot(
            TestCollectionName,
            createSecondSnapshotResult.Name,
            CancellationToken.None);
    }

    [Test]
    public async Task DownloadSnapshot()
    {
        await PrepareCollection(_qdrantHttpClientSingleNode, TestCollectionName);

        var createSnapshotResult =
            (await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var downloadSnapshotResponse = await _qdrantHttpClientSingleNode.DownloadCollectionSnapshot(
            TestCollectionName,
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

        downloadSnapshotResponse.Result.SnapshotType.Should().Be(SnapshotType.Collection);

        await AssertSnapshotActualSize(downloadSnapshotResponse.Result.SnapshotDataStream, createSnapshotResult.Size);
        
        // Clean up
        
        await _qdrantHttpClientSingleNode.DeleteCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None);
    }

    [Test]
    public async Task DownloadSnapshot_AfterCollectionIsDeleted()
    {
        // This test proves that deleting collection does not delete snapshots (which is fair enough)!
        // And if we create collection with the same name again we will be able to download previous snapshot

        await PrepareCollection(_qdrantHttpClientSingleNode, TestCollectionName);

        var createSnapshotResult =
            (await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();
        (await _qdrantHttpClientSingleNode.DeleteCollection(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        // after explicit collection delete the snapshot download will not be accessible with message saying that the collection does not exist

        var downloadSnapshotResponse = await _qdrantHttpClientSingleNode.DownloadCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeFalse();
        downloadSnapshotResponse.Status.Error.Should().Contain("Collection `test_collection` doesn't exist!");

        // create collection and see if we will be able to download previous snapshot

        (await _qdrantHttpClientSingleNode.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None)).EnsureSuccess();

        downloadSnapshotResponse = await _qdrantHttpClientSingleNode.DownloadCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeTrue();

        downloadSnapshotResponse.Result.SnapshotSizeBytes.Should().Be(
            !_clientSettings.EnableCompression
                ? createSnapshotResult.Size
                : 0
        );
        
        downloadSnapshotResponse.Result.SnapshotName.Should().Be(createSnapshotResult.Name);
        downloadSnapshotResponse.Result.SnapshotDataStream.Should().NotBeNull();

        await AssertSnapshotActualSize(downloadSnapshotResponse.Result.SnapshotDataStream, createSnapshotResult.Size);
        
        // Clean up
        
        await _qdrantHttpClientSingleNode.DeleteCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None);
    }

    [Test]
    public async Task RecoverFromDeletedSnapshot()
    {
        await PrepareCollection(_qdrantHttpClientSingleNode, TestCollectionName);

        var createSnapshotResult =
            (await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        // delete snapshot

        (await _qdrantHttpClientSingleNode.DeleteCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();

        // Recover collection from deleted snapshot

        var recoverCollectionResult = await _qdrantHttpClientSingleNode.RecoverCollectionFromSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None,
            isWaitForResult: true,
            SnapshotPriority.Snapshot,
            snapshotChecksum: createSnapshotResult.Checksum);

        recoverCollectionResult.Status.IsSuccess.Should().BeFalse();
        recoverCollectionResult.Status.Error.Should().ContainAll(
            "Snapshot file",
            createSnapshotResult.Name,
            "does not exist");
    }

    [Test]
    public async Task RecoverFromSnapshot()
    {
        var expectedVectorCount = 50;
        
        await PrepareCollection(
                _qdrantHttpClientSingleNode,
                TestCollectionName,
                vectorCount: expectedVectorCount);

        var createSnapshotResult =
            (await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        // delete collection

        (await _qdrantHttpClientSingleNode.DeleteCollection(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        var listCollectionsResult = (await _qdrantHttpClientSingleNode.ListCollections(CancellationToken.None)).EnsureSuccess();
        listCollectionsResult.Collections.Length.Should().Be(0);

        // recover collection from local snapshot

        var recoverCollectionResult = await _qdrantHttpClientSingleNode.RecoverCollectionFromSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None,
            isWaitForResult: true,
            SnapshotPriority.Snapshot,
            snapshotChecksum: createSnapshotResult.Checksum);

        recoverCollectionResult.Status.IsSuccess.Should().BeTrue();
        recoverCollectionResult.Result.Should().BeTrue();
        recoverCollectionResult.Time.Should().BeGreaterThan(0);
        
        // check collection recovered

        listCollectionsResult = (await _qdrantHttpClientSingleNode.ListCollections(CancellationToken.None)).EnsureSuccess();

        listCollectionsResult.Collections.Length.Should().Be(1);
        listCollectionsResult.Collections[0].Name.Should().Be(TestCollectionName);

        // Check collection data is recovered

        var countPointsResult = (await _qdrantHttpClientSingleNode.CountPoints(
            TestCollectionName,
            new CountPointsRequest(),
            CancellationToken.None)).EnsureSuccess();

        countPointsResult.Count.Should().Be((ulong)expectedVectorCount);
        
        // Clean up
        
        await _qdrantHttpClientSingleNode.DeleteCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None);
    }

    [Test]
    public async Task RecoverFromUploadedSnapshot()
    {
        await PrepareCollection(_qdrantHttpClientSingleNode, TestCollectionName);

        // Create and download snapshot

        var createSnapshotResult =
            (await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        // this method call is here since when collection is deleted it's impossible to download its snapshot
        var downloadedSnapshotResult = (await _qdrantHttpClientSingleNode.DownloadCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();

        // Copy snapshot to memory

        MemoryStream downloadedSnapshotStream = new MemoryStream();
        await downloadedSnapshotResult.SnapshotDataStream.CopyToAsync(downloadedSnapshotStream);
        downloadedSnapshotStream.Position = 0;

        // delete collection

        (await _qdrantHttpClientSingleNode.DeleteCollection(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        var listCollectionsResult = (await _qdrantHttpClientSingleNode.ListCollections(CancellationToken.None)).EnsureSuccess();
        listCollectionsResult.Collections.Length.Should().Be(0);

        // recover collection from downloaded snapshot

        var recoverCollectionResult = await _qdrantHttpClientSingleNode.RecoverCollectionFromUploadedSnapshot(
            TestCollectionName,
            downloadedSnapshotStream,
            CancellationToken.None,
            isWaitForResult: true,
            SnapshotPriority.Snapshot,
            snapshotChecksum: createSnapshotResult.Checksum);

        recoverCollectionResult.Status.IsSuccess.Should().BeTrue();
        recoverCollectionResult.Result.Should().BeTrue();

        // check collection recovered

        listCollectionsResult = (await _qdrantHttpClientSingleNode.ListCollections(CancellationToken.None)).EnsureSuccess();

        listCollectionsResult.Collections.Length.Should().Be(1);
        listCollectionsResult.Collections[0].Name.Should().Be(TestCollectionName);

        // Check collection data is recovered

        var countPointsResult = (await _qdrantHttpClientSingleNode.CountPoints(
            TestCollectionName,
            new CountPointsRequest(),
            CancellationToken.None)).EnsureSuccess();

        countPointsResult.Count.Should().Be(10);
        
        // Clean up
        
        await _qdrantHttpClientSingleNode.DeleteCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None);
    }
}
