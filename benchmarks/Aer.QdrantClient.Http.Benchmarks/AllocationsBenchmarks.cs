using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aer.QdrantClient.Http.DependencyInjection;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aer.QdrantClient.Http.Benchmarks;

// For more information on the VS BenchmarkDotNet Diagnosers see https://learn.microsoft.com/visualstudio/profiling/profiling-with-benchmark-dotnet
//[DotNetCountersDiagnoser]
//[DotNetObjectAllocDiagnoser]
[MemoryDiagnoser]
public class AllocationsBenchmarks
{
    private QdrantHttpClient _qdrantClient;

    protected const string TestCollectionName = "test_collection";

    [GlobalSetup(Target = nameof(CollectionCreateUpsertIndex))]
    public void Setup_CollectionCreateUpsertIndex()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton(configuration);

        services.AddQdrantClientFactory();

        services.AddQdrantHttpClient(configuration, registerAsInterface: false);

        var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        _qdrantClient = serviceProvider.GetRequiredService<QdrantHttpClient>();
    }

    [GlobalSetup(Target = nameof(QueryScrollPoints))]
    public void Setup_QueryScrollPoints()
    {
        Setup_CollectionCreateUpsertIndex();

        var (_, upsertPointsByPointIds, _) = QdrantTestsBase
            .PrepareCollection(
                _qdrantClient,
                TestCollectionName,
                vectorCount: 100,
                vectorSize: 256,
                payloadInitializerFunction: (i) =>
                    new TestPayload()
                    {
                        FloatingPointNumber = i,
                        Integer = i,
                        DateTimeValue = DateTime.UtcNow - TimeSpan.FromSeconds(i),
                        Text = i.ToString(),
                    },
                isWaitForCollectionReady: false
            )
            .GetAwaiter()
            .GetResult();

        _qdrantClient
            .CreatePayloadIndex(TestCollectionName, "integer", PayloadIndexedFieldType.Integer, CancellationToken.None)
            .GetAwaiter()
            .GetResult()
            .EnsureSuccess();

        _qdrantClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None).GetAwaiter().GetResult();
    }

    [GlobalSetup(Target = nameof(GetCollectionInfo))]
    public void Setup_GetCollectionInfo()
    {
        Setup_CollectionCreateUpsertIndex();

        _qdrantClient
            .CreateCollection(
                TestCollectionName,
                new CreateCollectionRequest(VectorDistanceMetric.Cosine, 100, isServeVectorsFromDisk: true)
                {
                    OnDiskPayload = true,
                    OptimizersConfig = new OptimizersConfiguration() { IndexingThreshold = 10 },
                },
                CancellationToken.None
            )
            .GetAwaiter()
            .GetResult()
            .EnsureSuccess();

        _qdrantClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None).GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _qdrantClient.DeleteCollection(TestCollectionName, CancellationToken.None).GetAwaiter().GetResult();
    }

    [Benchmark]
    public async Task CollectionCreateUpsertIndex()
    {
        var vectorSize = 100;
        var vectorCount = 200;

        await _qdrantClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Cosine, (ulong)vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true,
                OptimizersConfig = new OptimizersConfiguration() { IndexingThreshold = 10 },
            },
            CancellationToken.None
        );

        List<UpsertPointsRequest.UpsertPoint> pointsToUpsert = new(vectorCount);

        for (int i = 0; i < vectorCount; i++)
        {
            var pointId = PointId.Integer((ulong)i);

            object payload = new TestPayload()
            {
                FloatingPointNumber = i,
                Integer = i,
                DateTimeValue = DateTime.UtcNow - TimeSpan.FromSeconds(i),
                Text = i.ToString(),
            };

            pointsToUpsert.Add(new(pointId, QdrantTestsBase.CreateTestFloat32Vector((uint)vectorSize), payload));
        }

        (
            await _qdrantClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest() { Points = pointsToUpsert },
                CancellationToken.None
            )
        ).EnsureSuccess();

        (
            await _qdrantClient.CreateFullTextPayloadIndex(
                TestCollectionName,
                "text",
                FullTextIndexTokenizerType.Word,
                CancellationToken.None,
                stemmer: FullTextIndexStemmingAlgorithm.CreateSnowball(SnowballStemmerLanguage.English),
                stopwords: FullTextIndexStopwords.CreateDefault(StopwordsLanguage.English)
            )
        ).EnsureSuccess();

        (
            await _qdrantClient.CreatePayloadIndex(
                TestCollectionName,
                "integer",
                PayloadIndexedFieldType.Integer,
                CancellationToken.None
            )
        ).EnsureSuccess();
    }

    [Benchmark]
    public async Task QueryScrollPoints()
    {
        var points = (
            await _qdrantClient.ScrollPoints(
                TestCollectionName,
                Q.Must(
                    Q<TestPayload>.BeInRange(p => p.Integer, greaterThanOrEqual: 0, lessThanOrEqual: 10),
                    !Q<TestPayload>.MatchAny(p => p.Integer, [1, 2, 3])
                ),
                PayloadPropertiesSelector.All,
                CancellationToken.None,
                withVector: true,
                orderBySelector: OrderBySelector.Desc("integer")
            )
        ).EnsureSuccess();

        (
            await _qdrantClient.QueryPoints(
                TestCollectionName,
                new QueryPointsRequest(PointsQuery.CreateOrderByQuery(OrderBySelector.Asc("integer")))
                {
                    Prefetch =
                    [
                        new PrefetchPoints()
                        {
                            Query = new PointsQuery.NearestPointsQuery(points.Points.First().Vector),
                            Limit = 50,
                        },
                    ],
                    WithPayload = true,
                    WithVector = true,
                    Limit = 10,
                },
                CancellationToken.None
            )
        ).EnsureSuccess();
    }

    [Benchmark]
    public async Task GetCollectionInfo()
    {
        var collectionInfoResponse = await _qdrantClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);

        var collectionInfo = collectionInfoResponse.EnsureSuccess();

        GC.KeepAlive(collectionInfo);
    }
}
