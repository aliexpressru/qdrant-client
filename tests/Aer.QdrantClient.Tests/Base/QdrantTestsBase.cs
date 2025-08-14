using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.DependencyInjection;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using IMicrosoftLoggerAlias = Microsoft.Extensions.Logging.ILogger;

namespace Aer.QdrantClient.Tests.Base;

public class QdrantTestsBase
{
    protected bool IsCiEnvironment;
    protected IConfiguration Configuration;
    protected IServiceProvider ServiceProvider;

    protected Version QdrantVersion { get; private set; }

    protected const string TestCollectionName = "test_collection";
    protected const string TestCollectionAlias = "test_collection_alias";
    protected const string TestCollectionAlias2 = "test_collection_alias_2";

    protected const string TestShardKey1 = "shard1";
    protected const string TestShardKey2 = "shard2";
    protected const int TestShardKeyInt1 = 1;

    protected const string TestCollectionName2 = "test_collection_2";

    protected const string TestPayloadFieldName = "test_payload_field";
    protected const string TestPayloadFieldName2 = "test_payload_field_2";
    protected const string TestPayloadFieldName3 = "test_payload_field_3";

    // shared random with constant seed to make tests repeatable
    protected static readonly Random Random = new(1567);

    protected void Initialize(bool isDisableAuthorization = false)
    {
        Environment.SetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT",
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"))
                ? "Local"
                : "Testing");

        var qdrantVersion = Environment.GetEnvironmentVariable("QDRANT_VERSION") ?? GetQdrantVersionFromEnvFile();

        QdrantVersion = Version.Parse(qdrantVersion);

        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
            throw new InvalidOperationException("ASPNETCORE_ENVIRONMENT in not set");

        IsCiEnvironment = environmentName == "Testing";

        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json")
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton(Configuration);

        AddTestLogger(services);

        // add qdrant clients
        if (isDisableAuthorization)
        {
            services.AddQdrantHttpClient(
                Configuration,
                configureOptions: config => config.ApiKey = null);
        }
        else
        {
            services.AddQdrantHttpClient(Configuration);
        }

        ServiceProvider = services.BuildServiceProvider();
    }
    
    private string GetQdrantVersionFromEnvFile()
    {
        var envFilePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            ".env");

        var envFileLines = File.ReadAllLines(envFilePath);

        string foundVersion = null;
        
        foreach (var fileLine in envFileLines)
        {
            if (fileLine.StartsWith("#"))
            { 
                continue;
            }

            if (fileLine.StartsWith("QDRANT_VERSION"))
            { 
                foundVersion = fileLine.Split("=v")[1];
            }
        }

        if (foundVersion == null)
        { 
            throw new InvalidOperationException(
                "QDRANT_VERSION is not set in .env file. Please set it to the desired version.");
        }
        
        return foundVersion;
    }

    protected async Task ResetStorage(QdrantHttpClient qdrantClient = null)
    {
        bool wasException = true;
        while (wasException)
        {
            try
            {
                await DeleteCollectionsAndSnapshots(qdrantClient);

                wasException = false;
            }
            catch (Exception e)
            {
                await TestContext.Out.WriteAsync($"Exception happened during collection deletion {e}");
                // ignore and retry
            }
        }

        if (wasException)
        {
            throw new Exception("Qdrant collections reset failed. Check tests logs.");
        }

        await Task.Delay(TimeSpan.FromMilliseconds(500));
    }

    private async Task DeleteCollectionsAndSnapshots(QdrantHttpClient qdrantClient = null)
    {
        var qdrantHttpClient = qdrantClient ?? ServiceProvider.GetRequiredService<QdrantHttpClient>();

        await qdrantHttpClient.SetLockOptions(areWritesDisabled: false, "", CancellationToken.None);

        await qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);
        await qdrantHttpClient.DeleteCollection(TestCollectionName2, CancellationToken.None);

        await qdrantHttpClient.DeleteAllCollectionSnapshots(CancellationToken.None);
    }

    protected float[] CreateTestVector(
        uint vectorLength,
        VectorDataType vectorDataType = VectorDataType.Float32)
    {
        return vectorDataType switch
        {
            VectorDataType.Float32 => CreateTestFloat32Vector(vectorLength),
            VectorDataType.Float16 => CreateTestFloat16Vector(vectorLength),
            VectorDataType.Uint8 => CreateTestByteVector(vectorLength),
            _ => throw new ArgumentOutOfRangeException(nameof(vectorDataType), vectorDataType, null)
        };
    }

    protected (uint[] Indices, float[] Values) CreateTestSparseVector(
        uint vectorLength,
        uint numberOfNonZeroIndices,
        VectorDataType vectorDataType = VectorDataType.Float32)
    {
        var values = CreateTestVector(numberOfNonZeroIndices, vectorDataType);

        var indices = Enumerable.Range(0, (int) vectorLength)
            .Select(_ => (uint) Random.Next((int) vectorLength + 1))
            .Distinct()
            .Take((int) numberOfNonZeroIndices)
            .OrderBy(v => v)
            .ToArray();

        return (indices, values);
    }

    protected float[][] CreateTestMultivector(
        uint vectorLength,
        uint componentVectorCount,
        VectorDataType vectorDataType)
    {
        var ret = new float[componentVectorCount][];

        for (int i = 0; i < componentVectorCount; i++)
        {
            ret[i] = CreateTestVector(vectorLength, vectorDataType);
        }

        return ret;
    }

    private float[] CreateTestFloat32Vector(uint vectorLength)
        =>
            Enumerable.Range(0, (int) vectorLength)
