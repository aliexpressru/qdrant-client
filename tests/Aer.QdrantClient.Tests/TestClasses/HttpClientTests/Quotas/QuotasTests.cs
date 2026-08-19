using Aer.QdrantClient.Http;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

#if !DEBUG
[Ignore(
    "I didn't find a way to configure both single-node and a multi-node cluster in "
        + "GitHub actions so these tests will run only locally"
)]
#endif
internal class QuotasTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;

    [OneTimeSetUp]
    public void Setup()
    {
        Initialize();
        _qdrantHttpClient = Get2NodeClusterClient(ClusterNode.First);
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
        await ResetStorage(_qdrantHttpClient);
    }

    [Test]
    public async Task SetQuotas()
    {
        OnlyIfVersionAfterOrEqual("1.19.0", "Quotas API is only supported from v1.19");
        
        var quotasResult = await _qdrantHttpClient.GetQuotas(CancellationToken.None);
        
        quotasResult.Status.IsSuccess.Should().BeTrue();
        quotasResult.Result.Config.Enabled.Should().BeFalse();
        quotasResult.Result.Peers.Count.Should().Be(2);

        foreach (var peer in quotasResult.Result.Peers)
        {
            peer.Value.Exceeded.Should().BeFalse();
            peer.Value.DiskUsagePercent.Should().BeGreaterThan(0);
            peer.Value.ResidentMemoryPercent.Should().BeGreaterThan(0);
        }

        var setQuotasResult = await _qdrantHttpClient.SetQuotas(true, 50, 70, 5, CancellationToken.None);
        quotasResult = await _qdrantHttpClient.GetQuotas(CancellationToken.None);
        
        setQuotasResult.Result.Should().BeTrue();
        quotasResult.Result.Peers.Count.Should().Be(2);
        quotasResult.Result.Config.Enabled.Should().BeTrue();
        quotasResult.Result.Config.MaxDiskUsagePercent.Should().Be(70);
        quotasResult.Result.Config.MaxResidentMemoryPercent.Should().Be(50);
        quotasResult.Result.Config.ReleaseMarginPercent.Should().Be(5);
        
        foreach (var peer in quotasResult.Result.Peers)
        {
            peer.Value.Exceeded.Should().BeFalse();
            peer.Value.DiskUsagePercent.Should().BeGreaterThan(0);
            peer.Value.ResidentMemoryPercent.Should().BeGreaterThan(0);
        }
    }
}