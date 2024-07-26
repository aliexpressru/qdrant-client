using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

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
    [TestCase(VectorDataType.Float32)]
    [TestCase(VectorDataType.Uint8)]
    [TestCase(VectorDataType.Float16)]
    public async Task TestCreateCollection_CheckParameters(VectorDataType vectorDataType)
    {
        uint vectorSize = 10U;

        var createCollectionRequest = new CreateCollectionRequest(
            new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true,
                vectorDataType: vectorDataType
            )
        );

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            createCollectionRequest,
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        // upsert points

        var testPointId = PointId.NewGuid();
        var testVector = CreateTestVector(vectorSize, vectorDataType);
        TestPayload testPayload = "test";

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
                    {
                        new(testPointId, testVector, testPayload)
                    }
                },
                CancellationToken.None);

        upsertPointsResult.EnsureSuccess();

        // check parameters

        var createdCollectionInfoResponse =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        createdCollectionInfoResponse.EnsureSuccess();

        var collectionInfo = createdCollectionInfoResponse.Result;

        // todo: check other collection parameters

        collectionInfo.Config.Params.Vectors.AsSingleVectorConfiguration().Datatype.Should().Be(vectorDataType);
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
            ["Vector_4"] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Manhattan,
                50,
                isServeVectorsFromDisk: true,
                multivectorConfiguration: new MultivectorConfiguration(MultivectorComparator.MaxSim))
        };

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(namedVectors)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

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
    // this ugly string as second test case argument is a workaround for NUnit analyzer that
    // can't for some reason parse several enum values like this
    // [TestCase(VectorDataType.Float32, SparseVectorModifier.Idf)]
    // it parses preceding attribute as having 0 values
    [TestCase(VectorDataType.Float32, nameof(SparseVectorModifier.Idf))]
    [TestCase(VectorDataType.Uint8, null)]
    [TestCase(VectorDataType.Float16, nameof(SparseVectorModifier.None))]
    public async Task TestCreateCollection_NamedVectors_SparseVectors(
        VectorDataType vectorDataType,
        string sparseVectorModifierString)
    {
        SparseVectorModifier? sparseVectorModifier =
            sparseVectorModifierString is null
                ? null
                : Enum.Parse<SparseVectorModifier>(sparseVectorModifierString, ignoreCase: true);

        var sparseVectorName = "Vector_1";

        Dictionary<string, VectorConfigurationBase.SingleVectorConfiguration> namedVectors = new()
        {
            [sparseVectorName] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Dot,
                100,
                isServeVectorsFromDisk: true,
                vectorDataType: vectorDataType),
            ["Vector_2"] = new VectorConfigurationBase.SingleVectorConfiguration(
                VectorDistanceMetric.Euclid,
                5,
                isServeVectorsFromDisk: false,
                vectorDataType: vectorDataType),
        };

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(namedVectors)
            {
                OnDiskPayload = true,
                SparseVectors = new Dictionary<string, SparseVectorConfiguration>()
                {
                    [sparseVectorName] = sparseVectorModifierString is not null
                        ? new(
                            onDisk: true,
                            fullScanThreshold: 5000,
                            vectorDataType: vectorDataType,
                            sparseVectorModifier.Value)
                        : new(
                            onDisk: true,
                            fullScanThreshold: 5000,
                            vectorDataType: vectorDataType)
                }
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        // check sparse vectors config

        var createdCollectionInfo =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        createdCollectionInfo.Status.IsSuccess.Should().BeTrue();
        createdCollectionInfo.Result.Config.Params.Vectors.IsMultipleVectorsConfiguration.Should().BeTrue();

        var sparseVectorsConfiguration = createdCollectionInfo.Result.Config.Params.SparseVectors;

        sparseVectorsConfiguration.Should().NotBeNull();
        sparseVectorsConfiguration.Count.Should().Be(1);
        sparseVectorsConfiguration.Should().ContainKey(sparseVectorName);

        sparseVectorsConfiguration[sparseVectorName].OnDisk.Should().BeTrue();
        sparseVectorsConfiguration[sparseVectorName].FullScanThreshold.Should().NotBeNull();
        sparseVectorsConfiguration[sparseVectorName].FullScanThreshold.Should().Be(5000);
        sparseVectorsConfiguration[sparseVectorName].VectorDataType.Should().Be(vectorDataType);

        if (sparseVectorModifierString is null)
        {
            sparseVectorsConfiguration[sparseVectorName].Modifier.Should().Be(SparseVectorModifier.None);
        }
        else
        {
            sparseVectorsConfiguration[sparseVectorName].Modifier.Should().Be(sparseVectorModifier);
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

        collectionCreationResult.EnsureSuccess();

        // check on disk vector parameters

        var createdCollectionInfo =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        createdCollectionInfo.Result.Config.Params.Vectors.AsSingleVectorConfiguration().OnDisk.Should().BeTrue();
    }

    [Test]
    public async Task TestCreateCollection_CustomShardingMethod()
    {
        var createCollectionRequest = new CreateCollectionRequest(
            VectorDistanceMetric.Dot,
            100,
            isServeVectorsFromDisk: true)
        {
            OnDiskPayload = true,
            ShardingMethod = ShardingMethod.Custom
        };

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            createCollectionRequest,
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        // check on custom sharding parameter

        var createdCollectionInfo =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        createdCollectionInfo.Result.Config.Params.ShardingMethod.Should()
            .Be(ShardingMethod.Custom);
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

        collectionCreationResult.EnsureSuccess();

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

        collectionCreationResult.EnsureSuccess();

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

        collectionCreationResult.EnsureSuccess();

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

        collectionCreationResult.EnsureSuccess();

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
