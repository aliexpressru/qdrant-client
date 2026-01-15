using Aer.QdrantClient.Http.Abstractions;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Infrastructure;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests.QdrantClientResolution;

internal class ClusterRoutingTests : QdrantTestsBase
{
    [OneTimeSetUp]
    public void Setup()
    {
        Initialize(isAddMultipleQdrantClients: true);
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
    }

    [Test]
    public async Task UnknownClusterName()
    {
        var httpClientFactory = ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var customFirstHttpClient = httpClientFactory.CreateClient(FirstClientName);
        var customSecondHttpClient = httpClientFactory.CreateClient(SecondClientName);

        var firstClusterName = "Cluster1";
        var secondClusterName = "Cluster2";

        var customQdrantClient = new CustomRoutingQdrantHttpClient(new()
        {
            [TestCollectionName] = customFirstHttpClient,
            [firstClusterName] = customFirstHttpClient,

            [TestCollectionName2] = customSecondHttpClient,
            [secondClusterName] = customSecondHttpClient
        });

        var getInstanceDetailsWithoutClusterNameAct1 = async () => await customQdrantClient.GetInstanceDetails(CancellationToken.None);
        await getInstanceDetailsWithoutClusterNameAct1.Should().ThrowAsync<QdrantClientUninitializedException>();

        var getInstanceDetailsWithoutClusterNameAct2 = async () => await customQdrantClient.GetInstanceDetails(CancellationToken.None, clusterName: "Unknown cluster name");
        await getInstanceDetailsWithoutClusterNameAct2.Should().ThrowAsync<QdrantClientUninitializedException>();
    }

    [Test]
    public async Task CustomClusterRouting()
    {
        // Cleanup
        // Only in this test we are creating collections, so we need to reset storage to make sure it's empty.
        // Otherwise, tests would fail due to existing collections. We need to clean storage in both single-node clusters.
        // That's why we are using the IQdrantClientFactory to create clients for both clusters and reset their storage.
        var qdrantClientFactory = ServiceProvider.GetRequiredService<IQdrantClientFactory>();

        await ResetStorage(qdrantClientFactory.CreateClient(FirstClientName));
        await ResetStorage(qdrantClientFactory.CreateClient(SecondClientName));

        // Arrange

        var httpClientFactory = ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var customFirstHttpClient = httpClientFactory.CreateClient(FirstClientName);
        var customSecondHttpClient = httpClientFactory.CreateClient(SecondClientName);

        var firstClusterName = "Cluster1";
        var secondClusterName = "Cluster2";

        var customQdrantClient = new CustomRoutingQdrantHttpClient(new()
        {
            [TestCollectionName] = customFirstHttpClient,
            [firstClusterName] = customFirstHttpClient,

            [TestCollectionName2] = customSecondHttpClient,
            [secondClusterName] = customSecondHttpClient
        });

        // Sanity check - ensure that the correct HttpClients are returned for each cluster/collection name
        (await customQdrantClient.GetApiClient(firstClusterName)).BaseAddress.Should().Be(customFirstHttpClient.BaseAddress);
        (await customQdrantClient.GetApiClient(secondClusterName)).BaseAddress.Should().Be(customSecondHttpClient.BaseAddress);

        var firstInstanceDetails = await customQdrantClient.GetInstanceDetails(CancellationToken.None, clusterName: firstClusterName);
        var secondInstanceDetails = await customQdrantClient.GetInstanceDetails(CancellationToken.None, clusterName: secondClusterName);

        firstInstanceDetails.Version.Should().NotBeNullOrEmpty();
        secondInstanceDetails.Version.Should().NotBeNullOrEmpty();

        // Create two collections. Each collection should be created in its own cluster (based on the HttpClient mapping provided to the CustomQdrantClient)

        var collectionInFirstClusterCreationResult = await customQdrantClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var collectionInSecondClusterCreationResult = await customQdrantClient.CreateCollection(
            TestCollectionName2,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 200, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionInFirstClusterCreationResult.Status.IsSuccess.Should().BeTrue();
        collectionInSecondClusterCreationResult.Status.IsSuccess.Should().BeTrue();

        // Check collections created with specified configurations

        var firstCollectionInfo =
            await customQdrantClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        firstCollectionInfo.Status.IsSuccess.Should().BeTrue();
        firstCollectionInfo.Result.Config.Params.Vectors.AsSingleVectorConfiguration().Size.Should().Be(100);

        var secondCollectionInfo =
            await customQdrantClient.GetCollectionInfo(TestCollectionName2, CancellationToken.None);

        secondCollectionInfo.Status.IsSuccess.Should().BeTrue();
        secondCollectionInfo.Result.Config.Params.Vectors.AsSingleVectorConfiguration().Size.Should().Be(200);

        // Check each cluster only has its own collection

        var firstClusterAllCollections = await customQdrantClient.ListCollections(CancellationToken.None, clusterName: firstClusterName);
        var secondClusterAllCollections = await customQdrantClient.ListCollections(CancellationToken.None, clusterName: secondClusterName);

        firstClusterAllCollections.Status.IsSuccess.Should().BeTrue();
        firstClusterAllCollections.Result.Collections.Should().ContainSingle(c => c.Name == TestCollectionName);
        firstClusterAllCollections.Result.Collections.Should().NotContain(c => c.Name == TestCollectionName2);

        secondClusterAllCollections.Status.IsSuccess.Should().BeTrue();
        secondClusterAllCollections.Result.Collections.Should().ContainSingle(c => c.Name == TestCollectionName2);
        secondClusterAllCollections.Result.Collections.Should().NotContain(c => c.Name == TestCollectionName);
    }
}
