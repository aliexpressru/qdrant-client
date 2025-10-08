using System.Collections.Concurrent;
using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Infrastructure;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class CollectionUpdateParametersTests : QdrantTestsBase
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
    public async Task CollectionDoesNotExist()
    {
        var updateNoCollectionParametersResult = await _qdrantHttpClient.UpdateCollectionParameters(
            TestCollectionName,
            new UpdateCollectionParametersRequest()
            {
                OptimizersConfig = new()
                {
                    MaxSegmentSize = 10
                }
            },
            CancellationToken.None);

        updateNoCollectionParametersResult.Status.Type.Should().Be(QdrantOperationStatusType.Error);
        updateNoCollectionParametersResult.Status.IsSuccess.Should().BeFalse();
        updateNoCollectionParametersResult.Status.Error.Should()
            .Contain(TestCollectionName).And
            .Contain("doesn't exist");

        updateNoCollectionParametersResult.Result.Should().BeNull();
    }

    [Test]
    public async Task EmptyRequest()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                OptimizersConfig = new OptimizersConfiguration()
                {
                    MaxOptimizationThreads = 1,
                    IndexingThreshold = 1
                },
                HnswConfig = new HnswConfiguration()
                {
                    MaxIndexingThreads = 1
                }
            },
            CancellationToken.None);

        var triggerCollectionOptimizersResult = await _qdrantHttpClient.UpdateCollectionParameters(
            TestCollectionName,
            UpdateCollectionParametersRequest.Empty,
            CancellationToken.None);

        var updatedCollectionInfo =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        triggerCollectionOptimizersResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        triggerCollectionOptimizersResult.Status.IsSuccess.Should().BeTrue();

        triggerCollectionOptimizersResult.Result.Should().NotBeNull();

        updatedCollectionInfo.Result.Status.Should().Be(QdrantCollectionStatus.Green);
        updatedCollectionInfo.Result.OptimizerStatus.IsOk.Should().BeTrue();

        // Collection parameters should not change

        updatedCollectionInfo.Result.Config.OptimizerConfig.IndexingThreshold.Should().Be(1);
        updatedCollectionInfo.Result.Config.OptimizerConfig.MaxOptimizationThreads.Should().Be(1);
        updatedCollectionInfo.Result.Config.HnswConfig.MaxIndexingThreads.Should().Be(1);
    }

    [Test]
    [Obsolete("Testing obsolete functionality")]
    public async Task UpdateCollectionParameters_Before_1_16_0()
    {
        OnlyIfVersionBefore("1.16.0", "mmap_threshold parameter of the collection optimizer is deprecated and going to be removed in v1.16");

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = false,
                OptimizersConfig = new OptimizersConfiguration()
                {
                    MemmapThreshold = 1000,
                    MaxOptimizationThreads = 1,
                    IndexingThreshold = 1
                },
                HnswConfig = new HnswConfiguration(){
                    MaxIndexingThreads = 1
                }
            },
            CancellationToken.None);

        const int newMemmapThreshold = 1;
        const int newMaxOptimizationThreads = 10;
        const int newMaxIndexingThreads = 11;

        var updateCollectionParametersResult = await _qdrantHttpClient.UpdateCollectionParameters(
            TestCollectionName,
            new UpdateCollectionParametersRequest()
            {
                Params = new(){
                    OnDiskPayload = true
                },
                OptimizersConfig = new()
                {
                    MemmapThreshold = newMemmapThreshold,
                    MaxOptimizationThreads = newMaxOptimizationThreads
                },
                HnswConfig = new HnswConfiguration(){
                    MaxIndexingThreads = newMaxIndexingThreads
                }
            },
            CancellationToken.None);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var updatedCollectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        updateCollectionParametersResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        updateCollectionParametersResult.Status.IsSuccess.Should().BeTrue();

        updateCollectionParametersResult.Result.Should().NotBeNull();

        updatedCollectionInfo.Result.Status.Should().Be(QdrantCollectionStatus.Green);
        updatedCollectionInfo.Result.OptimizerStatus.IsOk.Should().BeTrue();

        updatedCollectionInfo.Result.Config.OptimizerConfig.IndexingThreshold.Should().Be(1); // should not change

        updatedCollectionInfo.Result.Config.OptimizerConfig.MemmapThreshold.Should().Be(newMemmapThreshold);
        updatedCollectionInfo.Result.Config.OptimizerConfig.MaxOptimizationThreads.Should().Be(newMaxOptimizationThreads);
        updatedCollectionInfo.Result.Config.HnswConfig.MaxIndexingThreads.Should().Be(newMaxIndexingThreads);
        updatedCollectionInfo.Result.Config.Params.OnDiskPayload.Should().BeTrue();
    }

    [Test]
    public async Task UpdateCollectionParameters()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = false,
                OptimizersConfig = new OptimizersConfiguration()
                {
                    MaxOptimizationThreads = 1,
                    IndexingThreshold = 1
                },
                HnswConfig = new HnswConfiguration(){
                    MaxIndexingThreads = 1
                }
            },
            CancellationToken.None);

        const int newMaxOptimizationThreads = 10;
        const int newMaxIndexingThreads = 11;

        var updateCollectionParametersResult = await _qdrantHttpClient.UpdateCollectionParameters(
            TestCollectionName,
            new UpdateCollectionParametersRequest()
            {
                Params = new(){
                    OnDiskPayload = true
                },
                OptimizersConfig = new()
                {
                    MaxOptimizationThreads = newMaxOptimizationThreads
                },
                HnswConfig = new HnswConfiguration(){
                    MaxIndexingThreads = newMaxIndexingThreads
                },
                StrictModeConfig = new StrictModeConfiguration(){
                    Enabled = true,
                    MaxPointsCount = 1000
                }
            },
            CancellationToken.None);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var updatedCollectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        updateCollectionParametersResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        updateCollectionParametersResult.Status.IsSuccess.Should().BeTrue();

        updateCollectionParametersResult.Result.Should().NotBeNull();

        updatedCollectionInfo.Result.Status.Should().Be(QdrantCollectionStatus.Green);
        updatedCollectionInfo.Result.OptimizerStatus.IsOk.Should().BeTrue();

        updatedCollectionInfo.Result.Config.OptimizerConfig.IndexingThreshold.Should().Be(1); // should not change

        updatedCollectionInfo.Result.Config.OptimizerConfig.MaxOptimizationThreads.Should().Be(newMaxOptimizationThreads);
        updatedCollectionInfo.Result.Config.HnswConfig.MaxIndexingThreads.Should().Be(newMaxIndexingThreads);
        updatedCollectionInfo.Result.Config.Params.OnDiskPayload.Should().BeTrue();

        updatedCollectionInfo.Result.Config.StrictModeConfig.Enabled.Should().BeTrue();
        updatedCollectionInfo.Result.Config.StrictModeConfig.MaxPointsCount.Should().Be(1000);
    }

    [Test]
    public async Task WithRetry()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = false,
                OptimizersConfig = new OptimizersConfiguration()
                {
                    MaxOptimizationThreads = 1,
                    IndexingThreshold = 1
                },
                HnswConfig = new HnswConfiguration()
                {
                    MaxIndexingThreads = 1
                }
            },
            CancellationToken.None);

        const int newOptimizationThreads = 10;

        var throwingQdrantHttpClient = new ThrowingQdrantHttpClient(_qdrantHttpClient.ApiClient);

        throwingQdrantHttpClient.ThrowOnce();
        throwingQdrantHttpClient.BadRequestOnce();

        int retryCount = 0;
        ConcurrentBag<Exception> exceptions = new();

        var updateCollectionParametersResult = await throwingQdrantHttpClient.UpdateCollectionParameters(
            TestCollectionName,
            new UpdateCollectionParametersRequest()
            {
                OptimizersConfig = new()
                {
                    MaxOptimizationThreads = newOptimizationThreads,
                }
            },
            CancellationToken.None,
            retryCount: 3,
            retryDelay: TimeSpan.FromMilliseconds(10),
            onRetry: (ex, _, _, _) => {
                Interlocked.Increment(ref retryCount);
                exceptions.Add(ex);
            }
        );

        retryCount.Should().Be(2);
        exceptions.Count.Should().Be(2);

        // 1 retry exception because when retrying bad request we don't actually have any real exception but a special one.
        exceptions.Count(e => e is QdrantRequestRetryException).Should().Be(1);
        exceptions.Count(e => e is HttpRequestException).Should().Be(1); // 1 exception for requested request failure

        updateCollectionParametersResult.Status.IsSuccess.Should().BeTrue();
        updateCollectionParametersResult.Result.Should().NotBeNull();

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var updatedCollectionInfo =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        // Just check that thew request to update collection parameters actually worked
        updatedCollectionInfo.Result.Config.OptimizerConfig.MaxOptimizationThreads.Should().Be(newOptimizationThreads);
    }

    [Test]
    public async Task TriggerCollectionOptimizers_CollectionDoesNotExist()
    {
        var triggerCollectionOptimizersResult = await _qdrantHttpClient.TriggerOptimizers(
            TestCollectionName,
            CancellationToken.None);

        triggerCollectionOptimizersResult.Status.Type.Should().Be(QdrantOperationStatusType.Error);
        triggerCollectionOptimizersResult.Status.IsSuccess.Should().BeFalse();
        triggerCollectionOptimizersResult.Status.Error.Should()
            .Contain(TestCollectionName).And
            .Contain("doesn't exist");

        triggerCollectionOptimizersResult.Result.Should().BeNull();
    }

    [Test]
    public async Task TriggerCollectionOptimizers()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                OptimizersConfig = new OptimizersConfiguration()
                {
                    MaxOptimizationThreads = 1,
                    IndexingThreshold = 1
                },
                HnswConfig = new HnswConfiguration(){
                    MaxIndexingThreads = 1
                }
            },
            CancellationToken.None);

        var triggerCollectionOptimizersResult = await _qdrantHttpClient.TriggerOptimizers(
            TestCollectionName,
            CancellationToken.None);

        var updatedCollectionInfo = await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        triggerCollectionOptimizersResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        triggerCollectionOptimizersResult.Status.IsSuccess.Should().BeTrue();

        triggerCollectionOptimizersResult.Result.Should().NotBeNull();

        updatedCollectionInfo.Result.Status.Should().Be(QdrantCollectionStatus.Green);
        updatedCollectionInfo.Result.OptimizerStatus.IsOk.Should().BeTrue();

        // Collection parameters should not change

        updatedCollectionInfo.Result.Config.OptimizerConfig.IndexingThreshold.Should().Be(1);
        updatedCollectionInfo.Result.Config.OptimizerConfig.MaxOptimizationThreads.Should().Be(1);
        updatedCollectionInfo.Result.Config.HnswConfig.MaxIndexingThreads.Should().Be(1);
    }
}
