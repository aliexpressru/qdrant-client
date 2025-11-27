using Aer.QdrantClient.Http.Abstractions;
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
        secondClient.BaseAddress.Should().Be(secondClientUpdated.BaseAddress);
        firstClient.BaseAddress.Should().NotBe(thirdClient.BaseAddress);
    }
}
