using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal class CollectionListTests : QdrantTestsBase
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
    public async Task GetCollectionInfo_NoCollection()
    {
        var nonExistentCollectionInfo =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        nonExistentCollectionInfo.Status.Type.Should().Be(QdrantOperationStatusType.Error);
        nonExistentCollectionInfo.Status.IsSuccess.Should().BeFalse();
        nonExistentCollectionInfo.Status.Error.Should()
            .Contain("doesn't exist").And
            .Contain(TestCollectionName);

        nonExistentCollectionInfo.Result.Should().BeNull();
    }

    [Test]
    public async Task EmptyCollectionList()
    {
        var existingCollections = await _qdrantHttpClient.ListCollections(CancellationToken.None);

        existingCollections.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        existingCollections.Status.IsSuccess.Should().BeTrue();

        existingCollections.Result.Should().NotBeNull();
        existingCollections.Result.Collections.Should().BeEmpty();
    }

    [Test]
    public async Task ListCollections()
    {
        // create collection
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var oneExistingCollection = await _qdrantHttpClient.ListCollections(CancellationToken.None);

        oneExistingCollection.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        oneExistingCollection.Status.IsSuccess.Should().BeTrue();

        oneExistingCollection.Result.Should().NotBeNull();
        oneExistingCollection.Result.Collections.Length.Should().Be(1);
        oneExistingCollection.Result.Collections[0].Name.Should().Be(TestCollectionName);

        // create second collection
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName2,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var twoExistingCollections = await _qdrantHttpClient.ListCollections(CancellationToken.None);

        twoExistingCollections.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        twoExistingCollections.Status.IsSuccess.Should().BeTrue();

        twoExistingCollections.Result.Should().NotBeNull();
        twoExistingCollections.Result.Collections.Length.Should().Be(2);

        twoExistingCollections.Result.Collections
            .SingleOrDefault(c => c.Name.Equals(TestCollectionName)).Should().NotBeNull();
        twoExistingCollections.Result.Collections
            .SingleOrDefault(c => c.Name.Equals(TestCollectionName2)).Should().NotBeNull();
    }
}
