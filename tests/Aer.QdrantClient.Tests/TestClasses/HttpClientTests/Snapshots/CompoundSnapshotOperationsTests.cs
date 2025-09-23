using Aer.QdrantClient.Http;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests.Snapshots;

[Ignore("WiP on Snapshot API support")]
public class CompoundSnapshotOperationsTests : QdrantTestsBase
{
	// NOTE: since we don't have a cluster in test and thus have only one shard
	// these tests basically repeat the tests from CollectionSnapshotTests but using shard methods
	private QdrantHttpClient _qdrantHttpClient;

	// since we don't have a cluster in test and thus have only one shard which is always 0
	private const int SINGLE_SHARD_ID = 0;

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

}
