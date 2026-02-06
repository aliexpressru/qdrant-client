using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Models.Requests;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal partial class ClusterTests : QdrantTestsBase
{
    [Test]
    public async Task CollectionCreateShardKey_ManualPlacement()
    {
        var vectorSize = 10U;

        (
            await _qdrantHttpClient.CreateCollection(
                TestCollectionName,
                new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
                {
                    OnDiskPayload = true,
                    WriteConsistencyFactor = 2,
                    ReplicationFactor = 1,
                    ShardNumber = 2,
                    ShardingMethod = ShardingMethod.Custom,
                },
                CancellationToken.None
            )
        ).EnsureSuccess();

        // configure collection manual sharding to ensure consistent results

        var allPeers = (await _qdrantHttpClient.GetClusterInfo(CancellationToken.None)).EnsureSuccess().AllPeerIds;

        var createFirstShardKey = await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            TestShardKey1,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.First()]
        );

        var createSecondShardKey = await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            TestShardKeyInt1,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.Skip(1).First()]
        );

        createFirstShardKey.Status.IsSuccess.Should().BeTrue();
        createFirstShardKey.Result.Should().BeTrue();

        createSecondShardKey.Status.IsSuccess.Should().BeTrue();
        createSecondShardKey.Result.Should().BeTrue();

        UpsertPointsRequest.UpsertPoint firstShardPoint = new(
            id: 1,
            vector: CreateTestVector(vectorSize),
            payload: (TestPayload)1
        );

        UpsertPointsRequest.UpsertPoint secondShardPoint = new(
            id: 2,
            vector: CreateTestVector(vectorSize),
            payload: (TestPayload)2
        );

        var upsertOnFirstShardResponse = await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest() { Points = [firstShardPoint], ShardKey = TestShardKey1 },
            CancellationToken.None
        );

        var upsertOnSecondShardResponse = await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest() { Points = [secondShardPoint], ShardKey = TestShardKeyInt1 },
            CancellationToken.None
        );

        upsertOnFirstShardResponse.Status.IsSuccess.Should().BeTrue();
        upsertOnSecondShardResponse.Status.IsSuccess.Should().BeTrue();

        var readPoints = (
            await _qdrantHttpClient.ScrollPoints(
                TestCollectionName,
                QdrantFilter.Empty,
                PayloadPropertiesSelector.All,
                CancellationToken.None,
                withVector: true,
                limit: 2
            )
        ).EnsureSuccess();

        readPoints.Points.Length.Should().Be(2);

        var firstReadPoint = readPoints.Points.Single(p => p.Id == firstShardPoint.Id);
        var secondReadPoint = readPoints.Points.Single(p => p.Id == secondShardPoint.Id);

        firstReadPoint.Payload.As<int>().Should().Be(firstShardPoint.Payload.As<int>());

        // manual cast to eliminate cyclic reference
        // Default = {Cyclic reference to type Aer.QdrantClient.Http.Models.Primitives.Vectors.DenseVector detected},
        firstReadPoint
            .Vector.Default.AsDenseVector()
            .VectorValues.Should()
            .BeEquivalentTo(firstShardPoint.Vector.Default.AsDenseVector().VectorValues);
        firstReadPoint.ShardKey.IsString().Should().BeTrue();
        firstReadPoint.ShardKey.GetString().Should().Be(TestShardKey1);

        secondReadPoint.Payload.As<int>().Should().Be(secondShardPoint.Payload.As<int>());
        secondReadPoint
            .Vector.Default.AsDenseVector()
            .VectorValues.Should()
            .BeEquivalentTo(secondShardPoint.Vector.Default.AsDenseVector().VectorValues);
        secondReadPoint.ShardKey.IsInteger().Should().BeTrue();
        secondReadPoint.ShardKey.GetInteger().Should().Be(TestShardKeyInt1);
    }

    [Test]
    public async Task CollectionCreateShardKey_WithInitialState()
    {
        OnlyIfVersionAfterOrEqual("1.16.0", "Initial shard state setting is available only after v1.16");

        var vectorSize = 10U;

        (
            await _qdrantHttpClient.CreateCollection(
                TestCollectionName,
                new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
                {
                    OnDiskPayload = true,
                    WriteConsistencyFactor = 2,
                    ReplicationFactor = 1,
                    ShardNumber = 2,
                    ShardingMethod = ShardingMethod.Custom,
                },
                CancellationToken.None
            )
        ).EnsureSuccess();

        var allPeers = (await _qdrantHttpClient.GetClusterInfo(CancellationToken.None)).EnsureSuccess().AllPeerIds;

        var createFirstShardKey = await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            TestPartialShardKey,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.First()],
            initialState: ShardState.Partial
        );

        var createSecondShardKey = await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            TestShardKeyInt1,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.Skip(1).First()]
        );

        createFirstShardKey.Status.IsSuccess.Should().BeTrue();
        createFirstShardKey.Result.Should().BeTrue();

        createSecondShardKey.Status.IsSuccess.Should().BeTrue();
        createSecondShardKey.Result.Should().BeTrue();

        var collectionClusteringInfo = (
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None)
        ).EnsureSuccess();

        collectionClusteringInfo.PartialShardCount.Should().Be(1);
        collectionClusteringInfo.DeadShardCount.Should().Be(0);
        collectionClusteringInfo.ShardCount.Should().Be(2);

        collectionClusteringInfo.LocalShards.Single().State.Should().Be(ShardState.Partial);
        collectionClusteringInfo.RemoteShards.Single().State.Should().Be(ShardState.Active);
    }

    [Test]
    public async Task CollectionCreateShardKey_ManualPlacement_WithFallback()
    {
        OnlyIfVersionAfterOrEqual("1.16.0", "Fallback shard and tiered multitenancy is available only after v1.16");

        var vectorSize = 10U;

        (
            await _qdrantHttpClient.CreateCollection(
                TestCollectionName,
                new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
                {
                    OnDiskPayload = true,
                    WriteConsistencyFactor = 2,
                    ReplicationFactor = 1,
                    ShardNumber = 2,
                    ShardingMethod = ShardingMethod.Custom,
                },
                CancellationToken.None
            )
        ).EnsureSuccess();

        // configure collection manual sharding to ensure consistent results

        var allPeers = (await _qdrantHttpClient.GetClusterInfo(CancellationToken.None)).EnsureSuccess().AllPeerIds;

        var createFirstShardKey = await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            TestShardKey1,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.First()]
        );

        var createSecondShardKey = await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            TestShardKeyInt1,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.Skip(1).First()]
        );

        createFirstShardKey.Status.IsSuccess.Should().BeTrue();
        createFirstShardKey.Result.Should().BeTrue();

        createSecondShardKey.Status.IsSuccess.Should().BeTrue();
        createSecondShardKey.Result.Should().BeTrue();

        var shardSelectorWithFallback = ShardSelector.String(
            shardKeyValue: "non-existent-shard",
            fallbackShardKeyValue: TestShardKeyInt1
        );

        UpsertPointsRequest.UpsertPoint firstShardPoint = new(
            id: 1,
            vector: CreateTestVector(vectorSize),
            payload: (TestPayload)1
        );

        UpsertPointsRequest.UpsertPoint secondShardPoint = new(
            id: 2,
            vector: CreateTestVector(vectorSize),
            payload: (TestPayload)2
        );

        var upsertOnFirstShardResponse = await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest() { Points = [firstShardPoint], ShardKey = TestShardKey1 },
            CancellationToken.None
        );

        var upsertOnSecondShardResponse = await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest() { Points = [secondShardPoint], ShardKey = shardSelectorWithFallback },
            CancellationToken.None
        );

        upsertOnFirstShardResponse.Status.IsSuccess.Should().BeTrue();
        upsertOnSecondShardResponse.Status.IsSuccess.Should().BeTrue();

        var readPoints = (
            await _qdrantHttpClient.ScrollPoints(
                TestCollectionName,
                QdrantFilter.Empty,
                PayloadPropertiesSelector.All,
                CancellationToken.None,
                withVector: true,
                limit: 2,
                shardSelector: shardSelectorWithFallback
            )
        ).EnsureSuccess();

        readPoints.Points.Length.Should().Be(1); // Only one point since we used fallback to second shard

        var readPoint = readPoints.Points.Single(p => p.Id == secondShardPoint.Id);

        // manual cast to eliminate cyclic reference
        // Default = {Cyclic reference to type Aer.QdrantClient.Http.Models.Primitives.Vectors.DenseVector detected},

        readPoint.Payload.As<int>().Should().Be(secondShardPoint.Payload.As<int>());
        readPoint
            .Vector.Default.AsDenseVector()
            .VectorValues.Should()
            .BeEquivalentTo(secondShardPoint.Vector.Default.AsDenseVector().VectorValues);
        readPoint.ShardKey.IsInteger().Should().BeTrue();
        readPoint.ShardKey.GetInteger().Should().Be(TestShardKeyInt1);
    }

    [Test]
    public async Task CollectionCreateShardKey_ManualPlacement_TieredMultitenancy()
    {
        OnlyIfVersionAfterOrEqual("1.16.0", "Tiered multitenancy is available only after v1.16");

        var vectorSize = 10U;

        (
            await _qdrantHttpClient.CreateCollection(
                TestCollectionName,
                new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
                {
                    OnDiskPayload = true,
                    ShardNumber = 1,
                    ShardingMethod = ShardingMethod.Custom,
                },
                CancellationToken.None
            )
        ).EnsureSuccess();

        // configure collection manual sharding to ensure consistent results

        var allPeers = (await _qdrantHttpClient.GetClusterInfo(CancellationToken.None)).EnsureSuccess().AllPeerIds;

        var defaultShardKey = TestShardKey1;
        var tenantShardKey = "tenant-1";

        var createDefaultShardKeyResponse = await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            defaultShardKey,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.First()]
        );

        createDefaultShardKeyResponse.Status.IsSuccess.Should().BeTrue();

        var createTenantIndexResponse = await _qdrantHttpClient.CreatePayloadIndex(
            TestCollectionName,
            "text",
            PayloadIndexedFieldType.Keyword,
            CancellationToken.None,
            isTenant: true
        );

        createTenantIndexResponse.Status.IsSuccess.Should().BeTrue();

        UpsertPointsRequest.UpsertPoint pointToUpsert = new(
            id: 1,
            vector: CreateTestVector(vectorSize),
            payload: (TestPayload)"test"
        );

        var upsertToDefaultShardShardResponse = await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest()
            {
                Points = [pointToUpsert],
                ShardKey = ShardSelector.String(shardKeyValue: tenantShardKey, fallbackShardKeyValue: defaultShardKey),
            },
            CancellationToken.None
        );

        upsertToDefaultShardShardResponse.Status.IsSuccess.Should().BeTrue();

        var readPointsWithFallbackToDefaultShard = (
            await _qdrantHttpClient.ScrollPoints(
                TestCollectionName,
                QdrantFilter.Empty,
                PayloadPropertiesSelector.All,
                CancellationToken.None,
                withVector: true,
                limit: 2,
                shardSelector: ShardSelector.String(shardKeyValue: tenantShardKey, fallbackShardKeyValue: defaultShardKey)
            )
        ).EnsureSuccess();

        // Read point from default fallback shard
        var readPoint = readPointsWithFallbackToDefaultShard.Points.Single();
        readPoint.ShardKey.IsString().Should().BeTrue();
        readPoint.ShardKey.GetString().Should().Be(defaultShardKey);

        var createTenantShardKeyResponse = await _qdrantHttpClient.CreateShardKey(
            TestCollectionName,
            tenantShardKey,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: 1,
            placement: [allPeers.Skip(1).First()]
        );

        createTenantShardKeyResponse.Status.IsSuccess.Should().BeTrue();

        var replicatePointsResponse = await _qdrantHttpClient.UpdateCollectionClusteringSetup(
            TestCollectionName,
            UpdateCollectionClusteringSetupRequest.CreateReplicatePointsRequest(
                defaultShardKey,
                tenantShardKey,
                Q.MatchValue("text", "test")
            ),
            CancellationToken.None
        );

        replicatePointsResponse.Status.IsSuccess.Should().BeTrue();

        // Wait for replication to kick in
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        await _qdrantHttpClient.EnsureCollectionReady(
            TestCollectionName,
            CancellationToken.None,
            isCheckShardTransfersCompleted: true
        );

        var collectionClusteringSetup = (
            await _qdrantHttpClient.GetCollectionClusteringInfo(TestCollectionName, CancellationToken.None)
        ).EnsureSuccess();

        collectionClusteringSetup.ShardCount.Should().Be(2);

        readPointsWithFallbackToDefaultShard = (
            await _qdrantHttpClient.ScrollPoints(
                TestCollectionName,
                QdrantFilter.Empty,
                PayloadPropertiesSelector.All,
                CancellationToken.None,
                withVector: true,
                limit: 2,
                shardSelector: ShardSelector.String(shardKeyValue: tenantShardKey, fallbackShardKeyValue: defaultShardKey)
            )
        ).EnsureSuccess();

        // Point should be read from tenant shard
        readPoint = readPointsWithFallbackToDefaultShard.Points.Single();
        readPoint.ShardKey.IsString().Should().BeTrue();
        readPoint.ShardKey.GetString().Should().Be(tenantShardKey);
    }

    [Test]
    public async Task CollectionDeleteShardKey_ManualPlacement()
    {
        (
            await _qdrantHttpClient.CreateCollection(
                TestCollectionName,
                new CreateCollectionRequest(VectorDistanceMetric.Dot, 10U, isServeVectorsFromDisk: true)
                {
                    OnDiskPayload = true,
                    WriteConsistencyFactor = 2,
                    ReplicationFactor = 1,
                    ShardNumber = 2,
                    ShardingMethod = ShardingMethod.Custom,
                },
                CancellationToken.None
            )
        ).EnsureSuccess();

        // configure collection manual sharding to ensure consistent results

        var allPeers = (await _qdrantHttpClient.GetClusterInfo(CancellationToken.None)).EnsureSuccess().AllPeerIds;

        (
            await _qdrantHttpClient.CreateShardKey(
                TestCollectionName,
                TestShardKey1,
                CancellationToken.None,
                shardsNumber: 1,
                replicationFactor: 1,
                placement: [allPeers.First()]
            )
        ).EnsureSuccess();

        (
            await _qdrantHttpClient.CreateShardKey(
                TestCollectionName,
                TestShardKey2,
                CancellationToken.None,
                shardsNumber: 1,
                replicationFactor: 1,
                placement: [allPeers.Skip(1).First()]
            )
        ).EnsureSuccess();

        var deleteShardKeyResult = await _qdrantHttpClient.DeleteShardKey(
            TestCollectionName,
            TestShardKey1,
            CancellationToken.None
        );

        deleteShardKeyResult.Status.IsSuccess.Should().BeTrue();
    }
}
