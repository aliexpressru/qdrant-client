using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Abstractions;
using Aer.QdrantClient.Http.Configuration;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests.QdrantClientResolution;

internal class QdrantClientFactoryTests : QdrantTestsBase
{
    [OneTimeSetUp]
    public void Setup()
    {
        Initialize(isAddMultipleQdrantClients: true);
    }

    [SetUp]
    public async Task BeforeEachTest()
    {
    }

    [Test]
    public async Task UninitializedClient()
    {
        var uninitializedQdrantClient = new QdrantHttpClient();

        var getUnderlyingClientAct = async () => await uninitializedQdrantClient.GetApiClient(null);
        await getUnderlyingClientAct.Should().ThrowAsync<QdrantClientUninitializedException>();

        var makeCallWithUninitializedClientAct = async () => await uninitializedQdrantClient.GetInstanceDetails(CancellationToken.None);
        await makeCallWithUninitializedClientAct.Should().ThrowAsync<QdrantClientUninitializedException>();
    }

    [Test]
    public async Task GetClientByName_NonExistentName()
    {
        var factory = ServiceProvider.GetRequiredService<IQdrantClientFactory>();

        var nonExistentClientName = "NonExistentClientName";

        Action act = () => factory.CreateClient(nonExistentClientName);

        act.Should().Throw<QdrantNamedQdrantClientNotFound>()
            .WithMessage($"No Qdrant HTTP client registered with the name '{nonExistentClientName}'*");
    }

    [Test]
    public async Task GetClientByName_FromAppSettings()
    {
        var factory = ServiceProvider.GetRequiredService<IQdrantClientFactory>();

        var firstClient = factory.CreateClient(FirstClientName);
        var secondClient = factory.CreateClient(SecondClientName);

        var firstClientBaseAddress = (await firstClient.GetApiClient(null)).BaseAddress;
        var secondClientBaseAddress = (await secondClient.GetApiClient(null)).BaseAddress;

        firstClientBaseAddress.Should().NotBeNull();
        secondClientBaseAddress.Should().NotBeNull();

        firstClientBaseAddress.Should().NotBe(secondClientBaseAddress);
    }

    [Test]
    public async Task GetApiClientByName_FromAppSettings()
    {
        var factory = ServiceProvider.GetRequiredService<IQdrantClientFactory>();

        var firstClient = factory.GetQdrantApiClient(FirstClientName);
        var secondClient = factory.GetQdrantApiClient(SecondClientName);

        var firstClientBaseAddress = firstClient.BaseAddress;
        var secondClientBaseAddress = secondClient.BaseAddress;

        firstClientBaseAddress.Should().NotBeNull();
        secondClientBaseAddress.Should().NotBeNull();

        firstClientBaseAddress.Should().NotBe(secondClientBaseAddress);
    }

    [Test]
    public async Task GetClientByName_ManualConfiguration()
    {
        var factory = ServiceProvider.GetRequiredService<IQdrantClientFactory>();

        factory.AddClientConfiguration(
            "TestClient1",
            apiKey: "test",
            httpAddress: "http://localhost-test1:6334",
            enableCompression: true);

        var firstClient = await (factory.CreateClient(FirstClientName)).GetApiClient(collectionOrClusterName: null);
        var secondClient = await (factory.CreateClient(SecondClientName)).GetApiClient(collectionOrClusterName: null);

        factory.AddClientConfiguration(
            SecondClientName, // Override pre-registered client
            apiKey: "test",
            httpAddress: "http://localhost-test2:6334",
            enableCompression: true);

        var secondClientUpdated = await (factory.CreateClient(SecondClientName)).GetApiClient(collectionOrClusterName: null);

        var thirdClient = await (factory.CreateClient("TestClient1")).GetApiClient(collectionOrClusterName: null);

        firstClient.BaseAddress.Should().NotBeNull();
        secondClient.BaseAddress.Should().NotBeNull();
        secondClientUpdated.BaseAddress.Should().NotBeNull();
        thirdClient.BaseAddress.Should().NotBeNull();

        firstClient.BaseAddress.Should().NotBe(secondClient.BaseAddress);
        secondClient.BaseAddress.Should().NotBe(secondClientUpdated.BaseAddress);
        firstClient.BaseAddress.Should().NotBe(thirdClient.BaseAddress);
    }

