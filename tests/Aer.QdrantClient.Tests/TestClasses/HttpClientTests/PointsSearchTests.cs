using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal class PointsSearchTests : QdrantTestsBase
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
    public async Task SearchPoints_CollectionDoesNotExist()
    {
        var searchPointInNonexistentCollectionResult
            = await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(CreateTestVector(10), 10),
                CancellationToken.None);

        searchPointInNonexistentCollectionResult.Status.IsSuccess.Should().BeFalse();
        searchPointInNonexistentCollectionResult.Status.Error.Should()
            .Contain(TestCollectionName)
            .And.Contain("doesn't exist");
    }

    [Test]
    public async Task SearchPoints_PointDoesNotExist()
    {
        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 10, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var searchForNonexistentPointResult
            = await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(CreateTestVector(10), 10),
                CancellationToken.None);

        searchForNonexistentPointResult.Status.IsSuccess.Should().BeTrue();

        searchForNonexistentPointResult.Result.Length.Should().Be(0);
    }

    [Test]
    public async Task SearchPoints_SinglePoint_WithoutFilter()
    {
        var (upsertPoints, upsertPointsByPointIds, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName);

        var searchResult =
            await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(
                    upsertPoints[0].Vector,
                    1)
                {
                    WithVector = true,
                    WithPayload = PayloadPropertiesSelector.All
                },
                CancellationToken.None);

        searchResult.Status.IsSuccess.Should().BeTrue();

        searchResult.Result.Length.Should().Be(1);

        searchResult.Result[0].Score.Should().BeGreaterThan(0);

        var readPointId = searchResult.Result[0].Id.AsInteger();

        var expectedPayload = upsertPointsByPointIds[readPointId].Payload.As<TestPayload>();
        var readPayload = searchResult.Result[0].Payload.As<TestPayload>();

        readPayload.Text.Should().Be(expectedPayload.Text);
        readPayload.Integer.Should().Be(expectedPayload.Integer);
        readPayload.FloatingPointNumber.Should().Be(expectedPayload.FloatingPointNumber);
    }

    [Test]
    public async Task SearchPoints_WithoutFilter()
    {
        var (upsertPoints, upsertPointsByPointIds, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName);

        var searchResult =
            await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(
                    upsertPoints[0].Vector,
                    5)
                {
                    WithVector = true,
                    WithPayload = PayloadPropertiesSelector.All
                },
                CancellationToken.None);

        searchResult.Status.IsSuccess.Should().BeTrue();

        searchResult.Result.Length.Should().Be(5);

        foreach (var readPoint in searchResult.Result)
        {
            readPoint.Score.Should().BeGreaterThan(0);
            
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.AsInteger().Should().Be(readPointId);

            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();
            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);
        }
    }

    [Test]
    public async Task SearchPoints_NamedVectors_WithoutFilter()
    {
        var vectorSize = 10U;
        var vectorCount = 10;
        var namedVectorsCount = 1;

        var vectorNames = CreateVectorNames(namedVectorsCount);

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(
                VectorDistanceMetric.Dot,
                vectorSize,
                isServeVectorsFromDisk: true,
                vectorNames)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    CreateTestNamedVectors(vectorSize, namedVectorsCount),
                    i
                )
            );
        }

        Dictionary<ulong, UpsertPointsRequest.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => ((IntegerPointId) p.Id).Id);

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        upsertPointsResult.EnsureSuccess();

        var searchResult =
            await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(
                    upsertPoints[0].Vector, // implicitly convert named vector with one name to NamedSearchVector
                    5)
                {
                    WithVector = true,
                    WithPayload = PayloadPropertiesSelector.All
                },
                CancellationToken.None);

        searchResult.Status.IsSuccess.Should().BeTrue();

        searchResult.Result.Length.Should().Be(5);

        foreach (var readPoint in searchResult.Result)
        {
            readPoint.Score.Should().BeGreaterThan(0);
            
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.AsInteger().Should().Be(readPointId);

            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();
            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);
        }
    }

    [Test]
    public async Task SearchPoints_NamedVectors_SparseVectors_WithoutFilter()
    {
        var vectorSize = 10U;
        var vectorCount = 10;

        Dictionary<string, SparseVectorConfiguration> sparseVectors = new()
        {
            [VectorBase.DefaultVectorName] = new(onDisk: true, fullScanThreshold: 1000),
        };

        var collectionCreationResult = await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(sparseVectors)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        collectionCreationResult.EnsureSuccess();

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>();

        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer(i),
                    vector: new NamedVectors()
                    {
                        Vectors = new Dictionary<string, VectorBase>()
                        {
                            [VectorBase.DefaultVectorName] = CreateTestSparseVector(vectorSize, 5)
                        }
                    },
                    new TestPayload()
                    {
                        Integer = i,
                        Text = i.ToString()
                    })
            );
        }

        Dictionary<ulong, UpsertPointsRequest.UpsertPoint> upsertPointsByPointIds =
            upsertPoints.ToDictionary(p => p.Id.AsInteger());

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        upsertPointsResult.EnsureSuccess();

        var searchResult =
            await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(
                    upsertPoints[0].Vector, // implicitly convert named vector with one name to NamedSearchVector
                    5)
                {
                    WithVector = true,
                    WithPayload = PayloadPropertiesSelector.All
                },
                CancellationToken.None);

        searchResult.Status.IsSuccess.Should().BeTrue();

        searchResult.Result.Length.Should().Be(5);

        foreach (var readPoint in searchResult.Result)
        {
            readPoint.Score.Should().BeGreaterThan(0);
            
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.AsInteger().Should().Be(readPointId);

            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();
            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);
        }
    }

    [Test]
    public async Task SearchPoints_WithoutFilter_WithQuantization()
    {
        var (upsertPoints, upsertPointsByPointIds, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                quantizationConfig: QuantizationConfiguration.Scalar(quantile: 0.99f, isQuantizedVectorAlwaysInRam: true));

        var searchResult =
            await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(
                    upsertPoints[0].Vector,
                    5)
                {
                    Params = new VectorSearchParameters()
                    {
                        Quantization = new VectorSearchParameters.QuantizationParameters()
                        {
                            Ignore = false,
                            Rescore = true,
                            Oversampling = 1
                        }
                    },
                    WithVector = true,
                    WithPayload = PayloadPropertiesSelector.All
                },
                CancellationToken.None);

        searchResult.Status.IsSuccess.Should().BeTrue();

        searchResult.Result.Length.Should().Be(5);

        foreach (var readPoint in searchResult.Result)
        {
            readPoint.Score.Should().BeGreaterThan(0);
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.AsInteger().Should().Be(readPointId);

            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();
            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);
        }
    }

    [Test]
    public async Task SearchPoints_WithFilter()
    {
        var (upsertPoints, upsertPointsByPointIds, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName);

        var searchResult =
            await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(
                    upsertPoints[0].Vector,
                    5)
                {
                    WithVector = true,
                    WithPayload = PayloadPropertiesSelector.All,
                    Filter =
                        Q.Must(
                            Q<TestPayload>.BeInRange(p => p.Integer, greaterThanOrEqual: 0)
                        )
                },
                CancellationToken.None
            );

        searchResult.Status.IsSuccess.Should().BeTrue();

        searchResult.Result.Length.Should().Be(5);

        foreach (var readPoint in searchResult.Result)
        {
            readPoint.Score.Should().BeGreaterThan(0);
            
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.AsInteger().Should().Be(readPointId);

            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();
            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);
        }
    }

    [Test]
    public async Task SearchPoints_Euclid_WithFilter()
    {
        var (upsertPoints, upsertPointsByPointIds, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                distanceMetric: VectorDistanceMetric.Euclid);

        var searchResult =
            await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(
                    upsertPoints[0].Vector,
                    5)
                {
                    WithVector = true,
                    WithPayload = PayloadPropertiesSelector.All,
                    Filter =
                        Q.Must(
                            Q<TestPayload>.BeInRange(p => p.Integer, greaterThanOrEqual: 0)
                        )
                },
                CancellationToken.None
            );

        searchResult.Status.IsSuccess.Should().BeTrue();

        searchResult.Result.Length.Should().Be(5);

        foreach (var readPoint in searchResult.Result)
        {
            if (readPoint.Id.Equals(upsertPoints[0].Id))
            {
                // The exact point with which we searched for similar will give 0 score 
                readPoint.Score.Should().Be(0);
            }
            else
            {
                readPoint.Score.Should().NotBe(0);
            }

            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.AsInteger().Should().Be(readPointId);

            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();
            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.Text.Should().Be(expectedPointPayload.Text);
        }
    }

    [Test]
    public async Task SearchPoints_Cosine_WithFilter()
    {
        var (upsertPoints, upsertPointsByPointIds, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                distanceMetric: VectorDistanceMetric.Cosine);

        var searchResult =
            await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(
                    upsertPoints[0].Vector,
                    5)
                {
                    WithVector = true,
                    WithPayload = PayloadPropertiesSelector.All,
                    Filter =
                        Q.Must(
                            Q<TestPayload>.BeInRange(p => p.Integer, greaterThanOrEqual: 0)
                        )
                },
                CancellationToken.None
            );

        searchResult.Status.IsSuccess.Should().BeTrue();

        searchResult.Result.Length.Should().Be(5);

        foreach (var readPoint in searchResult.Result)
        {
            readPoint.Score.Should().BeGreaterThan(0);
            
            var readPointId = readPoint.Id.AsInteger();

            var expectedPoint = upsertPointsByPointIds[readPointId];

            expectedPoint.Id.AsInteger().Should().Be(readPointId);

            var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();
            var readPointPayload = readPoint.Payload.As<TestPayload>();

            readPointPayload.As<TestPayload>().Integer.Should().Be(expectedPointPayload.Integer);
            readPointPayload.As<TestPayload>().FloatingPointNumber.Should().Be(expectedPointPayload.FloatingPointNumber);
            readPointPayload.As<TestPayload>().Text.Should().Be(expectedPointPayload.Text);
        }
    }

    [Test]
    public async Task SearchPoints_Must_MatchAny_Equivalent_Should_MatchValue_Filter()
    {
        var vectorSize = 10U;
        var vectorCount = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>();

        // using the same vector since we are comparing filter return values

        var singleVector = CreateTestVector(vectorSize);

        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    singleVector,
                    i
                )
            );
        }

        var filterValuesToMatch = Enumerable.Range(0, 5).ToArray();

        await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest()
            {
                Points = upsertPoints
            },
            CancellationToken.None);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var searchResultWithMustMatchAnyFilter =
            await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(
                    singleVector,
                    5)
                {
                    WithVector = true,
                    WithPayload = PayloadPropertiesSelector.All,
                    Filter =
                        Q.Must(
                            Q<TestPayload>.MatchAny(p => p.Integer, filterValuesToMatch)
                        )
                },
                CancellationToken.None
            );

        var searchResultWithShouldMatchValueFilter =
            await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(
                    singleVector,
                    5)
                {
                    WithVector = true,
                    WithPayload = PayloadPropertiesSelector.All,
                    Filter =
                        Q.Should(
                            filterValuesToMatch.Select(
                                    filterValue =>
                                        Q<TestPayload>.MatchValue(p => p.Integer, filterValue)
                                )
                                .ToArray()
                        )
                },
                CancellationToken.None
            );

        searchResultWithMustMatchAnyFilter.Status.IsSuccess.Should().BeTrue();
        searchResultWithShouldMatchValueFilter.Status.IsSuccess.Should().BeTrue();

        searchResultWithMustMatchAnyFilter.Result.Length.Should().Be(filterValuesToMatch.Length);
        searchResultWithShouldMatchValueFilter.Result.Length.Should().Be(filterValuesToMatch.Length);

        searchResultWithMustMatchAnyFilter.Result.Length.Should()
            .Be(searchResultWithShouldMatchValueFilter.Result.Length);

        var searchResultWithMustMatchAnyFilterPintIds =
            searchResultWithMustMatchAnyFilter.Result.Select(r => r.Id.AsInteger());

        var searchResultWithShouldMatchValueFilterPintIds =
            searchResultWithShouldMatchValueFilter.Result.Select(r => r.Id.AsInteger());

        searchResultWithMustMatchAnyFilterPintIds.Should().BeEquivalentTo(searchResultWithShouldMatchValueFilterPintIds);
    }

    [Test]
    public async Task SearchPoints_Must_MatchAny_Equivalent_MatchAnyFast_Filter()
    {
        var vectorSize = 10U;
        var vectorCount = 10;

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var upsertPoints = new List<UpsertPointsRequest.UpsertPoint>();

        // using the same vector since we are comparing filter return values

        var singleVector = CreateTestVector(vectorSize);

        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    singleVector,
                    i
                )
            );
        }

        var filterValuesToMatch = Enumerable.Range(0, 5).ToArray();

        await _qdrantHttpClient.UpsertPoints(
            TestCollectionName,
            new UpsertPointsRequest()
            {
                Points = upsertPoints
            },
            CancellationToken.None);

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        var searchResultWithMustMatchAnyFilter =
            await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(
                    singleVector,
                    5)
                {
                    WithVector = true,
                    WithPayload = PayloadPropertiesSelector.All,
                    Filter =
                        Q.Must(
                            Q<TestPayload>.MatchAny(p => p.Integer, filterValuesToMatch)
                        )
                },
                CancellationToken.None
            );

        var searchResultWithMustMatchAnyFastFilter =
            await _qdrantHttpClient.SearchPoints(
                TestCollectionName,
                new SearchPointsRequest(
                    singleVector,
                    5)
                {
                    WithVector = true,
                    WithPayload = PayloadPropertiesSelector.All,
                    Filter =
                        Q.Must(
                            Q<TestPayload>.MatchAnyFast(p => p.Integer, filterValuesToMatch)
                        )
                },
                CancellationToken.None
            );

        searchResultWithMustMatchAnyFilter.Status.IsSuccess.Should().BeTrue();
        searchResultWithMustMatchAnyFastFilter.Status.IsSuccess.Should().BeTrue();

        searchResultWithMustMatchAnyFilter.Result.Length.Should().Be(filterValuesToMatch.Length);
        searchResultWithMustMatchAnyFastFilter.Result.Length.Should().Be(filterValuesToMatch.Length);

        searchResultWithMustMatchAnyFilter.Result.Length.Should()
            .Be(searchResultWithMustMatchAnyFastFilter.Result.Length);

        var searchResultWithMustMatchAnyFilterPintIds =
            searchResultWithMustMatchAnyFilter.Result.Select(r => r.Id.AsInteger());

        var searchResultWithShouldMatchValueFilterPintIds =
            searchResultWithMustMatchAnyFastFilter.Result.Select(r => r.Id.AsInteger());

        searchResultWithMustMatchAnyFilterPintIds.Should().BeEquivalentTo(searchResultWithShouldMatchValueFilterPintIds);
    }

    [Test]
    public async Task SearchPointsBatched_WithFilter()
    {
        var (upsertPoints, upsertPointsByPointIds, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName);

        var searchResult =
            await _qdrantHttpClient.SearchPointsBatched(
                TestCollectionName,
                new SearchPointsBatchedRequest(
                    new SearchPointsRequest(upsertPoints[0].Vector, 5)
                    {
                        WithVector = true,
                        WithPayload = PayloadPropertiesSelector.All,
                        Filter =
                            Q.Must(
                                Q<TestPayload>.BeInRange(p => p.Integer, greaterThanOrEqual: 0)
                            )
                    },
                    new SearchPointsRequest(upsertPoints[1].Vector, 5)
                    {
                        WithVector = true,
                        WithPayload = PayloadPropertiesSelector.All,
                        Filter =
                            Q.Must(
                                Q<TestPayload>.BeInRange(p => p.Integer, greaterThanOrEqual: 0)
                            )
                    }),
                CancellationToken.None
            );

        searchResult.Status.IsSuccess.Should().BeTrue();

        searchResult.Result.Length.Should().Be(2); // two single requests in batch request

        foreach (var readPointsForOneRequestInBatch in searchResult.Result)
        {
            readPointsForOneRequestInBatch.Length.Should().Be(5);

            foreach (var readPoint in readPointsForOneRequestInBatch)
            {
                readPoint.Score.Should().BeGreaterThan(0);
                
                var readPointId = readPoint.Id.AsInteger();

                var expectedPoint = upsertPointsByPointIds[readPointId];

                expectedPoint.Id.AsInteger().Should().Be(readPointId);

                var expectedPointPayload = expectedPoint.Payload.As<TestPayload>();
                var readPointPayload = readPoint.Payload.As<TestPayload>();

                readPointPayload.Integer.Should().Be(expectedPointPayload.Integer);
                readPointPayload.FloatingPointNumber.Should()
                    .Be(expectedPointPayload.FloatingPointNumber);
                readPointPayload.Text.Should().Be(expectedPointPayload.Text);
            }
        }
    }

    [Test]
    public async Task SearchPointsGrouped_WithFilter()
    {
        var vectorCount = 10;

        var (upsertPoints, _, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: vectorCount,
                payloadInitializerFunction: i => new TestPayload()
                {
                    Integer = i < 5
                        ? 1
                        : 2,
                    Text = (i + 1).ToString()
                });

        var searchResult =
            await _qdrantHttpClient.SearchPointsGrouped(
                TestCollectionName,
                new SearchPointsGroupedRequest(
                    upsertPoints[0].Vector,
                    groupBy: Q<TestPayload>.GetPayloadFieldName(p => p.Integer),
                    groupsLimit: 2,
                    groupSize: 10,
                    withVector: true,
                    withPayload: true),
                CancellationToken.None
            );

        searchResult.Status.IsSuccess.Should().BeTrue();
        searchResult.Result.Groups.Length.Should().Be(2); // 2 possible values of Integer payload property

        searchResult.Result.Groups.Should()
            .AllSatisfy(g => g.Hits.Length.Should().Be(vectorCount / 2))
            .And.AllSatisfy(
                g=> g.Hits.Should()
                    .AllSatisfy(h=>h.Payload.Should().NotBeNull())
                    .And.AllSatisfy(h=>h.Vector.Should().NotBeNull())
                    .And.AllSatisfy(h=>h.Score.Should().BeGreaterThan(0))
            );
    }

    [Test]
    public async Task SearchPoints_DistanceMatrix_Pairs()
    {
        var nearestPointsToConsider = 4;
        var vectorCount = 10;

        await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: vectorCount,
                payloadInitializerFunction: i => new TestPayload()
                {
                    Integer = i
                });

        var searchResult =
            await _qdrantHttpClient.SearchPointsDistanceMatrixPairs(
                TestCollectionName,
                new SearchPointsDistanceMatrixRequest()
                {
                    Filter = Q<TestPayload>.BeInRange(p=>p.Integer, greaterThanOrEqual: 2), // total points 10 - 2 = 8
                    Sample = 10,
                    Limit = (uint)nearestPointsToConsider
                },
                CancellationToken.None);

        searchResult.Status.IsSuccess.Should().BeTrue();

        searchResult.Result.Pairs.Length.Should().Be(nearestPointsToConsider * (vectorCount - 2)); // 2 points filtered out

        foreach (var pointsPairDistance in searchResult.Result.Pairs)
        {
            // check points 0 and 1 are excluded by filter
            pointsPairDistance.A.AsInteger().Should().BeGreaterThan(1);
            pointsPairDistance.B.AsInteger().Should().BeGreaterThan(1);

            pointsPairDistance.Score.Should().BeGreaterThan(1);
        }
    }

    [Test]
    public async Task SearchPoints_DistanceMatrix_Offsets()
    {
        var nearestPointsToConsider = 4;
        var vectorCount = 10;

        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName,
            vectorCount: vectorCount,
            payloadInitializerFunction: i => new TestPayload()
            {
                Integer = i
            });

        var searchResult =
            await _qdrantHttpClient.SearchPointsDistanceMatrixOffsets(
                TestCollectionName,
                new SearchPointsDistanceMatrixRequest()
                {
                    Filter = Q<TestPayload>.BeInRange(p => p.Integer, greaterThanOrEqual: 2), // total points 10 - 2 = 8
                    Sample = 10,
                    Limit = (uint)nearestPointsToConsider
                },
                CancellationToken.None);

        searchResult.Status.IsSuccess.Should().BeTrue();

        var consideredPointsCount = vectorCount - 2; // 2 points filtered out

        searchResult.Result.Ids.Count.Should().Be(consideredPointsCount);

        var expectedResultsCount = nearestPointsToConsider * consideredPointsCount;

        searchResult.Result.OffsetsCol.Length.Should().Be(expectedResultsCount);
        searchResult.Result.OffsetsRow.Length.Should().Be(expectedResultsCount);

        searchResult.Result.Scores.Length.Should().Be(expectedResultsCount);

        searchResult.Result.Ids.Should().AllSatisfy(pid=>pid.AsInteger().Should().BeGreaterThan(1));
        searchResult.Result.Scores.Should().AllSatisfy(s=>s.Should().BeGreaterThan(1));
    }
}
