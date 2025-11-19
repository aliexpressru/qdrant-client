using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Configuration;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots;

#if !DEBUG
[Ignore("Multi-node tests are ignored since we don't have multi-node test environment in CI/CD")]
#endif
public class CollectionSnapshotTestsMultiNode : SnapshotTestsBase
{
    private QdrantHttpClient _qdrantHttpClientClusterNode1;
    private QdrantHttpClient _qdrantHttpClientClusterNode2;
    private QdrantClientSettings _clientSettings;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();

        _clientSettings = GetQdrantClientSettings();
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
    public async Task CreateSnapshot()
    {
        await PrepareCollection(_qdrantHttpClientClusterNode1, TestCollectionName);

        var createNode1SnapshotResult =
            await _qdrantHttpClientClusterNode1.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        createNode1SnapshotResult.Status.IsSuccess.Should().BeTrue();
        createNode1SnapshotResult.Result.Name.Should().Contain(TestCollectionName);
        createNode1SnapshotResult.Result.Size.Should().BeGreaterThan(0);
        createNode1SnapshotResult.Result.SizeMegabytes.Should().BeGreaterThan(0);
        createNode1SnapshotResult.Result.Checksum.Should().NotBeNullOrEmpty();
        createNode1SnapshotResult.Result.SnapshotType.Should().Be(SnapshotType.Collection);

        var createNode2SnapshotResult =
            await _qdrantHttpClientClusterNode2.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None);

        createNode2SnapshotResult.Status.IsSuccess.Should().BeTrue();
        createNode2SnapshotResult.Result.Name.Should().Contain(TestCollectionName);
        createNode2SnapshotResult.Result.Size.Should().BeGreaterThan(0);
        createNode2SnapshotResult.Result.SizeMegabytes.Should().BeGreaterThan(0);
        createNode2SnapshotResult.Result.Checksum.Should().NotBeNullOrEmpty();
        createNode2SnapshotResult.Result.SnapshotType.Should().Be(SnapshotType.Collection);

        createNode1SnapshotResult.Result.Name.Should().NotBe(createNode2SnapshotResult.Result.Name);

        // Drop snapshots to clean up

        (await _qdrantHttpClientClusterNode1.DeleteCollectionSnapshot(
            TestCollectionName,
            createNode1SnapshotResult.Result.Name,
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClientClusterNode2.DeleteCollectionSnapshot(
            TestCollectionName,
            createNode2SnapshotResult.Result.Name,
            CancellationToken.None)).EnsureSuccess();
    }

