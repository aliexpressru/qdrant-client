using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

#if !DEBUG
[Ignore("Since snapshot has a minimal size of roughly 100MB these tests are time consuming"
+" and we only run these tests on local machine, not in CI")]
#endif
public class CollectionSnapshotTests : QdrantTestsBase
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

    #region Invalid cases and non-existent colelctions

    [Test]
    public async Task NonExistentCollectionSnapshotsOperations()
    {
        // list
        var listSnapshotsResult = await _qdrantHttpClient.ListCollectionSnapshots(
            TestCollectionName,
            CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeFalse();
        listSnapshotsResult.Status.Error.Should().Contain("Not found");

        // create
        var createSnapshotResult = await _qdrantHttpClient.CreateCollectionSnapshot(
            TestCollectionName,
            CancellationToken.None);

        createSnapshotResult.Status.IsSuccess.Should().BeFalse();
        createSnapshotResult.Status.Error.Should().Contain("Not found");

        // delete

        var deleteSnapshotResult = await _qdrantHttpClient.DeleteCollectionSnapshot(
            TestCollectionName,
            "non_existent_snapshot_name",
            CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeFalse();
        deleteSnapshotResult.Status.Error.Should().Contain("Not found");

        // recover

        var recoverFromSnapshotInvalidUrlResult = await _qdrantHttpClient.RecoverCollectionFromSnapshot(
            TestCollectionName,
            "non_existent_snapshot_uri",
            CancellationToken.None);

        recoverFromSnapshotInvalidUrlResult.Status.IsSuccess.Should().BeFalse();

        var recoverFromSnapshotNonExistentSnapshotResult = await _qdrantHttpClient.RecoverCollectionFromSnapshot(
            TestCollectionName,
            "file:///qdrant/snapshots/test_collection-2022-08-04-10-49-10.snapshot",
            CancellationToken.None);

        recoverFromSnapshotNonExistentSnapshotResult.Status.IsSuccess.Should().BeFalse();
        recoverFromSnapshotNonExistentSnapshotResult.Status.Error.Should().Contain("does not exist");

        // download

        var downloadSnapshotResult = await _qdrantHttpClient.DownloadCollectionSnapshot(
            TestCollectionName,
            "non_existent_snapshot_name",
            CancellationToken.None);

        downloadSnapshotResult.Status.IsSuccess.Should().BeFalse();
        downloadSnapshotResult.Status.Error.Should().Contain("Not found: Collection `test_collection` doesn't exist!");
        downloadSnapshotResult.Result.Should().BeNull();
    }

    [Test]
    public async Task ListSnapshots_ExistingCollectionNoSnapshotsYet()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);

        var listSnapshotsResult = await _qdrantHttpClient.ListCollectionSnapshots(
            TestCollectionName,
            CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Length.Should().Be(0);
    }

    #endregion

    [Test]
    public async Task CreateSnapshot()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        createSnapshotResult.Status.IsSuccess.Should().BeTrue();
        createSnapshotResult.Result.Name.Should().Contain(TestCollectionName);
        createSnapshotResult.Result.Size.Should().BeGreaterThan(0);
        createSnapshotResult.Result.SizeMegabytes.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task ListSnapshots()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);

        // create first snapshot

        var createSnapshotResult = await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        var listSnapshotsResult = await _qdrantHttpClient.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);

        createSnapshotResult.EnsureSuccess();

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();

        listSnapshotsResult.Result.Length.Should().Be(1); // one snapshot created

        listSnapshotsResult.Result.Single().Name.Should().Be(createSnapshotResult.Result.Name);
        listSnapshotsResult.Result.Single().Size.Should().Be(createSnapshotResult.Result.Size);

        // create second snapshot

        var createSecondSnapshotResult =
            await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        createSecondSnapshotResult.EnsureSuccess();

        listSnapshotsResult = await _qdrantHttpClient.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);

        listSnapshotsResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsResult.Result.Length.Should().Be(2); // two snapshots so far

        var newSnapshot = listSnapshotsResult.Result.Single(n => n.Name != createSnapshotResult.Result.Name);

        newSnapshot.Name.Should().Be(createSecondSnapshotResult.Result.Name);
        newSnapshot.Size.Should().Be(createSecondSnapshotResult.Result.Size);
    }

    [Test]
    public async Task DeleteSnapshot()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);

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

    [Test]
    public async Task DownloadSnapshot()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        createSnapshotResult.EnsureSuccess();

        var downloadSnapshotResponse = await _qdrantHttpClient.DownloadCollectionSnapshot(
            TestCollectionName,
            createSnapshotResult.Result.Name,
            CancellationToken.None);

        downloadSnapshotResponse.Status.IsSuccess.Should().BeTrue();
        downloadSnapshotResponse.Result.SnapshotSizeBytes.Should().Be(createSnapshotResult.Result.Size);
    }

    [Test]
    public async Task DownloadSnapshot_AfterCollectionIsDeleted()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);

        var createSnapshotResult = await _qdrantHttpClient.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        createSnapshotResult.EnsureSuccess();

        var deleteColelctionResut = await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);
        deleteColelctionResut.EnsureSuccess();

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

    [Test]
    public async Task RecoverFromSnapshot()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);

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

    [Test]
    public async Task RecoverFromUploadedSnapshot()
    {
        await PrepareCollection(_qdrantHttpClient, TestCollectionName);

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
