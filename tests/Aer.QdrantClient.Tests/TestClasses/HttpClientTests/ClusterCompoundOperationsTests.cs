using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Http.Models.Shared;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

#if !DEBUG
[Ignore("I didn't find a way to configure both single-node deployment and 3 node cluster in "
+"GitHub actions so these tests will run only locally")]
#endif
public class ClusterCompoundOperationsTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();
        _qdrantHttpClient = GetClusterClient();
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage(_qdrantHttpClient, isDeleteCollectionFiles: false);
    }

    [Test]
    public async Task TestGetPeerInfoByUriSubstring_Success()
    {

    }

    [Test]
    public async Task TestIsPeerEmpty_Success()
    {
        const uint vectorSize = 10;

        var clusterInfo = (await _qdrantHttpClient.GetClusterInfo(CancellationToken.None)).EnsureSuccess();

        var firstPeerUri = clusterInfo.Peers.First().Value.Uri;

        var checkPeerEmptyResult = await _qdrantHttpClient.CheckIsPeerEmpty(firstPeerUri, CancellationToken.None);

        checkPeerEmptyResult.Status.IsSuccess.Should().BeTrue();
        checkPeerEmptyResult.Result.Should().BeTrue();

        (await _qdrantHttpClient.CreateCollection(
            TestCollectionName,
            new CreateCollectionRequest(VectorDistanceMetric.Dot, vectorSize, isServeVectorsFromDisk: true)
            {
                OnDiskPayload = true
            },
            CancellationToken.None)).EnsureSuccess();

        var vectorCount = 10;

        var upsertPoints = new List<UpsertPointsRequest<TestPayload>.UpsertPoint>();
        for (int i = 0; i < vectorCount; i++)
        {
            upsertPoints.Add(
                new(
                    PointId.Integer((ulong) i),
                    CreateTestFloatVector(vectorSize),
                    i
                )
            );
        }

        (await _qdrantHttpClient.UpsertPoints(
                TestCollectionName,
                new UpsertPointsRequest<TestPayload>()
                {
                    Points = upsertPoints
                },
                CancellationToken.None,
                isWaitForResult: true,
                ordering: OrderingType.Strong)).EnsureSuccess();

        await _qdrantHttpClient.EnsureCollectionReady(TestCollectionName, CancellationToken.None);

        checkPeerEmptyResult = await _qdrantHttpClient.CheckIsPeerEmpty(firstPeerUri, CancellationToken.None);

        checkPeerEmptyResult.Status.IsSuccess.Should().BeTrue();
        checkPeerEmptyResult.Result.Should().BeFalse();
    }

    [Test]
    public async Task TestDrainPeer_Success()
    { }

    [Test]
    public async Task TestReplicateShardsToClusterNode_Success()
    { }
}