    [Test]
    public async Task ListSnapshots()
    {
        await PrepareCollection(_qdrantHttpClientClusterNode1, TestCollectionName);

        // create first snapshot on first node 

        var createFirsNodeSnapshotResult = (await _qdrantHttpClientClusterNode1
            .CreateCollectionSnapshot(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        var listSnapshotsOnFirstNodeResult =
            await _qdrantHttpClientClusterNode1.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);

        listSnapshotsOnFirstNodeResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsOnFirstNodeResult.Result.Count.Should().Be(1); // one snapshot created

        // check snapshot on second node as well - it should not be there

        var listSnapshotsOnSecondNodeResult =
            await _qdrantHttpClientClusterNode2.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);
        listSnapshotsOnSecondNodeResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsOnSecondNodeResult.Result.Count.Should().Be(0); // no snapshots on second node

        var firstNodeSnapshotInfo = listSnapshotsOnFirstNodeResult.Result.Single();

        firstNodeSnapshotInfo.Name.Should().Be(createFirsNodeSnapshotResult.Name);
        firstNodeSnapshotInfo.Size.Should().Be(createFirsNodeSnapshotResult.Size);
        firstNodeSnapshotInfo.SizeMegabytes.Should().Be(createFirsNodeSnapshotResult.SizeMegabytes);
        firstNodeSnapshotInfo.CreationTime.Should().Be(createFirsNodeSnapshotResult.CreationTime);
        firstNodeSnapshotInfo.Checksum.Should().Be(createFirsNodeSnapshotResult.Checksum);
        firstNodeSnapshotInfo.SnapshotType.Should().Be(SnapshotType.Collection);

        // create second snapshot on second node

        var createSecondNodeSnapshotResult =
            (await _qdrantHttpClientClusterNode2.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var listSnapshotsOnSecondeNodeResult =
            await _qdrantHttpClientClusterNode2.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);

        listSnapshotsOnSecondeNodeResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsOnSecondeNodeResult.Result.Count.Should()
            .Be(1); // one snapshot since we are created the first one on the first node

        var secondNodeSnapshotInfo = listSnapshotsOnSecondeNodeResult.Result.Single();

        secondNodeSnapshotInfo.Name.Should().Be(createSecondNodeSnapshotResult.Name);
        secondNodeSnapshotInfo.Size.Should().Be(createSecondNodeSnapshotResult.Size);
        secondNodeSnapshotInfo.Checksum.Should().Be(createSecondNodeSnapshotResult.Checksum);
        secondNodeSnapshotInfo.CreationTime.Should().Be(createSecondNodeSnapshotResult.CreationTime);
        secondNodeSnapshotInfo.SnapshotType.Should().Be(SnapshotType.Collection);

        // Drop snapshots to clean up

        (await _qdrantHttpClientClusterNode1.DeleteCollectionSnapshot(
            TestCollectionName,
            createFirsNodeSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClientClusterNode2.DeleteCollectionSnapshot(
            TestCollectionName,
            createSecondNodeSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();
    }

    [Test]
    public async Task DeleteSnapshot()
    {
        await PrepareCollection(_qdrantHttpClientClusterNode1, TestCollectionName);

        // create first snapshot and delete it

        var createSnapshotResult =
            (await _qdrantHttpClientClusterNode1.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var listSnapshotsResult =
            (await _qdrantHttpClientClusterNode1.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        listSnapshotsResult.Count.Should().Be(1);

        var deleteSnapshotResult =
            await _qdrantHttpClientClusterNode1.DeleteCollectionSnapshot(
                TestCollectionName,
                createSnapshotResult.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        var listSnapshotsAfterDeletionResult =
            (await _qdrantHttpClientClusterNode1.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        listSnapshotsAfterDeletionResult.Count.Should().Be(0);

        // create two snapshots and delete one

        var createFirstNodeSnapshotResult =
            (await _qdrantHttpClientClusterNode1.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var listSnapshotsOnFirstNodeResult =
            (await _qdrantHttpClientClusterNode1.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        listSnapshotsOnFirstNodeResult.Count.Should().Be(1);

        var createSecondNodeSnapshotResult =
            (await _qdrantHttpClientClusterNode2.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var listSnapshotsOnSecondNodeResult =
            (await _qdrantHttpClientClusterNode2.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        listSnapshotsOnSecondNodeResult.Count.Should().Be(1);

        deleteSnapshotResult =
            await _qdrantHttpClientClusterNode1.DeleteCollectionSnapshot(
                TestCollectionName,
                createFirstNodeSnapshotResult.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        var listFirstNodeRemainingSnapshotsResult =
            (await _qdrantHttpClientClusterNode1.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        listFirstNodeRemainingSnapshotsResult.Count.Should().Be(0);

        // We deleted snapshot on one node only. Second node snapshot should not have been deleted
        // since snapshots are local to each node

        var listSecondNodeRemainingSnapshotsResult =
            (await _qdrantHttpClientClusterNode2.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        listSecondNodeRemainingSnapshotsResult.Count.Should().Be(1);

        listSecondNodeRemainingSnapshotsResult.Single().Name.Should().Be(createSecondNodeSnapshotResult.Name);

        // Clean up second snapshot

        (await _qdrantHttpClientClusterNode2.DeleteCollectionSnapshot(
            TestCollectionName,
            createSecondNodeSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();
    }

    [Test]
    public async Task DownloadSnapshot()
    {
        await PrepareCollection(_qdrantHttpClientClusterNode1, TestCollectionName);

        var createFirstNodeSnapshotResult =
            (await _qdrantHttpClientClusterNode1.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var createSecondNodeSnapshotResult =
            (await _qdrantHttpClientClusterNode2.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var downloadFirstNodeSnapshotResponse = await _qdrantHttpClientClusterNode1.DownloadCollectionSnapshot(
            TestCollectionName,
            createFirstNodeSnapshotResult.Name,
            CancellationToken.None);

        var downloadSecondNodeSnapshotResponse = await _qdrantHttpClientClusterNode2.DownloadCollectionSnapshot(
            TestCollectionName,
            createSecondNodeSnapshotResult.Name,
            CancellationToken.None);

        await AssertSnapshotAsync(downloadFirstNodeSnapshotResponse, createFirstNodeSnapshotResult);
        await AssertSnapshotAsync(downloadSecondNodeSnapshotResponse, createSecondNodeSnapshotResult);

        // Clean up snapshots

        (await _qdrantHttpClientClusterNode1.DeleteCollectionSnapshot(
            TestCollectionName,
            createFirstNodeSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClientClusterNode2.DeleteCollectionSnapshot(
            TestCollectionName,
            createSecondNodeSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();

        async Task AssertSnapshotAsync(DownloadSnapshotResponse downloadedSnapshot, SnapshotInfo createdSnapshot)
        {
            downloadedSnapshot.Status.IsSuccess.Should().BeTrue();
            downloadedSnapshot.Result.SnapshotName.Should().Be(createdSnapshot.Name);

            downloadedSnapshot.Result.SnapshotSizeBytes.Should().Be(
                !_clientSettings.EnableCompression
                    ? createdSnapshot.Size
                    : 0
            );

            downloadedSnapshot.Result.SnapshotDataStream.Should().NotBeNull();
            downloadedSnapshot.Result.SnapshotType.Should().Be(SnapshotType.Collection);

            await AssertSnapshotActualSize(
                downloadedSnapshot.Result.SnapshotDataStream,
                createdSnapshot.Size);
        }
    }

    [Test]
    public async Task RecoverFromSnapshot()
    {
        var expectedVectorCount = 50;

        await PrepareCollection(
            _qdrantHttpClientClusterNode1,
            TestCollectionName,
            vectorCount: expectedVectorCount);

        var createFirstNodeSnapshotResult =
            (await _qdrantHttpClientClusterNode1.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var createSecondNodeSnapshotResult =
            (await _qdrantHttpClientClusterNode2.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        // delete collection

        (await _qdrantHttpClientClusterNode1.DeleteCollection(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var listCollectionsResult =
            (await _qdrantHttpClientClusterNode1.ListCollections(CancellationToken.None)).EnsureSuccess();
        listCollectionsResult.Collections.Length.Should().Be(0);

        // recover collection from local snapshot on both nodes

        var recoverFirstNodeCollectionResult = await _qdrantHttpClientClusterNode1.RecoverCollectionFromSnapshot(
            TestCollectionName,
            createFirstNodeSnapshotResult.Name,
            CancellationToken.None,
            isWaitForResult: true,
            SnapshotPriority.Snapshot,
            snapshotChecksum: createFirstNodeSnapshotResult.Checksum);

        var recoverSecondNodeCollectionResult = await _qdrantHttpClientClusterNode2.RecoverCollectionFromSnapshot(
            TestCollectionName,
            createSecondNodeSnapshotResult.Name,
            CancellationToken.None,
            isWaitForResult: true,
            SnapshotPriority.Snapshot,
            snapshotChecksum: createSecondNodeSnapshotResult.Checksum);

        AssertSnapshot(recoverFirstNodeCollectionResult);
        AssertSnapshot(recoverSecondNodeCollectionResult);

        void AssertSnapshot(DefaultOperationResponse recoverCollectionResult)
        {
            recoverCollectionResult.Status.IsSuccess.Should().BeTrue();
            recoverCollectionResult.Result.Should().BeTrue();
            recoverCollectionResult.Time.Should().BeGreaterThan(0);
        }

        await Task.Delay(TimeSpan.FromMilliseconds(500)); // wait for cluster to stabilize

        await _qdrantHttpClientClusterNode1.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        // check collection recovered

        listCollectionsResult =
            (await _qdrantHttpClientClusterNode1.ListCollections(CancellationToken.None)).EnsureSuccess();

        listCollectionsResult.Collections.Length.Should().Be(1);
        listCollectionsResult.Collections[0].Name.Should().Be(TestCollectionName);

        var recoveredCollectionInfo = (await _qdrantHttpClientClusterNode1.GetCollectionInfo(
                TestCollectionName,
                isCountExactPointsNumber: true,
                CancellationToken.None)
            ).EnsureSuccess();

        recoveredCollectionInfo.Status.Should().Be(QdrantCollectionStatus.Green);

        // Check collection data is recovered

        var countPointsResult = (await _qdrantHttpClientClusterNode1.CountPoints(
            TestCollectionName,
            new CountPointsRequest(),
            CancellationToken.None)).EnsureSuccess();

        // If we fail here we might consider increasing the Task.Delay time above

        countPointsResult.Count.Should().Be((ulong)expectedVectorCount);

        // Clean up snapshots

        (await _qdrantHttpClientClusterNode1.DeleteCollectionSnapshot(
            TestCollectionName,
            createFirstNodeSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClientClusterNode2.DeleteCollectionSnapshot(
            TestCollectionName,
            createSecondNodeSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();
    }

    [Test]
    public async Task RecoverFromUploadedSnapshot()
    {
        var expectedVectorCount = 50;

        await PrepareCollection(
            _qdrantHttpClientClusterNode1,
            TestCollectionName,
            vectorCount: expectedVectorCount);

        // Create and download snapshot from first node

        var createFirstNodeSnapshotResult =
            (await _qdrantHttpClientClusterNode1.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var downloadedFirstNodeSnapshotResult = (await _qdrantHttpClientClusterNode1.DownloadCollectionSnapshot(
            TestCollectionName,
            createFirstNodeSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();

        MemoryStream downloadedFirstNodeSnapshotStream = new MemoryStream();
        await downloadedFirstNodeSnapshotResult.SnapshotDataStream.CopyToAsync(downloadedFirstNodeSnapshotStream);
        downloadedFirstNodeSnapshotStream.Position = 0;

        var createSecondNodeSnapshotResult =
            (await _qdrantHttpClientClusterNode2.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var downloadedSecondNodeSnapshotResult = (await _qdrantHttpClientClusterNode2.DownloadCollectionSnapshot(
            TestCollectionName,
            createSecondNodeSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();

        MemoryStream downloadedSecondNodeSnapshotStream = new MemoryStream();
        await downloadedSecondNodeSnapshotResult.SnapshotDataStream.CopyToAsync(downloadedSecondNodeSnapshotStream);
        downloadedSecondNodeSnapshotStream.Position = 0;

        // delete collection

        (await _qdrantHttpClientClusterNode1.DeleteCollection(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var listCollectionsResult =
            (await _qdrantHttpClientClusterNode1.ListCollections(CancellationToken.None)).EnsureSuccess();
        listCollectionsResult.Collections.Length.Should().Be(0);

        // recover collection from downloaded snapshot on both nodes

        var recoverFirstNodeCollectionResult =
            await _qdrantHttpClientClusterNode1.RecoverCollectionFromUploadedSnapshot(
                TestCollectionName,
                downloadedFirstNodeSnapshotStream,
                CancellationToken.None,
                isWaitForResult: true,
                SnapshotPriority.Snapshot,
                snapshotChecksum: createFirstNodeSnapshotResult.Checksum);

        var recoverSecondNodeCollectionResult =
            await _qdrantHttpClientClusterNode2.RecoverCollectionFromUploadedSnapshot(
                TestCollectionName,
                downloadedSecondNodeSnapshotStream,
                CancellationToken.None,
                isWaitForResult: true,
                SnapshotPriority.Snapshot,
                snapshotChecksum: createSecondNodeSnapshotResult.Checksum);

        recoverFirstNodeCollectionResult.Status.IsSuccess.Should().BeTrue();
        recoverFirstNodeCollectionResult.Result.Should().BeTrue();

        recoverSecondNodeCollectionResult.Status.IsSuccess.Should().BeTrue();
        recoverSecondNodeCollectionResult.Result.Should().BeTrue();

        await Task.Delay(TimeSpan.FromMilliseconds(500)); // wait for cluster to stabilize

        await _qdrantHttpClientClusterNode1.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true);

        // check collection recovered

        listCollectionsResult =
            (await _qdrantHttpClientClusterNode1.ListCollections(CancellationToken.None)).EnsureSuccess();

        listCollectionsResult.Collections.Length.Should().Be(1);
        listCollectionsResult.Collections[0].Name.Should().Be(TestCollectionName);

        var recoveredCollectionInfo = (await _qdrantHttpClientClusterNode1.GetCollectionInfo(
                TestCollectionName,
                isCountExactPointsNumber: true,
                CancellationToken.None)
            ).EnsureSuccess();

        recoveredCollectionInfo.Status.Should().Be(QdrantCollectionStatus.Green);

        // Check collection data is recovered

        var countPointsResult = (await _qdrantHttpClientClusterNode1.CountPoints(
            TestCollectionName,
            new CountPointsRequest(),
            CancellationToken.None)).EnsureSuccess();

        // If we fail here we might consider increasing the Task.Delay time above

        countPointsResult.Count.Should().Be((ulong)expectedVectorCount);

        // Clean up snapshots

        (await _qdrantHttpClientClusterNode1.DeleteCollectionSnapshot(
            TestCollectionName,
            createFirstNodeSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();

        (await _qdrantHttpClientClusterNode2.DeleteCollectionSnapshot(
            TestCollectionName,
            createSecondNodeSnapshotResult.Name,
            CancellationToken.None)).EnsureSuccess();
    }
}
