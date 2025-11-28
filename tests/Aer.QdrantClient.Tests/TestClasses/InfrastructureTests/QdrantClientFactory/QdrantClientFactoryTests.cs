using Aer.QdrantClient.Http.Abstractions;
using Aer.QdrantClient.Http.Configuration;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests.QdrantClientFactory;

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
    public async Task GetClientByName()
    {
        var factory = ServiceProvider.GetRequiredService<IQdrantClientFactory>();

        factory.AddClientConfiguration(
            "TestClient1",
            apiKey: "test",
            httpAddress: "http://localhost-test1:6334",
            enableCompression: true);

        var firstClient = factory.CreateClient(FirstClientName);
        var secondClient = factory.CreateClient(SecondClientName);

        factory.AddClientConfiguration(
            SecondClientName, // Override pre-registered client
            apiKey: "test",
            httpAddress: "http://localhost-test2:6334",
            enableCompression: true);

        var secondClientUpdated = factory.CreateClient(SecondClientName);

        var thirdClient = factory.CreateClient("TestClient1");

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

        var firstClient = factory.CreateClient("TestClient1");
        var secondClient = factory.CreateClient("TestClient2");
        var thirdClient = factory.CreateClient("TestClient3");
        var fourthClient = factory.CreateClient("TestClient4");

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

        var firstClient = factory.CreateClient(
            apiKey: "test",
            httpAddress: "http://localhost-test1:6334",
            enableCompression: true);

        var secondClient = factory.CreateClient(
            new QdrantClientSettings()
            {
                ApiKey = "test",
                HttpAddress = "http://localhost-test2:6334",
                EnableCompression = true
            });

        var thirdClient = factory.CreateClient(
            new Uri("http://localhost-test3:6334"),
            apiKey: "test",
            enableCompression: true);

        var fourthClient = factory.CreateClient(
            host: "localhost-test4",
            port: 6335,
            apiKey: "test",
            useHttps: true,
            enableCompression: true);

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
