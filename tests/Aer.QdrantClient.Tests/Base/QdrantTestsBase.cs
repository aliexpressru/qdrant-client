using System.Reflection;
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

    protected const string TestCollectionName = "test_collection";
    protected const string TestCollectionAlias = "test_collection_alias";
    protected const string TestCollectionAlias2 = "test_collection_alias_2";

    protected const string TestCollectionName2 = "test_collection_2";

    protected const string TestPayloadFieldName = "test_payload_field";
    protected const string TestPayloadFieldName2 = "test_payload_field_2";

    private static string _collectionsDataDirectoryToClear;
    private static string _snapshotsDataDirectoryToClear;
    private const string ROOT_SOLUTION_DIRECTORY_NAME = "qdrant-client";
    private const string QDRANT_COLLECTIONS_DATA_DIRECTORY_RELATIVE_PATH = "volumes/0/qdrant_storage/collections";
    private const string QDRANT_SNAPSHOTS_DATA_DIRECTORY_RELATIVE_PATH = "volumes/0/qdrant_snapshots";


    protected void Initialize(bool isDisableAuthorization = false)
    {
        Environment.SetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT",
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"))
                ? "Local"
                : "Testing");

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

        // gRPC client is not implemented yet
        //services.AddQdrantGrpcClient(Configuration);

        ServiceProvider = services.BuildServiceProvider();
    }

    protected async Task ResetStorage()
    {
        bool wasException = true;
        while (wasException)
        {
            try
            {
                await DeleteCollectionsAndSnapshots();

                if (!IsCiEnvironment)
                {
                    // drop collection files since on local machine they sometimes
                    // get left after the collection deletion preventing new collection
                    // with the same name from being created

                    if (_collectionsDataDirectoryToClear is null)
                    {
                        var currentPath = Assembly.GetExecutingAssembly().Location;
                        var currentDirectory = new DirectoryInfo(currentPath);

                        while (currentDirectory.Name != ROOT_SOLUTION_DIRECTORY_NAME)
                        {
                            currentDirectory = currentDirectory.Parent ?? throw new InvalidOperationException(
                                $"The directory traverse for collection files deletion is in incorrect state. "
                                + $"No directory '{ROOT_SOLUTION_DIRECTORY_NAME}' found along the traverse. Starting directory was '{currentPath}'");
                        }

                        // we are in "qdrant-client" dir which is the root of the project
                        // now descend into 'volumes' directory and find 'qdrant_storage' dir

                        // get collections data directory

                        var qdrantCollectionsDataDirectoryPath = Path.Combine(
                            currentDirectory.FullName,
                            QDRANT_COLLECTIONS_DATA_DIRECTORY_RELATIVE_PATH);

                        if (Directory.Exists(qdrantCollectionsDataDirectoryPath))
                        {
                            // cache directory path to avoid multiple traversings
                            _collectionsDataDirectoryToClear = qdrantCollectionsDataDirectoryPath;
                        }
                        else
                        {
                            TestContext.Write($"The expected qdrant collection data directory '{qdrantCollectionsDataDirectoryPath}' does not exist check the path");
                            break;
                        }

                        // get snapshots data directory

                        var qdrantSnapshotsDataDirectoryPath = Path.Combine(
                            currentDirectory.FullName,
                            QDRANT_SNAPSHOTS_DATA_DIRECTORY_RELATIVE_PATH);

                        if (Directory.Exists(qdrantSnapshotsDataDirectoryPath))
                        {
                            // cache directory path to avoid multiple traversings
                            _snapshotsDataDirectoryToClear = qdrantSnapshotsDataDirectoryPath;
                        }
                        else
                        {
                            TestContext.Write(
                                $"The expected qdrant snapshots data directory '{qdrantSnapshotsDataDirectoryPath}' does not exist check the path");
                            break;
                        }
                    }

                    // delete collections data

                    var qdrantCollectionsDataDirectory = new DirectoryInfo(_collectionsDataDirectoryToClear);

                    foreach (var collectionDataDirectory in qdrantCollectionsDataDirectory.EnumerateDirectories())
                    {
                        try
                        {
                            Directory.Delete(collectionDataDirectory.FullName, recursive: true);
                        }
                        catch(Exception e)
                        {
                            TestContext.Write($"Can't delete collection data directory '{collectionDataDirectory.FullName}' : {e}");
                            // ignore
                        }
                    }

                    // delete snapshots data

                    var qdrantSnapshotsDataDirectory = new DirectoryInfo(_snapshotsDataDirectoryToClear);
                    foreach (var collectionSnapshotDirectory in qdrantSnapshotsDataDirectory.EnumerateDirectories())
                    {
                        if (collectionSnapshotDirectory.Name.Equals("tmp"))
                        {
                            // skip deletting "tmp" directory as is is created upon qdrant start
                            // and it won't be recreated automatically which will corrupt snapshot APIs
                            continue;
                        }

                        try
                        {
                            Directory.Delete(collectionSnapshotDirectory.FullName, recursive: true);
                        }
                        catch (Exception e)
                        {
                            TestContext.Write($"Can't delete collection snapshot directory '{collectionSnapshotDirectory.FullName}' : {e}");
                            // ignore
                        }
                    }
                }

                wasException = false;
            }
            catch(Exception e)
            {
                TestContext.Write($"Exception happened during collection deletion {e}");
                // ignore and retry
            }
        }

        if (wasException)
        {
            throw new Exception("Qdrant collections reset failed. Check tests logs.");
        }

        await Task.Delay(TimeSpan.FromMilliseconds(500));
    }

    private async Task DeleteCollectionsAndSnapshots()
    {
        var qdrantHttpClient = ServiceProvider.GetRequiredService<QdrantHttpClient>();

        await qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);
        await qdrantHttpClient.DeleteCollection(TestCollectionName2, CancellationToken.None);

        await qdrantHttpClient.DeleteAllCollectionSnapshots(TestCollectionName, CancellationToken.None);
        await qdrantHttpClient.DeleteAllCollectionSnapshots(TestCollectionName2, CancellationToken.None);
    }

    protected float[] CreateTestVector(uint vectorLength)
        =>
            Enumerable.Range(0, (int)vectorLength)
                .Select(_ => float.CreateTruncating(Random.Shared.NextDouble()))
                .ToArray();

    protected VectorBase CreateTestNamedVectors(uint vectorLength, int namedVectorsCount)
    {
        Dictionary<string, float[]> namedVectors = new(namedVectorsCount);

        foreach (var vectorName in GetVectorNames(namedVectorsCount))
        {
            var vector = Enumerable.Range(0, (int) vectorLength)
                .Select(_ => float.CreateTruncating(Random.Shared.NextDouble()))
                .ToArray();

            namedVectors.Add(vectorName, vector);
        }

        return namedVectors;
    }

    protected List<string> GetVectorNames(int vectorCount)
    {
        List<string> ret = new(vectorCount);
        for (int i = 0; i < vectorCount; i++)
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

    internal async Task<(IReadOnlyList<UpsertPointsRequest<TestPayload>.UpsertPoint> UpsertPoints,
        Dictionary<ulong, UpsertPointsRequest<TestPayload>.UpsertPoint> UpsertPointsByPointIds,
        IReadOnlyList<PointId> UpsertPointIds)> PrepareCollection(
        QdrantHttpClient qdrantHttpClient,
        string collectionName,
        VectorDistanceMetric distanceMetric = VectorDistanceMetric.Dot,
        uint vectorSize = 10U,
        int vectorCount = 10,
        Func<int, TestPayload> payloadInitializerFunction = null,
        QuantizationConfiguration quantizationConfig = null)
    {
        await qdrantHttpClient.CreateCollection(
            collectionName,
            new CreateCollectionRequest(distanceMetric, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                QuantizationConfig = quantizationConfig
            },
            CancellationToken.None);

        Func<int, TestPayload> payloadInitializer = payloadInitializerFunction ?? (i => i);

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
        var upsertPointIds = new List<PointId>();

        for (int i = 0; i < vectorCount; i++)
        {
            var pointId = PointId.Integer((ulong) i);

            upsertPoints.Add(
                new(
                    pointId,
                    CreateTestVector(vectorSize),
                    payloadInitializer(i)
                )
            );

            upsertPointIds.Add(pointId);
        }

        Dictionary<ulong, UpsertPointsRequest<TestPayload>.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId) p.Id).Id);

        var upsertPointsResult = await qdrantHttpClient.UpsertPoints(
            collectionName,
            new UpsertPointsRequest<TestPayload>()
            {
                Points = upsertPoints
            },
            CancellationToken.None);

        upsertPointsResult.EnsureSuccess();

        await qdrantHttpClient.EnsureCollectionReady(collectionName, CancellationToken.None);

        return (upsertPoints, upsertPointsByPointIds, upsertPointIds);
    }

    private void AddTestLogger(ServiceCollection services)
    {
        LogEventLevel minimumEventLevel = IsCiEnvironment
            ? LogEventLevel.Information
            : LogEventLevel.Verbose;

        Serilog.ILogger logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Is(minimumEventLevel)
            .CreateLogger();

        IMicrosoftLoggerAlias microsoftLogger = new SerilogLoggerFactory(logger)
            .CreateLogger<QdrantTestsBase>();

        LogLevel minimumMsLoggerLevel = IsCiEnvironment
            ? LogLevel.Information
            : LogLevel.Trace;

        services.AddLogging(
            builder => builder.ClearProviders()
                .AddSerilog(logger)
                .SetMinimumLevel(minimumMsLoggerLevel)
        );

        services.AddSingleton(microsoftLogger);
    }
}
