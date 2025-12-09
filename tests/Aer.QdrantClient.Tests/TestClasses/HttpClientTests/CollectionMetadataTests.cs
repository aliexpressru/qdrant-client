using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal class CollectionMetadataTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;

    [OneTimeSetUp]
    public void Setup()
    {
        OnlyIfVersionAfterOrEqual("1.16.0", "Collection metadata is only supported from v1.16");

        Initialize();

        _qdrantHttpClient = ServiceProvider.GetRequiredService<QdrantHttpClient>();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage(_qdrantHttpClient);
    }

    [Test]
    public async Task CollectionWithNoMetadata()
    {
        (await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
            },
            CancellationToken.None)
        ).EnsureSuccess();

        var collectionInfo =
            (await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        collectionInfo.Config.Metadata.Should().NotBeNull();
        collectionInfo.Config.Metadata.Keys.Count.Should().Be(0);
        collectionInfo.GetMetadata().Keys.Count.Should().Be(0);

        collectionInfo.ContainsMetadataKey("some_key").Should().BeFalse();
        collectionInfo.GetMetadataValueOrDefault("some_other_key", 42).Should().Be(42);
    }

    [Test]
    public async Task UpdateCollectionMetadata()
    {
        var metadata = new Dictionary<string, object>
        {
            ["test_string"] = "test",
            ["test_int"] = 1,
        };

        (await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                Metadata = metadata
            },
            CancellationToken.None)
        ).EnsureSuccess();

        var initialCollectionInfo =
            (await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        initialCollectionInfo.Config.Metadata.Should().NotBeNull();

        initialCollectionInfo.Config.Metadata.Count.Should().Be(2);

        initialCollectionInfo.GetMetadata().Keys.Should().BeEquivalentTo(metadata.Keys);

        AssertMetadataValue(initialCollectionInfo, "test_string", "test");
        AssertMetadataValue(initialCollectionInfo, "test_int", 1);

        var updateCollectionParametersResponse = await _qdrantHttpClient.UpdateCollectionParameters(TestCollectionName, new()
        {
            Metadata = new Dictionary<string, object>
            {
                ["test_string"] = "updated",
                ["test_int"] = 42,
            }
        }, CancellationToken.None);

        updateCollectionParametersResponse.Status.IsSuccess.Should().BeTrue();

        var updatedCollectionInfo =
            (await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        updatedCollectionInfo.Config.Metadata.Should().NotBeNull();

        updatedCollectionInfo.Config.Metadata.Count.Should().Be(2);
        updatedCollectionInfo.GetMetadata().Count.Should().Be(2);

        updatedCollectionInfo.GetMetadata().Keys.Should().BeEquivalentTo(metadata.Keys);

        AssertMetadataValue(updatedCollectionInfo, "test_string", "updated");
        AssertMetadataValue(updatedCollectionInfo, "test_int", 42);
    }

    [Test]
    public async Task DeleteCollectionMetadata()
    {
        var metadata = new Dictionary<string, object>
        {
            ["test_string"] = "test",
            ["test_int"] = 1,
        };

        (await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                Metadata = metadata
            },
            CancellationToken.None)
        ).EnsureSuccess();

        var initialCollectionInfo =
            (await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None))
            .EnsureSuccess();

        initialCollectionInfo.Config.Metadata.Should().NotBeNull();
        initialCollectionInfo.GetMetadata().Count.Should().Be(2);
        initialCollectionInfo.Config.Metadata.Count.Should().Be(2);

        // Delete first key

        var updateCollectionParametersResponse1 = await _qdrantHttpClient.UpdateCollectionParameters(TestCollectionName, new()
        {
            Metadata = new Dictionary<string, object>
            {
                ["test_string"] = null,
            }
        }, CancellationToken.None);

        updateCollectionParametersResponse1.Status.IsSuccess.Should().BeTrue();

        var updatedCollectionInfo =
           (await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None))
           .EnsureSuccess();

        updatedCollectionInfo.Config.Metadata.Should().NotBeNull();
        updatedCollectionInfo.GetMetadata().Count.Should().Be(1);
        updatedCollectionInfo.Config.Metadata.Count.Should().Be(1);

        updatedCollectionInfo.GetMetadata().Keys.Should().BeEquivalentTo(["test_int"]); // Only one key left

        updatedCollectionInfo.Config.Metadata.ContainsKey("test_string").Should().BeFalse();
        updatedCollectionInfo.ContainsMetadataKey("test_string").Should().BeFalse();
        updatedCollectionInfo.ContainsMetadataKey("test_int").Should().BeTrue();

        // Delete second key

        var updateCollectionParametersResponse2 = await _qdrantHttpClient.UpdateCollectionParameters(TestCollectionName, new()
        {
            Metadata = new Dictionary<string, object>
            {
                ["test_int"] = null,
            }
        }, CancellationToken.None);

        updateCollectionParametersResponse2.Status.IsSuccess.Should().BeTrue();

        var finalCollectionInfo =
           (await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None))
           .EnsureSuccess();

        finalCollectionInfo.Config.Metadata.Should().NotBeNull();

        finalCollectionInfo.GetMetadata().Count.Should().Be(0);
        finalCollectionInfo.Config.Metadata.Count.Should().Be(0);

        finalCollectionInfo.GetMetadata().Keys.Should().BeEmpty();

        finalCollectionInfo.ContainsMetadataKey("test_string").Should().BeFalse();
        finalCollectionInfo.ContainsMetadataKey("test_int").Should().BeFalse();
    }

    private static void AssertMetadataValue<T>(
        GetCollectionInfoResponse.CollectionInfo collectionInfo,
        string key,
        T expectedValue)
    {
        collectionInfo.Config.Metadata.ContainsKey(key).Should().BeTrue();
        collectionInfo.ContainsMetadataKey(key).Should().BeTrue();

        collectionInfo.Config.Metadata.GetValueOrDefault<T>(key).Should().Be(expectedValue);
        collectionInfo.GetMetadataValueOrDefault<T>(key).Should().Be(expectedValue);
    }
}
