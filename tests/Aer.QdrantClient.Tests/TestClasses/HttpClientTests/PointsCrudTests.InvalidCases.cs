using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Helpers;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

internal partial class PointsCrudTests
{
    [Test]
    public async Task GetPoint_CollectionDoesNotExist()
    {
        var getPointFromNonexistentCollectionResult
            = await _qdrantHttpClient.GetPoint(TestCollectionName, PointId.Integer(1), CancellationToken.None);

        getPointFromNonexistentCollectionResult.Status.IsSuccess.Should().BeFalse();
        getPointFromNonexistentCollectionResult.Status.Error.Should()
            .Contain(TestCollectionName)
            .And.Contain("doesn't exist");
    }

    [Test]
    public async Task GetPoint_EnsureSuccess_CollectionDoesNotExist()
    {
        var getPointFromNonexistentCollectionResult
            = await _qdrantHttpClient.GetPoint(TestCollectionName, PointId.Integer(1), CancellationToken.None);

        var act = () => getPointFromNonexistentCollectionResult.EnsureSuccess();

        act.Should().Throw<QdrantUnsuccessfulResponseStatusException>()
            .Where(e => e.Message.Contains("doesn't exist"));
    }

    [Test]
    public async Task GetPoint_PointDoesNotExist_NumberId()
    {
        var nonexistentPointId = PointId.Integer(1);

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var getNonexistentPointResult
            = await _qdrantHttpClient.GetPoint(TestCollectionName, nonexistentPointId, CancellationToken.None);

        getNonexistentPointResult.Status.IsSuccess.Should().BeFalse();
        getNonexistentPointResult.Status.Error.Should()
            .Contain("Not found")
            .And.Contain(nonexistentPointId.ToString())
            .And.Contain("does not exists");

        getNonexistentPointResult.Result.Should().BeNull();
    }

    [Test]
    public async Task GetPoint_PointDoesNotExist_GuidId()
    {
        var nonexistentPointId = PointId.Guid(Guid.NewGuid());

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var getNonexistentPointResult
            = await _qdrantHttpClient.GetPoint(TestCollectionName, nonexistentPointId, CancellationToken.None);

        getNonexistentPointResult.Status.IsSuccess.Should().BeFalse();
        getNonexistentPointResult.Status.Error.Should()
            .Contain("Not found")
            .And.Contain(nonexistentPointId.ToString())
            .And.Contain("does not exists");

        getNonexistentPointResult.Result.Should().BeNull();
    }

    [Test]
    public async Task GetPoints_CollectionDoesNotExist()
    {
        var getPointFromNonexistentCollectionResult
            = await _qdrantHttpClient.GetPoints(
                TestCollectionName,
                PointId.Integer(1).YieldSingle(),
                PayloadPropertiesSelector.None,
                CancellationToken.None);

        getPointFromNonexistentCollectionResult.Status.IsSuccess.Should().BeFalse();
        getPointFromNonexistentCollectionResult.Status.Error.Should()
            .Contain(TestCollectionName)
            .And.Contain("doesn't exist");
    }

    [Test]
    public async Task GetPoints_PointDoesNotExist_NumberId()
    {
        var nonexistentPointId = PointId.Integer(1);

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var getNonexistentPointResult
            = await _qdrantHttpClient.GetPoints(
                TestCollectionName,
                nonexistentPointId.YieldSingle(),
                PayloadPropertiesSelector.None,
                CancellationToken.None);

        getNonexistentPointResult.Status.IsSuccess.Should().BeTrue();
        getNonexistentPointResult.Result.Length.Should().Be(0);
    }

    [Test]
    public async Task GetPoints_PointDoesNotExist_GuidId()
    {
        var nonexistentPointId = PointId.Guid(Guid.NewGuid());

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var getNonexistentPointResult
            = await _qdrantHttpClient.GetPoints(
                TestCollectionName,
                nonexistentPointId.YieldSingle(),
                PayloadPropertiesSelector.None,
                CancellationToken.None);

        getNonexistentPointResult.Status.IsSuccess.Should().BeTrue();
        getNonexistentPointResult.Result.Length.Should().Be(0);
    }

    [Test]
    public async Task UpsertPoint_CollectionDoesNotExist()
    {
        var upsertPointsToNonExistentCollectionResult
            = await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = new List<UpsertPointsRequest.UpsertPoint>()
                    {
                        new(
                            PointId.Integer(1),
                            CreateTestVector(1),
                            "test"
                        )
                    }
                },
                CancellationToken.None);

        upsertPointsToNonExistentCollectionResult.Status.IsSuccess.Should().BeFalse();
        upsertPointsToNonExistentCollectionResult.Status.Error.Should()
            .Contain(TestCollectionName)
            .And.Contain("doesn't exist");
    }

    [Test]
    public async Task UpsertPoint_InvalidPayloadType()
    {
        var upsertPointsResult =
             await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest()
                {
                    Points = new List<UpsertPointsRequest.UpsertPoint>()
                    {
                        new(
                            PointId.Integer(1),
                            CreateTestVector(1),
                            "test"
                        )
                    }
                },
                CancellationToken.None);

        upsertPointsResult.Status.IsSuccess.Should().BeFalse();
        upsertPointsResult.Status.GetErrorMessage().Should().Contain("Format error in JSON body: invalid type: string");
    }

    [Test]
    public async Task DeletePoints_CollectionDoesNotExist()
    {
        var getPointFromNonexistentCollectionResult
            = await _qdrantHttpClient.DeletePoints(
                TestCollectionName,
                PointId.Integer(1).YieldSingle(),
                CancellationToken.None);

        getPointFromNonexistentCollectionResult.Status.IsSuccess.Should().BeFalse();
        getPointFromNonexistentCollectionResult.Status.Error.Should()
            .Contain(TestCollectionName)
            .And.Contain("doesn't exist");
    }

    [Test]
    public async Task DeletePoints_PointDoesNotExist()
    {
        var nonexistentPointId = PointId.Integer(1);

        await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, 100, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None);

        var getNonexistentPointResult
            = await _qdrantHttpClient.DeletePoints(
                TestCollectionName,
                nonexistentPointId.YieldSingle(),
                CancellationToken.None);

        getNonexistentPointResult.Status.IsSuccess.Should().BeTrue();
    }
}
