using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots;

//[Ignore("WiP on Snapshot API support")]
public class StorageSnapshotTests : SnapshotTestsBase
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
        var listSnapshotsResult = await _qdrantHttpClient.ListStorageSnapshots(
            CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Length.Should().Be(0);
        
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
        downloadSnapshotResult.Result.Should().BeNull();
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
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName2);

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
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        // create first snapshot

        var createFirstSnapshotResult = (await _qdrantHttpClient.CreateStorageSnapshot(CancellationToken.None)).EnsureSuccess();

        var listSnapshotsResult = await _qdrantHttpClient.ListStorageSnapshots(CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Length.Should().Be(1); // one snapshot created

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
        listSnapshotsResult.Result.Length.Should().Be(2); // two snapshots so far

        var newSnapshot = listSnapshotsResult.Result.Single(n => n.Name != createFirstSnapshotResult.Name);

        newSnapshot.Name.Should().Be(createSecondSnapshotResult.Name);
        newSnapshot.Size.Should().Be(createSecondSnapshotResult.Size);
        newSnapshot.SnapshotType.Should().Be(SnapshotType.Storage);
        newSnapshot.Checksum.Should().Be(createSecondSnapshotResult.Checksum);
        newSnapshot.CreationTime.Should().Be(createSecondSnapshotResult.CreationTime);
    }

    //[Test]
    public async Task DeleteSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        // create first snapshot

        var createSnapshotResult = await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);
        var listSnapshotsResult = await _qdrantHttpClient.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);

        createSnapshotResult.EnsureSuccess();
        listSnapshotsResult.EnsureSuccess();

        listSnapshotsResult.Result.Length.Should().Be(1);

        var deleteSnapshotResult =
            await _qdrantHttpClient.DeleteCollectionSnapshot(
                TestCollectionName,
                createSnapshotResult.Result.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        listSnapshotsResult = await _qdrantHttpClient.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);

        listSnapshotsResult.EnsureSuccess();

        listSnapshotsResult.Result.Length.Should().Be(0);

        // create two snapshots and delete one

        var createFirstSnapshotResult =
            await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);
        var createSecondSnapshotResult =
            await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        createFirstSnapshotResult.EnsureSuccess();
        createSecondSnapshotResult.EnsureSuccess();

        listSnapshotsResult = await _qdrantHttpClient.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);

        listSnapshotsResult.EnsureSuccess();

        listSnapshotsResult.Result.Length.Should().Be(2);

        deleteSnapshotResult =
            await _qdrantHttpClient.DeleteCollectionSnapshot(
                TestCollectionName,
                createFirstSnapshotResult.Result.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        listSnapshotsResult = await _qdrantHttpClient.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);

        listSnapshotsResult.EnsureSuccess();

        listSnapshotsResult.Result.Length.Should().Be(1);

        listSnapshotsResult.Result.Single().Name.Should().Be(createSecondSnapshotResult.Result.Name);
    }

    //[Test]
    public async Task DownloadSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        createSnapshotResult.EnsureSuccess();

        var downloadSnapshotResponse = await _qdrantHttpClient.DownloadCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Result.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeTrue();
        downloadSnapshotResponse.Result.SnapshotSizeBytes.Should().Be(createSnapshotResult.Result.Size);
    }

    //[Test]
    public async Task DownloadSnapshot_AfterCollectionIsDeleted()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        createSnapshotResult.EnsureSuccess();

        var deleteCollectionResult = await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);
        deleteCollectionResult.EnsureSuccess();

        // after explicit collection delete the snapshot download will not be accessible with message saying that the collection does not exist

        var downloadSnapshotResponse = await _qdrantHttpClient.DownloadCollectionSnapshot(
            TestCollectionName,
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

        downloadSnapshotResponse = await _qdrantHttpClient.DownloadCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Result.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeTrue();
        downloadSnapshotResponse.Result.SnapshotSizeBytes.Should().Be(createSnapshotResult.Result.Size);
    }

    //[Test]
    public async Task RecoverFromSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        createSnapshotResult.EnsureSuccess();

        // delete collection

        var deleteCollectionResponse = await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);

        deleteCollectionResponse.EnsureSuccess();

        var listCollectionsResult = await _qdrantHttpClient.ListCollections(CancellationToken.None);

        listCollectionsResult.EnsureSuccess();

        listCollectionsResult.Result.Collections.Length.Should().Be(0);

        // recover collection from local snapshot

        var recoverCollectionResult = await _qdrantHttpClient.RecoverCollectionFromSnapshot(
            TestCollectionName,
            createSnapshotResult.Result.Name,
            CancellationToken.None);

        recoverCollectionResult.Status.IsSuccess.Should().BeTrue();
        recoverCollectionResult.Result.Should().BeTrue();

        // check collection recovered

        listCollectionsResult = await _qdrantHttpClient.ListCollections(CancellationToken.None);

        listCollectionsResult.EnsureSuccess();

        listCollectionsResult.Result.Collections.Length.Should().Be(1);
    }

    //[Test]
    public async Task RecoverFromUploadedSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        createSnapshotResult.EnsureSuccess();

        // this method call is here since when collection is deleted it's impossible to download its snapshot for some reason
        var downloadedSnapshot = await _qdrantHttpClient.DownloadCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Result.Name,
            CancellationToken.None);

        downloadedSnapshot.EnsureSuccess();

        // delete collection

        var deleteCollectionResponse = await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);

        deleteCollectionResponse.EnsureSuccess();

        var listCollectionsResult = await _qdrantHttpClient.ListCollections(CancellationToken.None);

        listCollectionsResult.EnsureSuccess();

        listCollectionsResult.Result.Collections.Length.Should().Be(0);

        // recover collection from downloaded snapshot

        // this recovery may take a lot of time!
        var recoverCollectionResult = await _qdrantHttpClient.RecoverCollectionFromUploadedSnapshot(
            TestCollectionName,
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