    [Test]
    public async Task GetClientByName_AllConfigVariants()
    {
        var factory = ServiceProvider.GetRequiredService<IQdrantClientFactory>();

        factory.AddClientConfiguration(
            "TestClient1",
            apiKey: "test",
            httpAddress: "http://localhost-test1:6334",
            enableCompression: true);

        factory.AddClientConfiguration(
            "TestClient2",
            new QdrantClientSettings()
            {
                ApiKey = "test",
                HttpAddress = "http://localhost-test2:6334",
                EnableCompression = true
            });

        factory.AddClientConfiguration(
            "TestClient3",
            new Uri("http://localhost-test3:6334"),
            apiKey: "test",
            enableCompression: true);

        factory.AddClientConfiguration(
            "TestClient4",
            host: "localhost-test4",
            port: 6335,
            apiKey: "test",
            useHttps: true,
            enableCompression: true);

        var firstClient = await (factory.CreateClient("TestClient1")).GetApiClient(collectionOrClusterName: null);
        var secondClient = await (factory.CreateClient("TestClient2")).GetApiClient(collectionOrClusterName: null);
        var thirdClient = await (factory.CreateClient("TestClient3")).GetApiClient(collectionOrClusterName: null);
        var fourthClient = await (factory.CreateClient("TestClient4")).GetApiClient(collectionOrClusterName: null);

        firstClient.BaseAddress.Should().NotBeNull();
        firstClient.BaseAddress.Should().Be("http://localhost-test1:6334");

        secondClient.BaseAddress.Should().NotBeNull();
        secondClient.BaseAddress.Should().Be("http://localhost-test2:6334");

        thirdClient.BaseAddress.Should().NotBeNull();
        thirdClient.BaseAddress.Should().Be("http://localhost-test3:6334");

        fourthClient.BaseAddress.Should().NotBeNull();
        fourthClient.BaseAddress.Should().Be("https://localhost-test4:6335");
    }

    [Test]
    public async Task CreateClient()
    {
        var factory = ServiceProvider.GetRequiredService<IQdrantClientFactory>();

        var firstClient = await (factory.CreateClient(
            apiKey: "test",
            httpAddress: "http://localhost-test1:6334",
            enableCompression: true)).GetApiClient(collectionOrClusterName: null);

        var secondClient = await factory.CreateClient(
            new QdrantClientSettings()
            {
                ApiKey = "test",
                HttpAddress = "http://localhost-test2:6334",
                EnableCompression = true
            })
            .GetApiClient(collectionOrClusterName: null);

        var thirdClient = await factory.CreateClient(
            new Uri("http://localhost-test3:6334"),
            apiKey: "test",
            enableCompression: true)
            .GetApiClient(collectionOrClusterName: null);

        var fourthClient = await factory.CreateClient(
            host: "localhost-test4",
            port: 6335,
            apiKey: "test",
            useHttps: true,
            enableCompression: true)
            .GetApiClient(collectionOrClusterName: null);

        firstClient.BaseAddress.Should().NotBeNull();
        firstClient.BaseAddress.Should().Be("http://localhost-test1:6334");

        secondClient.BaseAddress.Should().NotBeNull();
        secondClient.BaseAddress.Should().Be("http://localhost-test2:6334");

        thirdClient.BaseAddress.Should().NotBeNull();
        thirdClient.BaseAddress.Should().Be("http://localhost-test3:6334");

        fourthClient.BaseAddress.Should().NotBeNull();
        fourthClient.BaseAddress.Should().Be("https://localhost-test4:6335");
    }
}
