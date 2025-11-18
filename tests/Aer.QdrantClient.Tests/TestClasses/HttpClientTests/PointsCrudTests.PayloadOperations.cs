using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Tests.Helpers;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal partial class PointsCrudTests
{
    [Test]
    public async Task SetPointsPayload()
    {
        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                payloadInitializerFunction: (i) => (TestPayload) (i + 1));

        // update payload by id

        var pointIdsToUpdatePayloadFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var setPayloadById = await _qdrantHttpClient.SetPointsPayload(
            TestCollectionName,
            new SetPointsPayloadRequest(
                (TestPayload) "100",
                pointIdsToUpdatePayloadFor
            ),
            CancellationToken.None
        );

        setPayloadById.Status.IsSuccess.Should().BeTrue();

        // check payload updated by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdatePayloadFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdatePayloadFor.Contains(p.Id));

        // check initial key is intact
        readAllPoints.Result.Should().AllSatisfy(p => p.Payload.As<TestPayload>()
            .Integer.Should().BeGreaterThan(0));

        pointsThatShouldBeUpdated.Should().AllSatisfy(p => p.Payload.As<TestPayload>()
            .Text.Should().Be("100"));
        pointsThatShouldNotBeUpdated.Should().AllSatisfy(p => p.Payload.As<TestPayload>()
            .Text.Should().NotBe("100"));

        // update payload by filter

        var pointsToUpdatePayloadByFilter = upsertPoints.Skip(5).Take(5);

        var pointIdsToUpdateByFilter = pointsToUpdatePayloadByFilter.Select(p => p.Id).ToHashSet();

        var pointFilterToUpdatePayloadFor = QdrantFilter.Create(
            Q.Must(
                Q.HaveAnyId(pointIdsToUpdateByFilter))
        );

        var setPayloadByFilter = await _qdrantHttpClient.SetPointsPayload(
            TestCollectionName,
            new SetPointsPayloadRequest(
                """
                {
                    "text": "1000"
                }
                """,
                pointFilterToUpdatePayloadFor),
            CancellationToken.None);

        setPayloadByFilter.Status.IsSuccess.Should().BeTrue();

        // check payload updated by filter

        readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        readAllPoints.Status.IsSuccess.Should().BeTrue();

        pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdateByFilter.Contains(p.Id));
        pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdateByFilter.Contains(p.Id));

        // check initial key is intact
        readAllPoints.Result.Should().AllSatisfy(p => p.Payload.As<TestPayload>().Integer.Should().BeGreaterThan(0)
        );

        pointsThatShouldBeUpdated.Should().AllSatisfy(p => p.Payload.As<TestPayload>().Text.Should().Be("1000")
        );
        pointsThatShouldNotBeUpdated.Should().AllSatisfy(p => p.Payload.As<TestPayload>().Text.Should().NotBe("1000")
        );
    }

    [Test]
    public async Task SetPointsPayloadByKey()
    {
        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                payloadInitializerFunction: (i) => new TestComplexPayload() {IntProperty = i + 1});

        // update payload key by id

        var pointIdsToUpdatePayloadFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var setPayloadByIdAndKey = await _qdrantHttpClient.SetPointsPayload(
            TestCollectionName,
            new SetPointsPayloadRequest(
                new TestComplexPayload.NestedClass()
                {
                    Double = 1.567
                },
                pointIdsToUpdatePayloadFor,
                key: "nested"
            ),
            CancellationToken.None
        );

        setPayloadByIdAndKey.Status.IsSuccess.Should().BeTrue();

        // check payload updated by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdatePayloadFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdatePayloadFor.Contains(p.Id));

        // check initial key is intact
        readAllPoints.Result.Should().AllSatisfy(p => p.Payload.As<TestComplexPayload>()
            .IntProperty.Should().BeGreaterThan(0)
        );

        pointsThatShouldBeUpdated.Should().AllSatisfy(p => p.Payload.As<TestComplexPayload>()
            .Nested.Double.Should().Be(1.567)
        );

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(p => p.Payload.As<TestComplexPayload>()
            .Nested.Should().BeNull()
        );

        // update payload by filter

        var pointsToUpdatePayloadByFilter = upsertPoints.Skip(5).Take(5);

        var pointIdsToUpdateByFilter = pointsToUpdatePayloadByFilter.Select(p => p.Id).ToHashSet();

        var pointFilterToUpdatePayloadFor = QdrantFilter.Create(
            Q.Must(
                Q.HaveAnyId(pointIdsToUpdateByFilter))
        );

        var setPayloadByFilterAndKey = await _qdrantHttpClient.SetPointsPayload(
            TestCollectionName,
            new SetPointsPayloadRequest(
                new TestComplexPayload.NestedNestedClass()
                {
                    Double = 1567.12
                },
                pointFilterToUpdatePayloadFor,
                key: "nested.nested"),
            CancellationToken.None);

        setPayloadByFilterAndKey.Status.IsSuccess.Should().BeTrue();

        // check payload updated by filter

        readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        readAllPoints.Status.IsSuccess.Should().BeTrue();

        pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdateByFilter.Contains(p.Id));

        pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdateByFilter.Contains(p.Id));

        // check initial key is intact
        readAllPoints.Result.Should()
            .AllSatisfy(p => p.Payload.As<TestComplexPayload>().IntProperty.Should().BeGreaterThan(0)
            );

        pointsThatShouldBeUpdated.Should()
            .AllSatisfy(p => p.Payload.As<TestComplexPayload>().Nested.Nested.Double.Should().Be(1567.12));

        pointsThatShouldNotBeUpdated.Should()
            .AllSatisfy(p => p.Payload.As<TestComplexPayload>().Nested.Nested.Should().BeNull());
    }

    [Test]
    public async Task OverwritePointsPayload_BatchOperation_NonExistentPoints()
    {
        var (upsertPoints, _, _) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                vectorCount: 3,
                payloadInitializerFunction: (i) => (TestPayload) (i + 1)
            );

        // One present and two non-existent point ids
        PointId[] pointIdsToUpdatePayloadFor = [upsertPoints[0].Id, 1567, 1568];

        var expectedText = "overwritten";

        var newPayload = new TestPayload()
        {
            Text = expectedText
        };

        var batchUpdateRequest = BatchUpdatePointsRequest.Create();

        batchUpdateRequest.OverwritePointsPayload(
            newPayload,
            pointsToOverwritePayloadFor: pointIdsToUpdatePayloadFor);

        var overwritePayloadResult = await _qdrantHttpClient.BatchUpdate(
            TestCollectionName,
            batchUpdateRequest,
            CancellationToken.None,
            isWaitForResult: true);

        // Success is false since we haven't found points by non-existent ids 1567 and 1568
        // Error message contains only one point id though - the first not found
        // The update operation for existent points are successful though
        // Yeah, this API is a mess
        overwritePayloadResult.Status.IsSuccess.Should().BeFalse();
        overwritePayloadResult.Status.GetErrorMessage().Should().Contain("1567");
        overwritePayloadResult.Status.GetErrorMessage().Should().NotContain("1568");

        var readModifiedPointsBackResponse = await _qdrantHttpClient.ScrollPoints(
            TestCollectionName,
            Q.HaveAnyId(upsertPoints[0].Id),
            true,
            CancellationToken.None);

        readModifiedPointsBackResponse.Status.IsSuccess.Should().BeTrue();

        readModifiedPointsBackResponse.Result.Points.Length.Should().Be(1);
        readModifiedPointsBackResponse.Result.Points[0].Payload.As<TestPayload>().Text.Should().Be(expectedText);
        readModifiedPointsBackResponse.Result.Points[0].Payload.As<TestPayload>().Integer.Should().BeNull();
    }

    [Test]
    public async Task OverwritePointsPayload()
    {
        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                payloadInitializerFunction: (i) => (TestPayload) (i + 1));

        // overwrite payload by id

        var pointIdsToUpdatePayloadFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var overwritePayloadById = await _qdrantHttpClient.OverwritePointsPayload(
            TestCollectionName,
            new OverwritePointsPayloadRequest(
                (TestPayload) "100",
                pointIdsToUpdatePayloadFor
            ),
            CancellationToken.None
        );

        overwritePayloadById.Status.IsSuccess.Should().BeTrue();

        // check payload updated by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdatePayloadFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdatePayloadFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(p =>
        {
            // check initial key is overwritten
            p.Payload.As<TestPayload>().Integer.Should().BeNull();

            p.Payload.As<TestPayload>().Text.Should().Be("100");
        });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(p =>
        {
            // check initial key is not overwritten
            p.Payload.As<TestPayload>().Integer.Should().NotBeNull();

            p.Payload.As<TestPayload>().Text.Should().NotBe("100");
        });

        // overwrite payload by filter

        var pointsToUpdatePayloadByFilter = upsertPoints.Skip(5).Take(5);

        var pointIdsToUpdateByFilter = pointsToUpdatePayloadByFilter
            .Select(p => p.Id).ToHashSet();

        var pointFilterToUpdatePayloadFor = QdrantFilter.Create(
            Q.Must(
                Q.HaveAnyId(pointIdsToUpdateByFilter))
        );

        var overwritePayloadByFilter = await _qdrantHttpClient.OverwritePointsPayload(
            TestCollectionName,
            new OverwritePointsPayloadRequest(
                (TestPayload) "1000",
                pointFilterToUpdatePayloadFor),
            CancellationToken.None);

        overwritePayloadByFilter.Status.IsSuccess.Should().BeTrue();

        // check payload updated by filter

        readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        readAllPoints.Status.IsSuccess.Should().BeTrue();

        pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdateByFilter.Contains(p.Id));

        pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdateByFilter.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(p =>
        {
            // check initial key is overwritten
            p.Payload.As<TestPayload>().Integer.Should().BeNull();

            p.Payload.As<TestPayload>().Text.Should().Be("1000");
        });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(p => p.Payload.As<TestPayload>()
            .Text.Should().NotBe("1000"));
    }

    [Test]
    public async Task OverwritePointsPayload_NestedPropertyPath()
    {
        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                payloadInitializerFunction: (i) => new TestComplexPayload()
                {
                    IntProperty = i,
                    Nested = new TestComplexPayload.NestedClass()
                    {
                        Integer = i
                    }
                });

        // overwrite payload by id and path

        var pointIdsToUpdatePayloadFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var overwritePayloadById = await _qdrantHttpClient.OverwritePointsPayload(
            TestCollectionName,
            new OverwritePointsPayloadRequest(
                new {Integer = 100},
                pointIdsToUpdatePayloadFor,
                "nested"
            ),
            CancellationToken.None
        );

        overwritePayloadById.Status.IsSuccess.Should().BeTrue();

        // check payload updated by id and path

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdatePayloadFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdatePayloadFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(p =>
        {
            // check initial key is overwritten
            p.Payload.As<TestComplexPayload>().IntProperty.Should().NotBeNull();

            p.Payload.As<TestComplexPayload>().Nested.Integer.Should().Be(100);
        });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(p =>
        {
            // check initial key is not overwritten
            p.Payload.As<TestComplexPayload>().IntProperty.Should().NotBeNull();

            p.Payload.As<TestComplexPayload>().Nested.Integer.Should().NotBe(100);
        });
    }

    [Test]
    public async Task OverwritePointsPayload_SingleValue_ByJsonKey()
    {
        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                payloadInitializerFunction: (i) => new TestComplexPayload()
                {
                    IntProperty = i,
                    Nested = new TestComplexPayload.NestedClass()
                    {
                        Integer = i
                    }
                });

        var pointIdsToUpdatePayloadFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var overwritePayloadById = await _qdrantHttpClient.OverwritePointsPayload(
            TestCollectionName,
            new OverwritePointsPayloadRequest(
                """
                {
                    "integer": 100
                }
                """,
                pointIdsToUpdatePayloadFor,
                "nested"
            ),
            CancellationToken.None
        );

        overwritePayloadById.Status.IsSuccess.Should().BeTrue();

        // check payload updated by id and path

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdatePayloadFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdatePayloadFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(p =>
        {
            // check initial key is overwritten
            p.Payload.As<TestComplexPayload>().IntProperty.Should().NotBeNull();

            p.Payload.As<TestComplexPayload>().Nested.Integer.Should().Be(100);
        });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(p =>
        {
            // check initial key is not overwritten
            p.Payload.As<TestComplexPayload>().IntProperty.Should().NotBeNull();

            p.Payload.As<TestComplexPayload>().Nested.Integer.Should().NotBe(100);
        });
    }

    [Test]
    public async Task DeletePointsPayloadKeys()
    {
        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                payloadInitializerFunction: (i) => new TestPayload()
                {
                    Integer = i + 1,
                    Text = (i + 1).ToString()
                });

        // delete payload key by id

        var pointIdsToDeletePayloadKeysFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var deletePayloadKeysById = await _qdrantHttpClient.DeletePointsPayloadKeys(
            TestCollectionName,
            new DeletePointsPayloadKeysRequest(
                // we can't use the plain literal "Integer" here since when we are serializing json we are lowercasing property names
                // thus we are using filter helper here
                Q<TestPayload>.GetPayloadFieldName(p => p.Integer).YieldSingle(),
                pointIdsToDeletePayloadKeysFor),
            CancellationToken.None
        );

        deletePayloadKeysById.Status.IsSuccess.Should().BeTrue();

        // check payload key deleted by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToDeletePayloadKeysFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToDeletePayloadKeysFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(p =>
        {
            // check initial key intact
            p.Payload.As<TestPayload>().Text.Should().NotBeNull();

            p.Payload.As<TestPayload>().Integer.Should().BeNull();
        });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(p =>
        {
            // check initial key intact
            p.Payload.As<TestPayload>().Text.Should().NotBeNull();

            p.Payload.As<TestPayload>().Integer.Should().NotBeNull();
        });

        // delete payload key by filter

        var pointsToDeletePayloadKeysForByFilter = upsertPoints.Skip(5).Take(5);

        var pointIdsToUpdateByFilter = pointsToDeletePayloadKeysForByFilter
            .Select(p => p.Id).ToHashSet();

        var pointFilterToUpdatePayloadFor = QdrantFilter.Create(
            Q.Must(
                Q.HaveAnyId(pointIdsToUpdateByFilter))
        );

        var deletePayloadKeysByFilter = await _qdrantHttpClient.DeletePointsPayloadKeys(
            TestCollectionName,
            new DeletePointsPayloadKeysRequest(
                // we can't use the plain literal "Integer" here since when we are serializing json we are lowercasing property names
                // thus we are using filter helper here
                Q<TestPayload>.GetPayloadFieldName(p => p.Text).YieldSingle(),
                pointFilterToUpdatePayloadFor),
            CancellationToken.None);

        deletePayloadKeysByFilter.Status.IsSuccess.Should().BeTrue();

        // check payload updated by filter

        readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        readAllPoints.Status.IsSuccess.Should().BeTrue();

        pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdateByFilter.Contains(p.Id));

        pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdateByFilter.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(p =>
        {
            // check initial key intact
            p.Payload.As<TestPayload>().Text.Should().BeNull();

            // check initial key is overwritten
            p.Payload.As<TestPayload>().Integer.Should().NotBeNull();
        });

        pointsThatShouldNotBeUpdated.Should()
            .AllSatisfy(p => { p.Payload.As<TestPayload>().Text.Should().NotBeNull(); });
    }

    [Test]
    public async Task ClearPointsPayload()
    {
        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                payloadInitializerFunction: (i) => new TestPayload()
                {
                    Integer = i + 1,
                    Text = (i + 1).ToString()
                });

        // clear payload by id

        var pointIdsToClearPayloadFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var clearPayloadById = await _qdrantHttpClient.ClearPointsPayload(
            TestCollectionName,
            new ClearPointsPayloadRequest(pointIdsToClearPayloadFor),
            CancellationToken.None
        );

        clearPayloadById.Status.IsSuccess.Should().BeTrue();

        // check payload cleared by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToClearPayloadFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToClearPayloadFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(p =>
        {
            p.Payload.RawPayloadString.Should().Be(Payload.EmptyString);
            p.Payload.IsEmpty.Should().BeTrue();
        });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(p =>
        {
            p.Payload.RawPayloadString.Should().NotBe(Payload.EmptyString);
            p.Payload.IsEmpty.Should().BeFalse();
        });

        // clear payload key by filter

        var pointsToClearPayloadForByFilter = upsertPoints.Skip(5).Take(5);

        var pointIdsToUpdateByFilter = pointsToClearPayloadForByFilter
            .Select(p => p.Id).ToHashSet();

        var pointFilterToUpdatePayloadFor = QdrantFilter.Create(
            Q.Must(
                Q.HaveAnyId(pointIdsToUpdateByFilter))
        );

        var clearPayloadByFilter = await _qdrantHttpClient.ClearPointsPayload(
            TestCollectionName,
            new ClearPointsPayloadRequest(pointFilterToUpdatePayloadFor),
            CancellationToken.None);

        clearPayloadByFilter.Status.IsSuccess.Should().BeTrue();

        // check payload cleared by filter

        readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None);

        readAllPoints.Status.IsSuccess.Should().BeTrue();

        readAllPoints.Result.Should().AllSatisfy(p =>
        {
            p.Payload.RawPayloadString.Should().Be(Payload.EmptyString);
            p.Payload.IsEmpty.Should().BeTrue();
        });
    }
}