#if NET7_0_OR_GREATER
                .Select(_ => float.CreateTruncating(Random.NextDouble()))
#else
                .Select(_ => Random.NextSingle())
#endif
                .ToArray();

    private float[] CreateTestFloat16Vector(uint vectorLength)
        =>
            Enumerable.Range(0, (int) vectorLength)
#if NET7_0_OR_GREATER
                .Select(_ => (float) Half.CreateTruncating(Random.NextDouble()))
#else
                .Select(_ => (float) ((Half) Random.NextSingle()))
#endif
                .ToArray();

    private float[] CreateTestByteVector(uint vectorLength)
        =>
            Enumerable.Range(0, (int) vectorLength)
#if NET7_0_OR_GREATER
                .Select(_ => (float) byte.CreateTruncating(Random.Next()))
#else
                .Select(_ => (float) unchecked((byte) Random.Next()))
#endif
                .ToArray();

    protected VectorBase CreateTestNamedVectors(uint vectorLength, int namedVectorsCount)
    {
        Dictionary<string, float[]> namedVectors = new(namedVectorsCount);

        foreach (var vectorName in CreateVectorNames(namedVectorsCount))
        {
            var vector = CreateTestVector(vectorLength);
            namedVectors.Add(vectorName, vector);
        }

        return namedVectors;
    }

    protected List<string> CreateVectorNames(int vectorCount, bool addDefaultVector = false)
    {
        List<string> ret = new(vectorCount);

        if (addDefaultVector)
        {
            ret.Add(VectorBase.DefaultVectorName);
        }

        for (int i = 0;
             i < (addDefaultVector
                 ? vectorCount - 1
                 : vectorCount);
             i++)
        {
            ret.Add($"Vector_{i}");
        }

        return ret;
    }

    protected float[] CreateConstantTestVector(float vectorElement, uint vectorLength)
        =>
            Enumerable.Range(0, (int) vectorLength)
                .Select(_ => vectorElement)
                .ToArray();

    /// <summary>
    /// Returns <see cref="QdrantHttpClient"/> for 2-node cluster.
    /// </summary>
    protected QdrantHttpClient GetClusterClient() =>
        new("localhost", apiKey: "test", port: 6343, useHttps: false);

    internal async Task<
            (IReadOnlyList<UpsertPointsRequest<TPayload>.UpsertPoint> UpsertPoints,
            Dictionary<ulong, UpsertPointsRequest<TPayload>.UpsertPoint> UpsertPointsByPointIds,
            IReadOnlyList<PointId> UpsertPointIds)
        >
        PrepareCollection<TPayload>(
            QdrantHttpClient qdrantHttpClient,
            string collectionName,
            VectorDistanceMetric distanceMetric = VectorDistanceMetric.Dot,
            uint vectorSize = 10U,
            int vectorCount = 10,
            Func<int, TPayload> payloadInitializerFunction = null,
            QuantizationConfiguration quantizationConfig = null)
        where TPayload : Payload, new()
    {
        await qdrantHttpClient.CreateCollection(
            collectionName,
            new CreateCollectionRequest(distanceMetric, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                QuantizationConfig = quantizationConfig
            },
            CancellationToken.None);

        var upsertPoints = new List<UpsertPointsRequest<TPayload>.UpsertPoint>();
        var upsertPointIds = new List<PointId>();

        for (int i = 0; i < vectorCount; i++)
        {
            var pointId = PointId.Integer((ulong) i);

            Payload payload = payloadInitializerFunction is null
                ? new TestPayload() {Integer = i}
                : payloadInitializerFunction(i);

            upsertPoints.Add(
                new(
                    pointId,
                    CreateTestFloat32Vector(vectorSize),
                    (TPayload) payload
                )
            );

            upsertPointIds.Add(pointId);
        }

        Dictionary<ulong, UpsertPointsRequest<TPayload>.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId) p.Id).Id);

        var upsertPointsResult = await qdrantHttpClient.UpsertPoints(
            collectionName,
            new UpsertPointsRequest<TPayload>()
            {
                Points = upsertPoints
            },
            CancellationToken.None);

        upsertPointsResult.EnsureSuccess();

        await qdrantHttpClient.EnsureCollectionReady(collectionName, CancellationToken.None);

        return (upsertPoints, upsertPointsByPointIds, upsertPointIds);
    }

    protected async Task CreateSmallTestShardedCollection(
        QdrantHttpClient qdrantHttpClient,
        string collectionName,
        uint vectorSize,
        uint replicationFactor = 1)
    {
        (await qdrantHttpClient.CreateCollection(
            collectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                WriteConsistencyFactor = 2,
                ReplicationFactor = replicationFactor,
                ShardNumber = 2,
                ShardingMethod = ShardingMethod.Custom
            },
            CancellationToken.None)).EnsureSuccess();

        // configure collection manual sharding to ensure consistent results

        var allPeers = (await qdrantHttpClient.GetClusterInfo(CancellationToken.None))
            .EnsureSuccess().AllPeerIds;

        (await qdrantHttpClient.CreateShardKey(
            collectionName,
            TestShardKey1,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: replicationFactor,
            // with manual shard placement we need to manually specify all replica peers,
            // since qdrant allows having collection replication factor of 2 while having only one peer
            // for a specific shard. Thus, we need to manually tell it both primary peer and a replica peer
            placement: replicationFactor == 1
                ? [allPeers.First()]
                : [..allPeers])).EnsureSuccess();

        (await qdrantHttpClient.CreateShardKey(
            collectionName,
            TestShardKey2,
            CancellationToken.None,
            shardsNumber: 1,
            replicationFactor: replicationFactor,
            placement: replicationFactor == 1
                ? [allPeers.Skip(1).First()]
                : [..allPeers])).EnsureSuccess();

        (await qdrantHttpClient.UpsertPoints(
            collectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
                {
                    new(PointId.NewGuid(), CreateTestFloat32Vector(vectorSize), "test"),
                },
                ShardKey = TestShardKey1
            },
            CancellationToken.None)).EnsureSuccess();

        (await qdrantHttpClient.UpsertPoints(
            collectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>()
                {
                    new(PointId.NewGuid(), CreateTestFloat32Vector(vectorSize), "test2"),
                },
                ShardKey = TestShardKey2
            },
            CancellationToken.None)).EnsureSuccess();

        await qdrantHttpClient.EnsureCollectionReady(collectionName, cancellationToken: CancellationToken.None);

        await Task.Delay(TimeSpan.FromSeconds(1));
    }

    private void AddTestLogger(ServiceCollection services)
    {
        LogEventLevel minimumEventLevel = IsCiEnvironment
            ? LogEventLevel.Warning
            : LogEventLevel.Verbose;

        Serilog.ILogger logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Is(minimumEventLevel)
            .CreateLogger();

        IMicrosoftLoggerAlias microsoftLogger = new SerilogLoggerFactory(logger)
            .CreateLogger<QdrantTestsBase>();

        LogLevel minimumMsLoggerLevel = IsCiEnvironment
            ? LogLevel.Warning
            : LogLevel.Trace;

        services.AddLogging(builder => builder.ClearProviders()
            .AddSerilog(logger)
            .SetMinimumLevel(minimumMsLoggerLevel)
        );

        services.AddSingleton(microsoftLogger);
    }

    protected void OnlyIfVersionAfterOrEqual(Version versionInclusive, string reason)
    {
        if (QdrantVersion >= versionInclusive)
        {
            return;
        }

        Assert.Ignore(
            $"Test ignored because Qdrant version {QdrantVersion} is lower than required {versionInclusive}. Reason: {reason}");
    }

    protected void OnlyIfVersionBefore(Version versionExclusive, string reason)
    {
        if (QdrantVersion < versionExclusive)
        {
            return;
        }

        Assert.Ignore(
            $"Test ignored because Qdrant version {QdrantVersion} is higher than required {versionExclusive}. Reason: {reason}");
    }
}
