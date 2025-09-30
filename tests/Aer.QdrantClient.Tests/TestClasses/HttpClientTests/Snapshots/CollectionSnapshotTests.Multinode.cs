using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots;

public class CollectionSnapshotTestsMultiNode : SnapshotTestsBase
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
    public async Task CreateSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClientClusterNode1, TestCollectionName);

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
    }

    [Test]
    public async Task ListSnapshots()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClientClusterNode1, TestCollectionName);

        // create first snapshot on first node 

        var createFirsOnFirstNodeSnapshotResult = (await _qdrantHttpClientClusterNode1
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

        firstNodeSnapshotInfo.Name.Should().Be(createFirsOnFirstNodeSnapshotResult.Name);
        firstNodeSnapshotInfo.Size.Should().Be(createFirsOnFirstNodeSnapshotResult.Size);
        firstNodeSnapshotInfo.SizeMegabytes.Should().Be(createFirsOnFirstNodeSnapshotResult.SizeMegabytes);
        firstNodeSnapshotInfo.CreationTime.Should().Be(createFirsOnFirstNodeSnapshotResult.CreationTime);
        firstNodeSnapshotInfo.Checksum.Should().Be(createFirsOnFirstNodeSnapshotResult.Checksum);
        firstNodeSnapshotInfo.SnapshotType.Should().Be(SnapshotType.Collection);

        // create second snapshot on second node

        var createSecondSnapshotResult =
            (await _qdrantHttpClientClusterNode2.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var listSnapshotsOnSecondeNodeResult =
            await _qdrantHttpClientClusterNode2.ListCollectionSnapshots(TestCollectionName, CancellationToken.None);

        listSnapshotsOnSecondeNodeResult.Status.IsSuccess.Should().BeTrue();
        listSnapshotsOnSecondeNodeResult.Result.Count.Should().Be(1); // one snapshot since we are created the first one on the first node

        var secondNodeSnapshotInfo = listSnapshotsOnSecondeNodeResult.Result.Single();

        secondNodeSnapshotInfo.Name.Should().Be(createSecondSnapshotResult.Name);
        secondNodeSnapshotInfo.Size.Should().Be(createSecondSnapshotResult.Size);
        secondNodeSnapshotInfo.Checksum.Should().Be(createSecondSnapshotResult.Checksum);
        secondNodeSnapshotInfo.CreationTime.Should().Be(createSecondSnapshotResult.CreationTime);
        secondNodeSnapshotInfo.SnapshotType.Should().Be(SnapshotType.Collection);
    }
    
    [Test]
    public async Task DeleteSnapshot()
    {
        await PrepareCollection<TestPayload>(_qdrantHttpClientClusterNode1, TestCollectionName);

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

        var createFirstSnapshotResult =
            (await _qdrantHttpClientClusterNode1.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        var listSnapshotsOnFirstNodeResult =
            (await _qdrantHttpClientClusterNode1.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();
        
        listSnapshotsOnFirstNodeResult.Count.Should().Be(1);

        var createSecondSnapshotResult =
            (await _qdrantHttpClientClusterNode2.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();
        
        var listSnapshotsOnSecondNodeResult =
            (await _qdrantHttpClientClusterNode2.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();
        
        listSnapshotsOnSecondNodeResult.Count.Should().Be(1);

        deleteSnapshotResult =
            await _qdrantHttpClientClusterNode1.DeleteCollectionSnapshot(
                TestCollectionName,
                createFirstSnapshotResult.Name,
                CancellationToken.None);

        deleteSnapshotResult.Status.IsSuccess.Should().BeTrue();
        deleteSnapshotResult.Result.Should().BeTrue();

        var listNodeOneRemainingSnapshotsResult =
            (await _qdrantHttpClientClusterNode1.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();
        
        listNodeOneRemainingSnapshotsResult.Count.Should().Be(0);

        // We deleted snapshot on one node only. Second node snapshot should not have been deleted
        // since snapshots are local to each node
        
        var listNodeTwoRemainingSnapshotsResult =
            (await _qdrantHttpClientClusterNode1.ListCollectionSnapshots(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        listNodeTwoRemainingSnapshotsResult.Count.Should().Be(1);

        listNodeOneRemainingSnapshotsResult.Single().Name.Should().Be(createSecondSnapshotResult.Name);
    }


    // [Test]
    // public async Task DownloadSnapshot()
    // {
    //     await PrepareCollection<TestPayload>(_qdrantHttpClientSingleNode, TestCollectionName);
    //
    //     var createSnapshotResult =
    //         (await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
    //         .EnsureSuccess();
    //
    //     var downloadSnapshotResponse = await _qdrantHttpClientSingleNode.DownloadCollectionSnapshot(
    //         TestCollectionName,
    //         createSnapshotResult.Name,
    //         CancellationToken.None);
    //
    //     downloadSnapshotResponse.Status.IsSuccess.Should().BeTrue();
    //     downloadSnapshotResponse.Result.SnapshotName.Should().Be(createSnapshotResult.Name);
    //     downloadSnapshotResponse.Result.SnapshotSizeBytes.Should().Be(createSnapshotResult.Size);
    //
    //     downloadSnapshotResponse.Result.SnapshotDataStream.Should().NotBeNull();
    //
    //     downloadSnapshotResponse.Result.SnapshotType.Should().Be(SnapshotType.Collection);
    //
    //     await AssertSnapshotActualSize(downloadSnapshotResponse.Result.SnapshotDataStream, createSnapshotResult.Size);
    // }

    // [Test]
    // public async Task RecoverFromSnapshot()
    // {
    //     var expectedVectorCount = 50;
    //     var (points, _, _) =
    //         await PrepareCollection<TestPayload>(
    //             _qdrantHttpClientSingleNode,
    //             TestCollectionName,
    //             vectorCount: expectedVectorCount);
    //
    //     var createSnapshotResult =
    //         (await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
    //         .EnsureSuccess();
    //
    //     // delete collection
    //
    //     (await _qdrantHttpClientSingleNode.DeleteCollection(TestCollectionName, CancellationToken.None)).EnsureSuccess();
    //
    //     var listCollectionsResult = (await _qdrantHttpClientSingleNode.ListCollections(CancellationToken.None)).EnsureSuccess();
    //     listCollectionsResult.Collections.Length.Should().Be(0);
    //
    //     // recover collection from local snapshot
    //
    //     var recoverCollectionResult = await _qdrantHttpClientSingleNode.RecoverCollectionFromSnapshot(
    //         TestCollectionName,
    //         createSnapshotResult.Name,
    //         CancellationToken.None,
    //         isWaitForResult: true,
    //         SnapshotPriority.Snapshot,
    //         snapshotChecksum: createSnapshotResult.Checksum);
    //
    //     recoverCollectionResult.Status.IsSuccess.Should().BeTrue();
    //     recoverCollectionResult.Result.Should().BeTrue();
    //     recoverCollectionResult.Time.Should().BeGreaterThan(0);
    //
    //     var collectionInfo = (await _qdrantHttpClientSingleNode.GetCollectionInfo(
    //             TestCollectionName,
    //             isCountExactPointsNumber: true,
    //             CancellationToken.None)
    //         ).EnsureSuccess();
    //
    //     // check collection recovered
    //
    //     listCollectionsResult = (await _qdrantHttpClientSingleNode.ListCollections(CancellationToken.None)).EnsureSuccess();
    //
    //     listCollectionsResult.Collections.Length.Should().Be(1);
    //     listCollectionsResult.Collections[0].Name.Should().Be(TestCollectionName);
    //
    //     // Check collection data is recovered
    //
    //     var countPointsResult = (await _qdrantHttpClientSingleNode.CountPoints(
    //         TestCollectionName,
    //         new CountPointsRequest(),
    //         CancellationToken.None)).EnsureSuccess();
    //
    //     countPointsResult.Count.Should().Be((ulong)expectedVectorCount);
    // }
    //
    // [Test]
    // public async Task RecoverFromUploadedSnapshot()
    // {
    //     await PrepareCollection<TestPayload>(_qdrantHttpClientSingleNode, TestCollectionName);
    //
    //     // Create and download snapshot
    //
    //     var createSnapshotResult =
    //         (await _qdrantHttpClientSingleNode.CreateCollectionSnapshot(TestCollectionName, CancellationToken.None))
    //         .EnsureSuccess();
    //
    //     // this method call is here since when collection is deleted it's impossible to download its snapshot
    //     var downloadedSnapshotResult = (await _qdrantHttpClientSingleNode.DownloadCollectionSnapshot(
    //         TestCollectionName,
    //         createSnapshotResult.Name,
    //         CancellationToken.None)).EnsureSuccess();
    //
    //     // Copy snapshot to memory
    //
    //     MemoryStream downloadedSnapshotStream = new MemoryStream();
    //     await downloadedSnapshotResult.SnapshotDataStream.CopyToAsync(downloadedSnapshotStream);
    //     downloadedSnapshotStream.Position = 0;
    //
    //     // delete collection
    //
    //     (await _qdrantHttpClientSingleNode.DeleteCollection(TestCollectionName, CancellationToken.None)).EnsureSuccess();
    //
    //     var listCollectionsResult = (await _qdrantHttpClientSingleNode.ListCollections(CancellationToken.None)).EnsureSuccess();
    //     listCollectionsResult.Collections.Length.Should().Be(0);
    //
    //     // recover collection from downloaded snapshot
    //
    //     var recoverCollectionResult = await _qdrantHttpClientSingleNode.RecoverCollectionFromUploadedSnapshot(
    //         TestCollectionName,
    //         downloadedSnapshotStream,
    //         CancellationToken.None,
    //         isWaitForResult: true,
    //         SnapshotPriority.Snapshot,
    //         snapshotChecksum: createSnapshotResult.Checksum);
    //
    //     recoverCollectionResult.Status.IsSuccess.Should().BeTrue();
    //     recoverCollectionResult.Result.Should().BeTrue();
    //
    //     // check collection recovered
    //
    //     listCollectionsResult = (await _qdrantHttpClientSingleNode.ListCollections(CancellationToken.None)).EnsureSuccess();
    //
    //     listCollectionsResult.Collections.Length.Should().Be(1);
    //     listCollectionsResult.Collections[0].Name.Should().Be(TestCollectionName);
    //
    //     // Check collection data is recovered
    //
    //     var countPointsResult = (await _qdrantHttpClientSingleNode.CountPoints(
    //         TestCollectionName,
    //         new CountPointsRequest(),
    //         CancellationToken.None)).EnsureSuccess();
    //
    //     countPointsResult.Count.Should().Be(10);
    // }
}
