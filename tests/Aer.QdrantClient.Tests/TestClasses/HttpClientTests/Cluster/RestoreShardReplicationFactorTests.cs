using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

#if !DEBUG
[Ignore(
    "I didn't find a way to configure both single-node and a multi-node cluster in "
        + "GitHub actions so these tests will run only locally"
)]
#endif
internal class RestoreShardReplicationFactorTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient1;
    private QdrantHttpClient _qdrantHttpClient2;
    private QdrantHttpClient _qdrantHttpClient3;
    private ILogger _logger;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();
        _qdrantHttpClient1 = Get3NodeClusterClient(ClusterNode.First);
        _qdrantHttpClient2 = Get3NodeClusterClient(ClusterNode.Second);
        _qdrantHttpClient3 = Get3NodeClusterClient(ClusterNode.Third);

        _logger = ServiceProvider.GetRequiredService<ILogger<ClusterCompoundOperationsTests>>();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage(_qdrantHttpClient1);
    }

    [Test]
    public async Task RestoreShardReplicationFactor_NoReplicationRequired()
    {
        await PrepareCollection(_qdrantHttpClient1, TestCollectionName, replicationFactor: 2, vectorCount: 100, shardCount: 6);

        var restoreReplicationFactorResponse = await _qdrantHttpClient1.RestoreShardReplicationFactor(
            TestCollectionName,
            CancellationToken.None
        );

        restoreReplicationFactorResponse.Status.IsSuccess.Should().BeTrue();

        var shardReplicator = restoreReplicationFactorResponse.Result;

        shardReplicator.ShardsNeedReplication.Should().BeFalse();

        shardReplicator.ReplicationPlan.Should().BeEmpty();

        var nonexistentReplicationExecuteResult = await shardReplicator.ExecuteNextReplication(CancellationToken.None);

        nonexistentReplicationExecuteResult.Status.IsSuccess.Should().BeFalse();
        nonexistentReplicationExecuteResult.Status.GetErrorMessage().Should().Contain("No replications to execute");
    }

    [Test]
    [TestCase(2, 6)]
    [TestCase(2, 7)]
    [TestCase(2, 5)]
    public async Task RestoreShardReplicationFactor(int replicationFactor, int shardCount)
    {
        /*
        This is an example for replicationFactor = 2 and shardCount = 6.

        This test checks whether the following transition happens when RestoreShardReplicationFactor is called:
        Note that the shard numbers may differ since qdrant chooses shard placement at random

        Node 1   Node 2   Node 3
        S0 S3     S1 S4    S2 S5
        S2 S5     S0 S3    S1 S4
        ------------------------- Move replicas until this state. S4 is more than replication factor, S0 is less, s3 is misplaced
        S0 S3     S1 S4    S2 S5
        S2 S5              S1 S4
        S4                 S3
        ------------------------- Call RestoreShardReplicationFactor, the replication factor and 4 shards per node should be recovered
        ------------------------- Note that the exact shard numbers might be mixed up but the 4 shards per node rule should hold
        S0 S3     S1 S4    S2 S5
        S2 S5     S0 S3    S1 S4
        */

        await PrepareCollection(
            _qdrantHttpClient1,
            TestCollectionName,
            replicationFactor: replicationFactor,
            vectorCount: 100,
            shardCount: shardCount
        );

        var node1Info = (await _qdrantHttpClient1.GetPeerInfo("qdrant-11", CancellationToken.None)).EnsureSuccess();

        var node2Info = (await _qdrantHttpClient2.GetPeerInfo("qdrant-12", CancellationToken.None)).EnsureSuccess();

        var node3Info = (await _qdrantHttpClient3.GetPeerInfo("qdrant-13", CancellationToken.None)).EnsureSuccess();

        // Since obtained collection clustering info contains all shards by peers straight and reversed dictionaries we can use any peer info

        var node1ShardState = (
            await _qdrantHttpClient1.GetCollectionClusteringInfo(
                TestCollectionName,
                CancellationToken.None,
                isTranslatePeerIdsToUris: true
            )
        ).EnsureSuccess();

        // Prepare initial unbalanced cluster state

        var notFoundShardId = 1567U; // sentinel value for indicating that shard was not found

        // Copy one shard from node 2 to node 1

        var shardNotPresentOnNode1 = node1ShardState
            .ShardsByPeers[node2Info.PeerId]
            .Except(node1ShardState.ShardsByPeers[node1Info.PeerId])
            .FirstOrDefault(notFoundShardId);

        shardNotPresentOnNode1.Should().NotBe(notFoundShardId);

        (
            await _qdrantHttpClient1.ReplicateShards(
                sourcePeerId: node2Info.PeerId,
                targetPeerId: node1ShardState.PeerId,
                CancellationToken.None,
                collectionNamesToReplicate: [TestCollectionName],
                shardIdsToReplicate: [shardNotPresentOnNode1]
            )
        ).EnsureSuccess();

        // Delete one shard from node 2 (not the one we copied)

        var shardToDeleteFromNode2 = node1ShardState.ShardsByPeers[node2Info.PeerId].Except([shardNotPresentOnNode1]).First();

        (
            await _qdrantHttpClient1.DropCollectionShardsFromPeer(
                TestCollectionName,
                peerId: node2Info.PeerId,
                shardIds: [shardToDeleteFromNode2],
                CancellationToken.None
            )
        ).EnsureSuccess();

        // Move another node from node 2 to node 3

        var shardToMoveToNode3 = node1ShardState
            .ShardsByPeers[node2Info.PeerId]
            .Except([shardNotPresentOnNode1, shardToDeleteFromNode2, .. node1ShardState.ShardsByPeers[node3Info.PeerId]])
            .FirstOrDefault(notFoundShardId);

        // If we can't move shard from node 2 to node 3 - try to move it to node 1 instead

        if (shardToMoveToNode3 != notFoundShardId)
        {
            // Means we can move shard to node 3
            (
                await _qdrantHttpClient1.UpdateCollectionClusteringSetup(
                    collectionName: TestCollectionName,
                    UpdateCollectionClusteringSetupRequest.CreateMoveShardRequest(
                        shardToMoveToNode3,
                        fromPeerId: node2Info.PeerId,
                        toPeerId: node3Info.PeerId,
                        shardTransferMethod: Http.Models.Shared.ShardTransferMethod.Snapshot
                    ),
                    CancellationToken.None
                )
            ).EnsureSuccess();
        }
        else
        {
            // Means we can't move shard to node 3 since node 3 already has that shard. Try to move it to node 1

            var shardToMoveToNode1 = node1ShardState
                .ShardsByPeers[node2Info.PeerId]
                .Except([shardNotPresentOnNode1, shardToDeleteFromNode2, .. node1ShardState.ShardsByPeers[node1Info.PeerId]])
                .FirstOrDefault(notFoundShardId);

            (
                await _qdrantHttpClient1.UpdateCollectionClusteringSetup(
                    collectionName: TestCollectionName,
                    UpdateCollectionClusteringSetupRequest.CreateMoveShardRequest(
                        shardToMoveToNode1,
                        fromPeerId: node2Info.PeerId,
                        toPeerId: node1Info.PeerId,
                        shardTransferMethod: Http.Models.Shared.ShardTransferMethod.Snapshot
                    ),
                    CancellationToken.None
                )
            ).EnsureSuccess();
        }

        await _qdrantHttpClient1.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true
        );

        await _qdrantHttpClient2.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true
        );

        await _qdrantHttpClient3.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true
        );

        // Restore replication factor

        var restoreReplicationFactorResponse = await _qdrantHttpClient1.RestoreShardReplicationFactor(
            TestCollectionName,
            CancellationToken.None
        );

        restoreReplicationFactorResponse.Status.IsSuccess.Should().BeTrue();

        var shardReplicator = restoreReplicationFactorResponse.Result;

        shardReplicator.ShardsNeedReplication.Should().BeTrue();

        shardReplicator.ReplicationPlan.Should().NotBeEmpty();

        shardReplicator._targetCollectionClusteringState.Should().NotBeNull();

        await foreach (var shardReplicationResult in shardReplicator.ExecuteReplications(CancellationToken.None))
        {
            shardReplicationResult.Status.IsSuccess.Should().BeTrue();

            shardReplicationResult.Result.ReplicatedShards.Count.Should().Be(1);

            var singleShardReplicationResult = shardReplicationResult.Result.ReplicatedShards[0];

            singleShardReplicationResult.CollectionName.Should().Be(TestCollectionName);

            singleShardReplicationResult.IsSuccess.Should().BeTrue();

            // Wait for replication to complete before moving to the next replication step
            await _qdrantHttpClient1.EnsureCollectionReady(
                TestCollectionName,
                CancellationToken.None,
                isCheckShardTransfersCompleted: true
            );
        }

        var node1ShardStateAfterReplication = (
            await _qdrantHttpClient1.GetCollectionClusteringInfo(
                TestCollectionName,
                CancellationToken.None,
                isTranslatePeerIdsToUris: true
            )
        ).EnsureSuccess();

        node1ShardStateAfterReplication
            .ShardsByPeers.All(p =>
                p.Value.Count >= shardReplicator._targetCollectionClusteringState.MinNumberOfReplicasPerPeer
                && p.Value.Count <= shardReplicator._targetCollectionClusteringState.MaxNumberOfReplicasPerPeer
            )
            .Should()
            .BeTrue();
    }

    [Test]
    public async Task RestoreShardReplicationFactor_ParallelCollectionUpdate()
    {
        // The test itself is basically the same as RestoreShardReplicationFactor but on the second replication step
        // we change collection clustering to be different from expected one

        await PrepareCollection(
            _qdrantHttpClient1,
            TestCollectionName,
            replicationFactor: 2,
            vectorCount: 100,
            shardCount: 6
        );

        var node1Info = (await _qdrantHttpClient1.GetPeerInfo("qdrant-11", CancellationToken.None)).EnsureSuccess();

        var node2Info = (await _qdrantHttpClient2.GetPeerInfo("qdrant-12", CancellationToken.None)).EnsureSuccess();

        // Since obtained collection clustering info contains all shards by peers straight and reversed dictionaries we can use any peer info

        var node1ShardState = (
            await _qdrantHttpClient1.GetCollectionClusteringInfo(
                TestCollectionName,
                CancellationToken.None,
                isTranslatePeerIdsToUris: true
            )
        ).EnsureSuccess();

        // Prepare initial unbalanced cluster state. Two imbalances is enough since we are going to change collection clustering after the first replication

        var notFoundShardId = 1567U; // sentinel value for indicating that shard was not found

        // Copy one shard from node 2 to node 1

        var shardNotPresentOnNode1 = node1ShardState
            .ShardsByPeers[node2Info.PeerId]
            .Except(node1ShardState.ShardsByPeers[node1Info.PeerId])
            .FirstOrDefault(notFoundShardId);

        shardNotPresentOnNode1.Should().NotBe(notFoundShardId);

        (
            await _qdrantHttpClient1.ReplicateShards(
                sourcePeerId: node2Info.PeerId,
                targetPeerId: node1ShardState.PeerId,
                CancellationToken.None,
                collectionNamesToReplicate: [TestCollectionName],
                shardIdsToReplicate: [shardNotPresentOnNode1]
            )
        ).EnsureSuccess();

        // Delete one shard from node 2 (not the one we copied)

        var shardToDeleteFromNode2 = node1ShardState.ShardsByPeers[node2Info.PeerId].Except([shardNotPresentOnNode1]).First();

        (
            await _qdrantHttpClient1.DropCollectionShardsFromPeer(
                TestCollectionName,
                peerId: node2Info.PeerId,
                shardIds: [shardToDeleteFromNode2],
                CancellationToken.None
            )
        ).EnsureSuccess();

        await _qdrantHttpClient1.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true
        );

        await _qdrantHttpClient2.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true
        );

        await _qdrantHttpClient3.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true
        );

        // Restore replication factor

        var restoreReplicationFactorResponse = await _qdrantHttpClient1.RestoreShardReplicationFactor(
            TestCollectionName,
            CancellationToken.None
        );

        restoreReplicationFactorResponse.Status.IsSuccess.Should().BeTrue();

        var shardReplicator = restoreReplicationFactorResponse.Result;

        shardReplicator.ShardsNeedReplication.Should().BeTrue();

        shardReplicator.ReplicationPlan.Should().NotBeEmpty();

        shardReplicator.ReplicationPlan.Count.Should().Be(2);

        var firstReplicationResult = await shardReplicator.ExecuteNextReplication(CancellationToken.None);

        firstReplicationResult.Status.IsSuccess.Should().BeTrue();

        // Wait for replication to complete before moving to the next replication step
        await _qdrantHttpClient1.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true
        );

        // Change collection clustering inf so that the expected state is different from an actual one
        // Any change will do, so we just redo the initial unbalancing that should have been fixed by first replication.

        (
            await _qdrantHttpClient1.ReplicateShards(
                sourcePeerId: node2Info.PeerId,
                targetPeerId: node1ShardState.PeerId,
                CancellationToken.None,
                collectionNamesToReplicate: [TestCollectionName],
                shardIdsToReplicate: [shardNotPresentOnNode1]
            )
        ).EnsureSuccess();

        await _qdrantHttpClient1.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true
        );

        // Try to commence next replication step - it should fail
        var secondReplicationAfterClusterChangeResult = await shardReplicator.ExecuteNextReplication(CancellationToken.None);

        secondReplicationAfterClusterChangeResult.Status.IsSuccess.Should().BeFalse();
        secondReplicationAfterClusterChangeResult.Status.GetErrorMessage().Should()
            .Contain("Can't restore shard replication factor")
            .And.Contain("collection clustering was changed");
    }

    //[Test]
    //public async Task RestoreShardReplicationFactor_UnderpopulationEdgeCase(int replicationFactor, int shardCount)
    //{

    //}
}
