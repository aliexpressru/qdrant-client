using Aer.QdrantClient.Http.Filters;
using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal partial class PointsCrudTests
{
    [Test]
    public async Task UpdatePointsVectors()
    {
        var vectorSize = 10U;

        var (upsertPoints, _, upsertPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName,
                vectorSize: vectorSize,
                payloadInitializerFunction: (i) => new TestPayload()
                {
                    Integer = i + 1,
                    Text = (i + 1).ToString()
                });

        // update points vectors by id

        var vectorToUpdateTo = CreateConstantTestVector(1.1f, vectorSize);

        var pointIdsToUpdateVectorsFor = upsertPoints.Take(5)
            .Select(p => p.Id).ToHashSet();

        var updatePointVectorsById =
            await _qdrantHttpClient.UpdatePointsVectors(
                TestCollectionName,
                new UpdatePointsVectorsRequest()
                {
                    Points = pointIdsToUpdateVectorsFor.Select(
                        pid => new PointVector()
                        {
                            Id = pid,
                            Vector = vectorToUpdateTo
                        }).ToArray()
                },
                CancellationToken.None
            );

        updatePointVectorsById.Status.IsSuccess.Should().BeTrue();

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        // check vector updated by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToUpdateVectorsFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToUpdateVectorsFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should().AllSatisfy(
            p =>
            {
                p.Vector.Default.AsDenseVector().VectorValues
                    .Should().BeEquivalentTo(vectorToUpdateTo);
            });

        pointsThatShouldNotBeUpdated.Should().AllSatisfy(
            p =>
            {
                p.Vector.Default.AsDenseVector().VectorValues.Should().NotBeEquivalentTo(vectorToUpdateTo);
            });
    }

    [Test]
    public async Task DeletePointsVectors_ById()
    {
        var vectorSize = 10U;
        var vectorCount = 10;
        var namedVectorsCount = 3;

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

        var upsertPointIds = upsertPoints.Select(p => p.Id).ToList();

        var pointIdsToDeleteVectorsFor = upsertPointIds.Take(5).ToHashSet();

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        upsertPointsResult.EnsureSuccess();

        var vectorNamesToDelete = vectorNames.Take(2).ToList();
        var vectorNamesToLeave = vectorNames.Skip(2).Take(2).ToList();

        var deletePointsVectorsResponse = await _qdrantHttpClient.DeletePointsVectors(
            TestCollectionName,
            new DeletePointsVectorsRequest(vectorNamesToDelete, pointIdsToDeleteVectorsFor),
            CancellationToken.None);

        deletePointsVectorsResponse.Status.IsSuccess.Should().BeTrue();

        // check vectors deleted by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToDeleteVectorsFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToDeleteVectorsFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should()
            .AllSatisfy(p => vectorNamesToDelete.All(vn => !p.Vector.ContainsVector(vn)).Should().BeTrue())
            .And.AllSatisfy(p => vectorNamesToLeave.All(vn => p.Vector.ContainsVector(vn)).Should().BeTrue());

        pointsThatShouldNotBeUpdated.Should()
            .AllSatisfy(p => vectorNamesToDelete.All(vn => p.Vector.ContainsVector(vn)).Should().BeTrue())
            .And.AllSatisfy(p => vectorNamesToLeave.All(vn => p.Vector.ContainsVector(vn)).Should().BeTrue());
    }

    [Test]
    public async Task DeletePointsVectors_ByFilter()
    {
        var vectorSize = 10U;
        var vectorCount = 10;
        var namedVectorsCount = 3;

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

        var upsertPointIds = upsertPoints.Select(p => p.Id).ToList();

        var pointIdsToDeleteVectorsFor = upsertPointIds.Take(5).ToHashSet();

        var upsertPointsResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = upsertPoints
                },
                CancellationToken.None);

        upsertPointsResult.EnsureSuccess();

        var vectorNamesToDelete = vectorNames.Take(2).ToList();
        var vectorNamesToLeave = vectorNames.Skip(2).Take(2).ToList();

        var deletePointsVectorsResponse = await _qdrantHttpClient.DeletePointsVectors(
            TestCollectionName,
            new DeletePointsVectorsRequest(
                vectorNamesToDelete,
                QdrantFilter.Create(
                    Q.Must(
                        Q.HaveAnyId(pointIdsToDeleteVectorsFor)
                    )
                )
            ),
            CancellationToken.None);

        deletePointsVectorsResponse.Status.IsSuccess.Should().BeTrue();

        // check vectors deleted by id

        var readAllPoints = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upsertPointIds,
            PayloadPropertiesSelector.All,
            CancellationToken.None,
            withVector: true);

        var pointsThatShouldBeUpdated = readAllPoints.Result
            .Where(p => pointIdsToDeleteVectorsFor.Contains(p.Id));

        var pointsThatShouldNotBeUpdated = readAllPoints.Result
            .Where(p => !pointIdsToDeleteVectorsFor.Contains(p.Id));

        pointsThatShouldBeUpdated.Should()
            .AllSatisfy(p => vectorNamesToDelete.All(vn => !p.Vector.ContainsVector(vn)).Should().BeTrue())
            .And.AllSatisfy(p => vectorNamesToLeave.All(vn => p.Vector.ContainsVector(vn)).Should().BeTrue());

        pointsThatShouldNotBeUpdated.Should()
            .AllSatisfy(p => vectorNamesToDelete.All(vn => p.Vector.ContainsVector(vn)).Should().BeTrue())
            .And.AllSatisfy(p => vectorNamesToLeave.All(vn => p.Vector.ContainsVector(vn)).Should().BeTrue());
    }
}
