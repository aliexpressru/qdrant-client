using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class CollectionParametersTests : QdrantTestsBase
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
    public async Task TestCreateCollection_NamedVectors()
    {
        Dictionary<string, VectorConfigurationBase.SingleVectorConfiguration> namedVectors = new()
        {
            ["Vector_1"] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Dot,
                100,
                isServeVectorsFromDisk: true),
            ["Vector_2"] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Euclid,
                5,
                isServeVectorsFromDisk: false),
            ["Vector_3"] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Cosine,
                50,
                isServeVectorsFromDisk: true),
        };

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(namedVectors)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();

        // check named vector parameters

        var createdCollectionInfo =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        createdCollectionInfo.Status.IsSuccess.Should().BeTrue();
        createdCollectionInfo.Result.Config.Params.Vectors.IsMultipleVectorsConfiguration.Should().BeTrue();

        var multipleVectorsConfiguration = createdCollectionInfo.Result.Config.Params.Vectors.AsMultipleVectorsConfiguration();

        multipleVectorsConfiguration.NamedVectors.Count.Should().Be(namedVectors.Count);

        foreach (var namedVectorConfig in multipleVectorsConfiguration.NamedVectors)
        {
            namedVectors.Should().ContainKey(namedVectorConfig.Key);
            namedVectors[namedVectorConfig.Key].Should().BeEquivalentTo(namedVectorConfig.Value);
        }
    }

    [Test]
    public async Task TestCreateCollection_ServeVectorsFromDisk()
    {
        var createCollectionRequest = new CreateCollectionRequest(
            VectorDistanceMetric.Dot,
            100,
            isServeVectorsFromDisk: true)
        {
            OnDiskPayload = true
        };

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            createCollectionRequest,
            CancellationToken.None);

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();

        // check on disk vector parameters

        var createdCollectionInfo =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        createdCollectionInfo.Result.Config.Params.Vectors.AsSingleVectorConfiguration().OnDisk.Should().BeTrue();
    }

    [Test]
    public async Task TestCreateCollection_ScalarQuantization()
    {
        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                QuantizationConfig = QuantizationConfiguration.Scalar(
                    quantile: 0.9f,
                    isQuantizedVectorAlwaysInRam: true)
            },
            CancellationToken.None);

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();

        // check quantization parameters

        var createdCollectionInfo =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        createdCollectionInfo.Result.Config.QuantizationConfig.Should()
            .BeOfType<QuantizationConfiguration.ScalarQuantizationConfiguration>();

        var quantizationConfig = createdCollectionInfo.Result.Config.QuantizationConfig
            .As<QuantizationConfiguration.ScalarQuantizationConfiguration>();

        quantizationConfig.Quantile.Should().Be(0.9f);
        quantizationConfig.AlwaysRam.Should().BeTrue();
    }

    [Test]
    public async Task TestCreateCollection_BinaryQuantization()
    {
        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                QuantizationConfig = QuantizationConfiguration.Binary(
                    isQuantizedVectorAlwaysInRam: true)
            },
            CancellationToken.None);

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();

        // check quantization parameters

        var createdCollectionInfo =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        createdCollectionInfo.Result.Config.QuantizationConfig.Should()
            .BeOfType<QuantizationConfiguration.BinaryQuantizationConfiguration>();

        var quantizationConfig = createdCollectionInfo.Result.Config.QuantizationConfig
            .As<QuantizationConfiguration.BinaryQuantizationConfiguration>();

        quantizationConfig.AlwaysRam.Should().BeTrue();
    }

    [Test]
    public async Task TestCreateCollection_ProductQuantization()
    {
        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                QuantizationConfig =
                    QuantizationConfiguration.Product(
                        ProductQuantizationCompressionRatio.x8,
                        isQuantizedVectorAlwaysInRam: true)
            },
            CancellationToken.None);

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();

        // check quantization parameters

        var createdCollectionInfo =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        createdCollectionInfo.Result.Config.QuantizationConfig.Should()
            .BeOfType<QuantizationConfiguration.ProductQuantizationConfiguration>();

        var quantizationConfig = createdCollectionInfo.Result.Config.QuantizationConfig
            .As<QuantizationConfiguration.ProductQuantizationConfiguration>();

        quantizationConfig.Compression.Should().Be(ProductQuantizationCompressionRatio.x8);
        quantizationConfig.AlwaysRam.Should().BeTrue();
    }

    [Test]
    public async Task TestCreateCollection_VectorsParametersOverrideCollectionParameters()
    {
        var createCollectionRequest = new CreateCollectionRequest(
            VectorDistanceMetric.Dot,
            100,
            isServeVectorsFromDisk: false)
        {
            OnDiskPayload = true,

            QuantizationConfig = QuantizationConfiguration.Product(
                ProductQuantizationCompressionRatio.x8,
                isQuantizedVectorAlwaysInRam: true),
        };

        var vectorsConfiguration = (VectorConfigurationBase.SingleVectorConfiguration) createCollectionRequest.Vectors;
        vectorsConfiguration.QuantizationConfig = QuantizationConfiguration.Scalar(
            quantile: 0.9f,
            isQuantizedVectorAlwaysInRam: true);

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            createCollectionRequest,
            CancellationToken.None);

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();

        // check collection parameters

        var createdCollectionInfo =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        createdCollectionInfo.Result.Config.QuantizationConfig.Should()
            .BeOfType<QuantizationConfiguration.ProductQuantizationConfiguration>();

        var collectionQuantizationConfig = createdCollectionInfo.Result.Config.QuantizationConfig
            .As<QuantizationConfiguration.ProductQuantizationConfiguration>();

        collectionQuantizationConfig.Compression.Should().Be(ProductQuantizationCompressionRatio.x8);
        collectionQuantizationConfig.AlwaysRam.Should().BeTrue();

        // check Config.Params.Vectors.QuantizationConfig parameters override Config.QuantizationConfig

        createdCollectionInfo.Result.Config.Params.Vectors.AsSingleVectorConfiguration().QuantizationConfig.Should()
            .BeOfType<QuantizationConfiguration.ScalarQuantizationConfiguration>();

        var vectorsQuantizationConfig = createdCollectionInfo.Result.Config.Params.Vectors.AsSingleVectorConfiguration()
            .QuantizationConfig
            .As<QuantizationConfiguration.ScalarQuantizationConfiguration>();

        vectorsQuantizationConfig.Quantile.Should().Be(0.9f);
        vectorsQuantizationConfig.AlwaysRam.Should().BeTrue();
    }
}
