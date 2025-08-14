using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class CollectionTriggerOptimizersTests : QdrantTestsBase
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
