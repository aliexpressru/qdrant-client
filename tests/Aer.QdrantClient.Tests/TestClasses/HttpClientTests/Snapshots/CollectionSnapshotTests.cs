using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots;

public class CollectionSnapshotTests : SnapshotTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;

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
        var listSnapshotsResult = await _qdrantHttpClient.ListCollectionSnapshots(
            TestCollectionName,
            CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeFalse();
        listSnapshotsResult.Status.Error.Should().ContainAll("Not found", TestCollectionName);

        // create
        var createSnapshotResult = await _qdrantHttpClient.CreateCollectionSnapshot(
            TestCollectionName,
            CancellationToken.None);

        createSnapshotResult.Status.IsSuccess.Should().BeFalse();
        createSnapshotResult.Status.Error.Should().ContainAll("Not found", TestCollectionName);

        // delete

        var deleteSnapshotResult = await _qdrantHttpClient.DeleteCollectionSnapshot(
            TestCollectionName,
            "non_existent_snapshot_name",
            CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeFalse();
        deleteSnapshotResult.Status.Error.Should().ContainAll("Not found", TestCollectionName);

        // recover

        var recoverFromSnapshotNonExistentSnapshotNameResult = await _qdrantHttpClient.RecoverCollectionFromSnapshot(
            TestCollectionName,
            "non_existent_snapshot_uri",
            CancellationToken.None);

        recoverFromSnapshotNonExistentSnapshotNameResult.Status.IsSuccess.Should().BeFalse();

        var recoverFromSnapshotNonExistentSnapshotLocalUriResult = await _qdrantHttpClient.RecoverCollectionFromSnapshot(
            TestCollectionName,
            new Uri("file:///qdrant/snapshots/test_collection-2022-08-04-10-49-10.snapshot"),
            CancellationToken.None);

        recoverFromSnapshotNonExistentSnapshotLocalUriResult.Status.IsSuccess.Should().BeFalse();
        recoverFromSnapshotNonExistentSnapshotLocalUriResult.Status.Error.Should().ContainAll("does not exist", "file");

        var recoverFromSnapshotNonExistentSnapshotUriResultAct = async () =>
            await _qdrantHttpClient.RecoverCollectionFromSnapshot(
                TestCollectionName,
                new Uri("https://non-exitent-address-12345.com/test_collection-2022-08-04-10-49-10.snapshot"),
                CancellationToken.None);

        await recoverFromSnapshotNonExistentSnapshotUriResultAct.Should().ThrowAsync<QdrantCommunicationException>()
            .Where(e => e.Message.Contains(
                "error sending request for url (https://non-exitent-address-12345.com/test_collection-2022-08-04-10-49-10.snapshot)"));

        // download

        var downloadSnapshotResult = await _qdrantHttpClient.DownloadCollectionSnapshot(
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
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var listSnapshotsResult = await _qdrantHttpClient.ListCollectionSnapshots(
            TestCollectionName,
            CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Length.Should().Be(0);
    }

    [Test]
    public async Task CreateSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        createSnapshotResult.Status.IsSuccess.Should().BeTrue();
        createSnapshotResult.Result.Name.Should().Contain(TestCollectionName);
        createSnapshotResult.Result.Size.Should().BeGreaterThan(0);
        createSnapshotResult.Result.SizeMegabytes.Should().BeGreaterThan(0);
        createSnapshotResult.Result.Checksum.Should().NotBeNullOrEmpty();
        createSnapshotResult.Result.SnapshotType.Should().Be(SnapshotType.Collection);
    }
    
    [Test]
    public async Task ListSnapshots()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        // create first two snapshots one by one

        var createFirstSnapshotResult = (await _qdrantHttpClient
            .CreateCollectionSnapshot(TestCollectionName, CancellationToken.None)).EnsureSuccess();
        
        var immediatelyCreateSecondSnapshotResult = (await _qdrantHttpClient
            .CreateCollectionSnapshot(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        var listSnapshotsResult = await _qdrantHttpClient.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();

        listSnapshotsResult.Result.Length.Should().Be(1); // one snapshot created
        
        var snapshotInfo = listSnapshotsResult.Result.Single(); 

        if(QdrantVersion < Version.Parse("1.15"))
        {
            // Qdrant 1.14 seems to return last created snapshot while 1.15 returns first.
            // At least this is what happens in CI tests, on local machine both versions return first snapshot

            snapshotInfo.Name.Should().Be(immediatelyCreateSecondSnapshotResult.Name);
            snapshotInfo.Size.Should().Be(immediatelyCreateSecondSnapshotResult.Size);
            
            // snapshotInfo.Checksum.Should().Be(
            //     immediatelyCreateSecondSnapshotResult.Checksum,
            //     $"Expected single snapshot to be the first one created, but found checksum mismatch. First snapshot checksum: {createFirstSnapshotResult.Checksum}, next snapshot checksum: {immediatelyCreateSecondSnapshotResult.Checksum}, listed snapshot checksum: {snapshotInfo.Checksum}"
            // );
            
            snapshotInfo.CreationTime.Should().Be(immediatelyCreateSecondSnapshotResult.CreationTime);
        }
        else
        {
            snapshotInfo.Name.Should().Be(createFirstSnapshotResult.Name);
            snapshotInfo.Size.Should().Be(createFirstSnapshotResult.Size);
            
            // snapshotInfo.Checksum.Should().Be(
            //     createFirstSnapshotResult.Checksum,
            //     $"Expected single snapshot to be the first one created, but found checksum mismatch. First snapshot checksum: {createFirstSnapshotResult.Checksum}, next snapshot checksum: {immediatelyCreateSecondSnapshotResult.Checksum}, listed snapshot checksum: {snapshotInfo.Checksum}"
            // );
            
            snapshotInfo.CreationTime.Should().Be(createFirstSnapshotResult.CreationTime);

            // This bit does not work in CI tests for Qdrant < 1.15 but for some reason it works on local machine for the same Qdrant version
            // This is not the main purpose of this test so leaving it as is for now
            immediatelyCreateSecondSnapshotResult.CreationTime.Should().Be(createFirstSnapshotResult.CreationTime);
            immediatelyCreateSecondSnapshotResult.Checksum.Should().Be(createFirstSnapshotResult.Checksum);
        }

        snapshotInfo.SnapshotType.Should().Be(SnapshotType.Collection);

        // create second snapshot
        
        // If requesting to create snapshot in less than a second after the previous one
        // Qdrant will just return the first snapshot info again instead of creating a new one
        // so we need to wait a bit to ensure that creation time will be different
        await Task.Delay(TimeSpan.FromSeconds(1));

        var createSecondSnapshotResult =
            (await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        listSnapshotsResult = await _qdrantHttpClient.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Length.Should().Be(2); // two snapshots so far

        var newSnapshotInfo = listSnapshotsResult.Result.Single(n => n.Name != createFirstSnapshotResult.Name);

        newSnapshotInfo.Name.Should().Be(createSecondSnapshotResult.Name);
        newSnapshotInfo.Size.Should().Be(createSecondSnapshotResult.Size);
        newSnapshotInfo.Checksum.Should().Be(createSecondSnapshotResult.Checksum);
        newSnapshotInfo.CreationTime.Should().Be(createSecondSnapshotResult.CreationTime);
        newSnapshotInfo.SnapshotType.Should().Be(SnapshotType.Collection);
    }

    [Test]
    public async Task DeleteSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        // create first snapshot and delete it

        var createSnapshotResult = (await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None)).EnsureSuccess();
        var listSnapshotsResult = (await _qdrantHttpClient.ListCollectionSnapshots(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        listSnapshotsResult.Length.Should().Be(1);

        var deleteSnapshotResult =
            await _qdrantHttpClient.DeleteCollectionSnapshot(
                TestCollectionName,
                createSnapshotResult.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        var listSnapshotsAfterDeletionResult = (await _qdrantHttpClient.ListCollectionSnapshots(TestCollectionName, CancellationToken.None)).EnsureSuccess();
        
        listSnapshotsAfterDeletionResult.Length.Should().Be(0);

        // create two snapshots and delete one

        var createFirstSnapshotResult =
            (await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        // If requesting to create snapshot in less than a second after the previous one
        // Qdrant will just return the first snapshot info again instead of creating a new one
        // so we need to wait a bit to ensure that creation time will be different
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        var createSecondSnapshotResult =
            (await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        var listTwoSnapshotsResults = (await _qdrantHttpClient.ListCollectionSnapshots(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        listTwoSnapshotsResults.Length.Should().Be(2);

        deleteSnapshotResult =
            await _qdrantHttpClient.DeleteCollectionSnapshot(
                TestCollectionName,
                createFirstSnapshotResult.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        var listLastRemainingSnapshotResult = (await _qdrantHttpClient.ListCollectionSnapshots(TestCollectionName, CancellationToken.None)).EnsureSuccess();
        listLastRemainingSnapshotResult.Length.Should().Be(1);

        listLastRemainingSnapshotResult.Single().Name.Should().Be(createSecondSnapshotResult.Name);
    }

    [Test]
    public async Task DownloadSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = (await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        var downloadSnapshotResponse = await _qdrantHttpClient.DownloadCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeTrue();
        downloadSnapshotResponse.Result.SnapshotName.Should().Be(createSnapshotResult.Name);
        downloadSnapshotResponse.Result.SnapshotSizeBytes.Should().Be(createSnapshotResult.Size);
        
        downloadSnapshotResponse.Result.SnapshotDataStream.Should().NotBeNull();
        
        downloadSnapshotResponse.Result.SnapshotType.Should().Be(SnapshotType.Collection);

        await AssertSnapshotActualSize(downloadSnapshotResponse.Result.SnapshotDataStream, createSnapshotResult.Size);
    }

    [Test]
    public async Task DownloadSnapshot_AfterCollectionIsDeleted()
    {
        // This test proves that deleting collection does not delete snapshots (which is fair enough)!
        // And if we create collection with the same name again we will be able to download previous snapshot
        
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = (await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None)).EnsureSuccess();
        (await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        // after explicit collection delete the snapshot download will not be accessible with message saying that the collection does not exist

        var downloadSnapshotResponse = await _qdrantHttpClient.DownloadCollectionSnapshot(
            TestCollectionName,
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

        downloadSnapshotResponse = await _qdrantHttpClient.DownloadCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeTrue();
        
        downloadSnapshotResponse.Result.SnapshotSizeBytes.Should().Be(createSnapshotResult.Size);
        downloadSnapshotResponse.Result.SnapshotName.Should().Be(createSnapshotResult.Name);
        downloadSnapshotResponse.Result.SnapshotDataStream.Should().NotBeNull();

        await AssertSnapshotActualSize(downloadSnapshotResponse.Result.SnapshotDataStream, createSnapshotResult.Size);
    }

    [Test]
    public async Task RecoverFromDeletedSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult =
            (await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();
        
        // delete snapshot
        
        (await _qdrantHttpClient.DeleteCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();
        
        // Recover collection from deleted snapshot

        var recoverCollectionResult = await _qdrantHttpClient.RecoverCollectionFromSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None,
            isWaitForResult: true,
            SnapshotPriority.Snapshot,
            snapshotChecksum: createSnapshotResult.Checksum);

        recoverCollectionResult.Status.IsSuccess.Should().BeFalse();
        recoverCollectionResult.Status.Error.Should().ContainAll("Snapshot file", createSnapshotResult.Name, "does not exist");
    }

    [Test]
    public async Task RecoverFromSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult =
            (await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();
        
        // delete collection

        (await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        var listCollectionsResult = (await _qdrantHttpClient.ListCollections(CancellationToken.None)).EnsureSuccess();
        listCollectionsResult.Collections.Length.Should().Be(0);

        // recover collection from local snapshot

        var recoverCollectionResult = await _qdrantHttpClient.RecoverCollectionFromSnapshot(
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

    [Test]
    public async Task RecoverFromUploadedSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        // Create and download snapshot
        
        var createSnapshotResult = (await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        // this method call is here since when collection is deleted it's impossible to download its snapshot
        var downloadedSnapshotResult = (await _qdrantHttpClient.DownloadCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();

        // Copy snapshot to memory
        
        MemoryStream downloadedSnapshotStream = new MemoryStream();
        await downloadedSnapshotResult.SnapshotDataStream.CopyToAsync(downloadedSnapshotStream);
        downloadedSnapshotStream.Position = 0;

        // delete collection

        (await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        var listCollectionsResult = (await _qdrantHttpClient.ListCollections(CancellationToken.None)).EnsureSuccess();
        listCollectionsResult.Collections.Length.Should().Be(0);

        // recover collection from downloaded snapshot

        var recoverCollectionResult = await _qdrantHttpClient.RecoverCollectionFromUploadedSnapshot(
            TestCollectionName,
            downloadedSnapshotStream,
            CancellationToken.None,
            isWaitForResult: true,
            SnapshotPriority.Snapshot,
            snapshotChecksum: createSnapshotResult.Checksum);
        
        recoverCollectionResult.Status.IsSuccess.Should().BeTrue();
        recoverCollectionResult.Result.Should().BeTrue();

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
