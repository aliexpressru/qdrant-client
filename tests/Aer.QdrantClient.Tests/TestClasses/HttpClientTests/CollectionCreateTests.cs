using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class CollectionCreateTests : QdrantTestsBase
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
    public async Task CreateCollection()
    {
        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();
    }

    [Test]
    public async Task VeryLongName()
    {
        var veryLongCollectionName = new string('t', 1024);

        var collectionCreationAct = () => _qdrantHttpClient.CreateCollection(
            veryLongCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        await collectionCreationAct.Should().ThrowAsync<QdrantInvalidEntityNameException>()
            .Where(e => e.Message.Contains("1024"));
    }
    
    [Test]
    [TestCase(VectorDataType.Float32)]
    [TestCase(VectorDataType.Uint8)]
    [TestCase(VectorDataType.Float16)]
    public async Task CheckParameters(VectorDataType vectorDataType)
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

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
                    {
                        new(
                            PointId.NewGuid(),
                            CreateTestVector(vectorSize, vectorDataType),
                            "test")
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

        var singleVectorConfig = collectionInfo.Config.Params.Vectors.AsSingleVectorConfiguration();

        singleVectorConfig.Datatype.Should().Be(vectorDataType);
        singleVectorConfig.DistanceMetric.Should().Be(VectorDistanceMetric.Dot);
    }
    
    [Test]
    public async Task NamedVectors()
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
    public async Task SameNamedVectors()
    {
        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                100,
                isServeVectorsFromDisk: true,
                CreateVectorNames(3))
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();
    }

    [Test]
    public async Task SparseVector()
    {
        var sparseVectorName = "test";

        var sparseVectorsConfiguration = new Dictionary<string, SparseVectorConfiguration>()
        {
            [sparseVectorName] = new(
                onDisk: true,
                fullScanThreshold: 5000,
                vectorDataType: VectorDataType.Float32)
        };
        
        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(sparseVectorsConfiguration: sparseVectorsConfiguration){
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();

        var createdCollectionInfo =
            (await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        createdCollectionInfo.Config.Params.Vectors.Should().BeNull();
        
        createdCollectionInfo.Config.Params.SparseVectors.Should().ContainKey(sparseVectorName);

        var sparseVectorConfig = createdCollectionInfo.Config.Params.SparseVectors[sparseVectorName];

        sparseVectorConfig.OnDisk.Should().BeTrue();
        sparseVectorConfig.FullScanThreshold.Should().Be(5000);
        sparseVectorConfig.VectorDataType.Should().Be(VectorDataType.Float32);
        
        sparseVectorConfig.Modifier.Should().Be(SparseVectorModifier.None);

        sparseVectorConfig.Index.Should().NotBeNull();

        sparseVectorConfig.Index.OnDisk.Should().BeTrue();
        sparseVectorConfig.Index.FullScanThreshold.Should().Be(5000);
        sparseVectorConfig.Index.VectorDataType.Should().Be(VectorDataType.Float32);
    }

    [Test]
    [TestCase(VectorDataType.Float32)]
    [TestCase(VectorDataType.Uint8)]
    [TestCase(VectorDataType.Float16)]
    public async Task NamedVectors_WithUpsertPoints_CheckParameters(VectorDataType vectorDataType)
    {
        uint vectorSize = 10U;

        var createCollectionRequest = new CreateCollectionRequest(
            new Dictionary<string, VectorConfigurationBase.SingleVectorConfiguration>()
            {
                ["Vector_1"] = new(
                    VectorDistanceMetric.Dot,
                    vectorSize,
                    isServeVectorsFromDisk: true,
                    vectorDataType: vectorDataType,
                    multivectorConfiguration: new MultivectorConfiguration(MultivectorComparator.MaxSim)
                )
            },
            new Dictionary<string, SparseVectorConfiguration>()
            {
                [VectorBase.DefaultVectorName] = new(
                    vectorDataType: vectorDataType,
                    onDisk: true,
                    fullScanThreshold: 100,
                    sparseVectorValueModifier: SparseVectorModifier.Idf)
            }
        );

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            createCollectionRequest,
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        // upsert points

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
                    {
                        new(
                            PointId.NewGuid(),
                            new NamedVectors()
                            {
                                Vectors = new Dictionary<string, VectorBase>()
                                {
                                    [VectorBase.DefaultVectorName] = CreateTestSparseVector(vectorSize, 2, vectorDataType),
                                    ["Vector_1"] = CreateTestVector(vectorSize, vectorDataType)
                                }
                            },
                            "test")
                    }
                },
                CancellationToken.None);

        upsertPointsResult.EnsureSuccess();

        // check parameters

        var createdCollectionInfoResponse =
            await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        createdCollectionInfoResponse.EnsureSuccess();

        var collectionInfo = createdCollectionInfoResponse.Result;

        collectionInfo.Config.Params.SparseVectors.Should().ContainKey(VectorBase.DefaultVectorName);
        collectionInfo.Config.Params.SparseVectors[VectorBase.DefaultVectorName].VectorDataType.Should().Be(vectorDataType);
        collectionInfo.Config.Params.SparseVectors[VectorBase.DefaultVectorName].OnDisk.Should().Be(true);
        collectionInfo.Config.Params.SparseVectors[VectorBase.DefaultVectorName].FullScanThreshold.Should().Be(100);
        collectionInfo.Config.Params.SparseVectors[VectorBase.DefaultVectorName].Modifier.Should().Be(SparseVectorModifier.Idf);

        var multipleVectorsConfiguration =
            collectionInfo.Config.Params.Vectors.AsMultipleVectorsConfiguration();

        multipleVectorsConfiguration.NamedVectors.Count.Should().Be(1);
        multipleVectorsConfiguration.NamedVectors["Vector_1"].MultivectorConfig.Should().NotBeNull();
        multipleVectorsConfiguration.NamedVectors["Vector_1"].MultivectorConfig.Comparator.Should()
            .Be(MultivectorComparator.MaxSim);
    }
    
    [Test]
    // this ugly string as second test case argument is a workaround for NUnit analyzer that
    // can't for some reason parse several enum values like this
    // [TestCase(VectorDataType.Float32, SparseVectorModifier.Idf)]
    // it parses preceding attribute as having 0 values
    [TestCase(VectorDataType.Float32, nameof(SparseVectorModifier.Idf))]
    [TestCase(VectorDataType.Uint8, null)]
    [TestCase(VectorDataType.Float16, nameof(SparseVectorModifier.None))]
    public async Task NamedVectors_SparseVectors(
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
    public async Task ServeVectorsFromDisk()
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
    public async Task CustomShardingMethod()
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
    public async Task ScalarQuantization()
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

        quantizationConfig.Method.Should().Be(QuantizationConfiguration.ScalarQuantizationConfiguration.QuantizationMethodName);
        quantizationConfig.Quantile.Should().Be(0.9f);
        quantizationConfig.AlwaysRam.Should().BeTrue();
    }

    [Test]
    public async Task BinaryQuantization_Before_1_15()
    { 
        OnlyIfVersionBefore("1.15.0", "Binary encoding and query encoding is not supported before 1.15.0");

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                QuantizationConfig = QuantizationConfiguration.Binary(
                    isQuantizedVectorAlwaysInRam: true
                )
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

        quantizationConfig.Method.Should()
            .Be(QuantizationConfiguration.BinaryQuantizationConfiguration.QuantizationMethodName);

        quantizationConfig.AlwaysRam.Should().BeTrue();
    }

    [Test]
    public async Task BinaryQuantization()
    {
        OnlyIfVersionAfterOrEqual("1.15.0", "Binary encoding and query encoding is not supported before 1.15.0");
        
        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                QuantizationConfig = QuantizationConfiguration.Binary(
                    isQuantizedVectorAlwaysInRam: true,
                    encoding: BinaryQuantizationEncoding.TwoBits,
                    queryEncoding: BinaryQuantizationQueryEncoding.Scalar4bits)
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

        quantizationConfig.Method.Should()
            .Be(QuantizationConfiguration.BinaryQuantizationConfiguration.QuantizationMethodName);
        
        quantizationConfig.AlwaysRam.Should().BeTrue();
        
        quantizationConfig.Encoding.Should().NotBeNull();
        quantizationConfig.Encoding.Should().Be(BinaryQuantizationEncoding.TwoBits);
        
        quantizationConfig.QueryEncoding.Should().NotBeNull();
        quantizationConfig.QueryEncoding.Should().Be(BinaryQuantizationQueryEncoding.Scalar4bits);
    }

    [Test]
    public async Task ProductQuantization()
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

        quantizationConfig.Method.Should()
            .Be(QuantizationConfiguration.ProductQuantizationConfiguration.QuantizationMethodName);
        quantizationConfig.Compression.Should().Be(ProductQuantizationCompressionRatio.x8);
        quantizationConfig.AlwaysRam.Should().BeTrue();
    }

    [Test]
    public async Task VectorsParametersOverrideCollectionParameters()
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

    [Test]
    public async Task StrictMode()
    {
        OnlyIfVersionAfterOrEqual("1.13.0", "Strict mode is not supported before 1.13.0");
        
        var strictModeConfig = new StrictModeConfiguration
        {
            Enabled = true,
            MaxQueryLimit = 1000,
            MaxTimeout = 5000,
            UnindexedFilteringRetrieve = true,
            UnindexedFilteringUpdate = false,
            SearchMaxHnswEf = 200,
            SearchAllowExact = true,
            SearchMaxOversampling = 2.3,
            UpsertMaxBatchsize = 10,
            MaxCollectionVectorSizeBytes = 100,
            ReadRateLimit = 10,
            WriteRateLimit = 10,
            MaxCollectionPayloadSizeBytes = 100,
            MaxPointsCount = 10,
            FilterMaxConditions = 3,
            ConditionMaxSize = 2, // This setting will cause an error upon query
            MultivectorConfig = new(){
                ["Vector1"] = new(){
                    MaxVectors = 10
                }
            },
            SparseConfig =new(){
                ["Vector2"] = new(){
                    MaxLength = 1000
                }
            }
        };
        
        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                StrictModeConfig = strictModeConfig
            },
            CancellationToken.None);

        collectionCreationResult.Status.Type.Should().Be(QdrantOperationStatusType.Ok);
        collectionCreationResult.Status.IsSuccess.Should().BeTrue();

        collectionCreationResult.Should().NotBeNull();
        collectionCreationResult.Result.Should().BeTrue();

        var createdCollectionInfo =
            (await _qdrantHttpClient.GetCollectionInfo(TestCollectionName, CancellationToken.None)).EnsureSuccess();

        createdCollectionInfo.Config.StrictModeConfig.Should().BeEquivalentTo(strictModeConfig);
        
        // Try to insert more points than allowed by MaxPointsCount

        var upsertPointsToNonExistentCollectionResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
                    {
                        new(
                            PointId.Integer(1),
                            CreateTestVector(10),
                            "test1"
                        ),

                        new(
                            PointId.Integer(2),
                            CreateTestVector(10),
                            "test2"
                        ),

                        new(
                            PointId.Integer(3),
                            CreateTestVector(10),
                            "test3"
                        )
                    }
                },
                CancellationToken.None);

        upsertPointsToNonExistentCollectionResult.EnsureSuccess();
    }
}
