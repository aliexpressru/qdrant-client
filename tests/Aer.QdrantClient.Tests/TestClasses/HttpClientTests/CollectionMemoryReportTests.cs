using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;
using static Aer.QdrantClient.Http.Models.Responses.CollectionMemoryReportResponse;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal class CollectionMemoryReportTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;

    [OneTimeSetUp]
    public void Setup()
    {
        OnlyIfVersionAfterOrEqual("1.18.0", "Collection memory report is only supported from v1.18");

        Initialize();

        _qdrantHttpClient = ServiceProvider.GetRequiredService<QdrantHttpClient>();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage(_qdrantHttpClient);
    }

    [Test]
    public async Task CollectionMemoryReport()
    {
        const uint vectorSize = 100U;
        const int pointCount = 1000;

        const string denseVectorName = "Dense";
        const string sparseVectorName = "Sparse";

        // "integer" and "text" are the SnakeCaseLower-serialized names of TestPayload.Integer and TestPayload.Text
        const string integerPayloadFieldName = "integer";
        const string textPayloadFieldName = "text";

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                new Dictionary<string, VectorConfigurationBase.SingleVectorConfiguration>()
                {
                    [denseVectorName] = new(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: false),
                }
            )
            {
                HnswConfig = new() { FullScanThreshold = 10 },
                OnDiskPayload = false,
                SparseVectors = new Dictionary<string, SparseVectorConfiguration>()
                {
                    [sparseVectorName] = new(onDisk: false, fullScanThreshold: 5000),
                },
                OptimizersConfig = new OptimizersConfiguration { IndexingThreshold = 10 },
            },
            CancellationToken.None
        );

        collectionCreationResult.EnsureSuccess();

        (
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                integerPayloadFieldName,
                PayloadIndexedFieldType.Integer,
                CancellationToken.None,
                isWaitForResult: true
            )
        ).EnsureSuccess();

        (
            await _qdrantHttpClient.CreatePayloadIndex(
                TestCollectionName,
                textPayloadFieldName,
                PayloadIndexedFieldType.Keyword,
                CancellationToken.None,
                isWaitForResult: true
            )
        ).EnsureSuccess();

        var upsertPoints = Enumerable
            .Range(0, pointCount)
            .Select(i => new UpsertPointsRequest.UpsertPoint(
                PointId.Integer((ulong)i),
                new NamedVectors()
                {
                    Vectors = new Dictionary<string, VectorBase>()
                    {
                        [denseVectorName] = CreateTestVector(vectorSize),
                        [sparseVectorName] = CreateTestSparseVector(vectorSize, 10),
                    },
                },
                new TestPayload { Integer = i, Text = $"text_{i}" }
            ))
            .ToList();

        (
            await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest() { Points = upsertPoints },
                CancellationToken.None
            )
        ).EnsureSuccess();

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        // Read every point with vector and payload from disk to ensure that all that should be hoisted to RAM - hoisted there

        (
            await _qdrantHttpClient.ScrollPoints(
                TestCollectionName,
                QdrantFilter.Empty,
                withPayload: true,
                withVector: true,
                limit: 10000,
                cancellationToken: CancellationToken.None
            )
        ).EnsureSuccess();

        var memoryReportResponse = await _qdrantHttpClient.GetCollectionMemoryReport(TestCollectionName, CancellationToken.None);

        memoryReportResponse.Status.IsSuccess.Should().BeTrue();

        var memoryReport = memoryReportResponse.Result;

        // Check total memory usage

        AssertMemoryUsage(memoryReport.Total);

        // Check dense vector memory usage

        memoryReport.Vectors.Should().HaveCount(1);

        foreach (var denseVectorReport in memoryReport.Vectors)
        {
            AssertMemoryUsage(denseVectorReport.Storage, allowRamZero: true, allowExpectedCacheZero: true);

            // For some reason dense vectors are not hoisted to memory even if asked for
            AssertMemoryUsage(denseVectorReport.Index, allowRamZero: true, allowExpectedCacheZero: true);
        }

        // Check sparse vector memory usage

        memoryReport.SparseVectors.Should().HaveCount(1);

        foreach (var sparseVectorReport in memoryReport.SparseVectors)
        {
            AssertMemoryUsage(sparseVectorReport.Storage, allowRamZero: true, allowExpectedCacheZero: true);
            AssertMemoryUsage(sparseVectorReport.Index, allowRamZero: false, allowExpectedCacheZero: true);
        }

        // Check total payload memory usage

        AssertMemoryUsage(memoryReport.Payload, allowRamZero: true, allowExpectedCacheZero: true);

        // Check payload index memory usage

        memoryReport.PayloadIndex.Should().HaveCount(2);

        foreach (var payloadIndexReport in memoryReport.PayloadIndex)
        {
            AssertMemoryUsage(payloadIndexReport.Usage, allowExpectedCacheZero: true);
        }

        // Check other memory usage

        AssertMemoryUsage(memoryReport.Other.IdTracker, allowExpectedCacheZero: true);

        static void AssertMemoryUsage(
            MemoryUsage memoryUsage,
            bool allowDiskZero = false,
            bool allowRamZero = false,
            bool allowCachedZero = false,
            bool allowExpectedCacheZero = false
        )
        {
            if (!allowDiskZero)
            {
                memoryUsage.DiskBytes.Should().BeGreaterThan(0);
            }

            if (!allowRamZero)
            {
                memoryUsage.RamBytes.Should().BeGreaterThan(0);
            }

            if (!allowCachedZero)
            {
                memoryUsage.CachedBytes.Should().BeGreaterThan(0);
            }

            if (!allowExpectedCacheZero)
            {
                memoryUsage.ExpectedCacheBytes.Should().BeGreaterThan(0);
            }
        }
    }
}
