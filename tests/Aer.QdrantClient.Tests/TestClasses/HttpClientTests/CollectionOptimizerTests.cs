using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using MoreLinq;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal class CollectionOptimizerTests : QdrantTestsBase
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
    public async Task TriggerOptimizers_CollectionDoesNotExist()
    {
        var triggerCollectionOptimizersResult = await _qdrantHttpClient.TriggerOptimizers(
            TestCollectionName,
            CancellationToken.None
        );

        triggerCollectionOptimizersResult.Status.Type.Should().Be(QdrantOperationStatusType.Error);
        triggerCollectionOptimizersResult.Status.IsSuccess.Should().BeFalse();
        triggerCollectionOptimizersResult.Status.Error.Should().Contain(TestCollectionName).And.Contain("doesn't exist");

        triggerCollectionOptimizersResult.Result.Should().BeNull();
    }

    [Test]
    public async Task TriggerOptimizers()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                OptimizersConfig = new OptimizersConfiguration() { MaxOptimizationThreads = 1, IndexingThreshold = 1 },
                HnswConfig = new HnswConfiguration() { MaxIndexingThreads = 1 },
            },
            CancellationToken.None
        );

        var triggerCollectionOptimizersResult = await _qdrantHttpClient.TriggerOptimizers(
            TestCollectionName,
            CancellationToken.None
        );

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

    [Test]
    public async Task GetOptimizerStatus_EmptyCollection_NoAdditionalProperties()
    {
        OnlyIfVersionAfterOrEqual("1.17.0", "Optimiser status API is introduced in v1.17");

        (
            await _qdrantHttpClient.CreateCollection(
                TestCollectionName,
                new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
                {
                    OnDiskPayload = true,
                    OptimizersConfig = new OptimizersConfiguration() { MaxOptimizationThreads = 1, IndexingThreshold = 1 },
                    HnswConfig = new HnswConfiguration() { MaxIndexingThreads = 1 },
                },
                CancellationToken.None
            )
        ).EnsureSuccess();

        var collectionOptimizationProgressResponse = await _qdrantHttpClient.GetCollectionOptimizationProgress(
            TestCollectionName,
            CancellationToken.None,
            OptimizationProgressOptionalInfoFields.None,
            completedLimit: 10
        );

        collectionOptimizationProgressResponse.Status.IsSuccess.Should().BeTrue();

        var progress = collectionOptimizationProgressResponse.Result;

        progress.Summary.QueuedSegments.Should().Be(0);
        progress.Summary.QueuedOptimizations.Should().Be(0);
        progress.Summary.QueuedPoints.Should().Be(0);

        // Empty collection will contain only idle segments
        progress.Summary.IdleSegments.Should().NotBe(0);

        // This property is always present
        progress.Running.Length.Should().Be(0);

        // None of the following properties were requested
        progress.Queued.Should().BeNull();
        progress.Completed.Should().BeNull();
        progress.IdleSegments.Should().BeNull();
    }

    [Test]
    public async Task GetOptimizerStatus_EmptyCollection()
    {
        OnlyIfVersionAfterOrEqual("1.17.0", "Optimiser status API is introduced in v1.17");

        (
            await _qdrantHttpClient.CreateCollection(
                TestCollectionName,
                new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
                {
                    OnDiskPayload = true,
                    OptimizersConfig = new OptimizersConfiguration() { MaxOptimizationThreads = 1, IndexingThreshold = 1 },
                    HnswConfig = new HnswConfiguration() { MaxIndexingThreads = 1 },
                },
                CancellationToken.None
            )
        ).EnsureSuccess();

        var collectionOptimizationProgressResponse = await _qdrantHttpClient.GetCollectionOptimizationProgress(
            TestCollectionName,
            CancellationToken.None,
            OptimizationProgressOptionalInfoFields.All,
            completedLimit: 10
        );

        collectionOptimizationProgressResponse.Status.IsSuccess.Should().BeTrue();

        var progress = collectionOptimizationProgressResponse.Result;

        progress.Summary.QueuedSegments.Should().Be(0);
        progress.Summary.QueuedOptimizations.Should().Be(0);
        progress.Summary.QueuedPoints.Should().Be(0);
        // Empty collection will contain only idle segments
        progress.Summary.IdleSegments.Should().NotBe(0);

        progress.Queued.Length.Should().Be(0);
        progress.Completed.Length.Should().Be(0);
        progress.Running.Length.Should().Be(0);

        // Empty collection will contain only idle segments
        progress.IdleSegments.Length.Should().NotBe(0);
    }

    [Test]
    public async Task GetOptimizerStatus()
    {
        OnlyIfVersionAfterOrEqual("1.17.0", "Optimiser status API is introduced in v1.17");

        await PrepareCollection(_qdrantHttpClient, TestCollectionName, vectorCount: 100_000, isWaitForCollectionReady: false);

        // Since we are not waiting for collection to become green
        // We get collection optimisation status while optimizer is still running

        var runningCollectionOptimizationProgressResponse = await _qdrantHttpClient.GetCollectionOptimizationProgress(
            TestCollectionName,
            CancellationToken.None,
            OptimizationProgressOptionalInfoFields.All,
            completedLimit: 10
        );

        runningCollectionOptimizationProgressResponse.Status.IsSuccess.Should().BeTrue();

        var progress = runningCollectionOptimizationProgressResponse.Result;

        // For some reason these are 0

        progress.Summary.QueuedSegments.Should().Be(0);
        progress.Summary.QueuedOptimizations.Should().Be(0);
        progress.Summary.QueuedPoints.Should().Be(0);
        // Even filled in collection contains idle segment with 0 points
        progress.Summary.IdleSegments.Should().NotBe(0);

        // Non empty collection will contain idle segments as well
        progress.IdleSegments.Length.Should().NotBe(0);

        progress.Queued.Length.Should().Be(0);
        progress.Completed.Length.Should().Be(0);

        progress.Running.Length.Should().NotBe(0);

        var runningOptimisation = progress.Running.First();

        runningOptimisation.Status.Should().Be(GetCollectionOptimizationProgressResponse.CollectionOptimizationProgress.TrackedOptimizerStatus.Optimizing);
        runningOptimisation.Optimizer.Should().NotBeNullOrEmpty();
        runningOptimisation.Uuid.Should().NotBeNullOrEmpty();
        runningOptimisation.Segments.Length.Should().BeGreaterThan(0);

        var firstSegment = runningOptimisation.Segments.First();
        firstSegment.Uuid.Should().NotBeNullOrEmpty();
        firstSegment.PointsCount.Should().BeGreaterThan(0);

        var runningOptimisationProgress = runningOptimisation.Progress;

        runningOptimisationProgress.Name.Should().NotBeNullOrEmpty();
        runningOptimisationProgress.Children.Length.Should().BeGreaterThan(0);

        runningOptimisationProgress.StartedAt.Should().NotBeNull();
        runningOptimisationProgress.FinishedAt.Should().BeNull();
        runningOptimisationProgress.Done.Should().BeNull();
        runningOptimisationProgress.DurationSec.Should().BeNull();
        runningOptimisationProgress.Total.Should().BeNull();

        // Now wait for optimisations to finish and check again

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var finishedCollectionOptimizationProgressResponse = await _qdrantHttpClient.GetCollectionOptimizationProgress(
            TestCollectionName,
            CancellationToken.None,
            OptimizationProgressOptionalInfoFields.All,
            completedLimit: 10
        );

        finishedCollectionOptimizationProgressResponse.Status.IsSuccess.Should().BeTrue(finishedCollectionOptimizationProgressResponse.Status.GetErrorMessage());

        progress = finishedCollectionOptimizationProgressResponse.Result;

        progress.Running.Length.Should().Be(0, $"Running optimizations {progress.Running.Length}");
        progress.Completed.Length.Should().NotBe(0, $"Completed optimizations {progress.Completed.Length}");

        var completedOrRunningOptimisation = progress.Completed.Length > 0
            ? progress.Completed.First()
            : progress.Running.First();

        completedOrRunningOptimisation.Uuid.Should().NotBeNullOrEmpty();
        completedOrRunningOptimisation.Optimizer.Should().NotBeNullOrEmpty();
        completedOrRunningOptimisation.Segments.Length.Should().BeGreaterThan(0);
        completedOrRunningOptimisation.Status.Should().BeOneOf(GetCollectionOptimizationProgressResponse.CollectionOptimizationProgress.TrackedOptimizerStatus.Done,
            GetCollectionOptimizationProgressResponse.CollectionOptimizationProgress.TrackedOptimizerStatus.Optimizing);

        var optimisationProgress = completedOrRunningOptimisation.Progress;

        optimisationProgress.Should().NotBeNull();

        optimisationProgress.Children.Length.Should().BeGreaterThan(0);

        // Looks like done and total are not applicable for root progress
        optimisationProgress.Done.Should().BeNull();
        optimisationProgress.Total.Should().BeNull();

        optimisationProgress.StartedAt.Should().NotBeNull();

        if (completedOrRunningOptimisation.Status is GetCollectionOptimizationProgressResponse.CollectionOptimizationProgress.TrackedOptimizerStatus.Done)
        {
            // These fields only apply to finished optimisations
            optimisationProgress.DurationSec.Should().NotBeNull();
            optimisationProgress.FinishedAt.Should().NotBeNull();

            // Find vector index child

            var mainGraphStage = optimisationProgress.Children
                .Single(c => c.Name == "vector_index")
                .Children.First() // First child element of the vector_index operation seems to be a grouping element.
                                  // We need to drill down further
                .Children
                .Single(c => c.Name == "main_graph");

            // Both Total and Done for main graph equal to the number of points

            mainGraphStage.Total.Should().NotBeNull();
            mainGraphStage.Done.Should().NotBeNull();
        }
    }
}
