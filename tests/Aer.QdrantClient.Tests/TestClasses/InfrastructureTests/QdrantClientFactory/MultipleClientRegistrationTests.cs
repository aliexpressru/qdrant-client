using Aer.QdrantClient.Http.Abstractions;
using Aer.QdrantClient.Http.DependencyInjection;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests.QdrantClientFactory;

public class MultipleClientRegistrationTests : QdrantTestsBase
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
    public async Task GetClientByName_NonExistentName()
    {
        var factory = ServiceProvider.GetRequiredService<IQdrantClientFactory>();

        var nonExistentClientName = "NonExistentClientName";

        Action act = () => factory.CreateClient(nonExistentClientName);

        act.Should().Throw<QdrantNamedQdrantClientNotFound>()
            .WithMessage($"No Qdrant HTTP client registered with the name '{nonExistentClientName}'*");
    }

    [Test]
    public async Task GetClientByName()
    {
        var factory = ServiceProvider.GetRequiredService<IQdrantClientFactory>();

        var firstClient = factory.CreateClient(FirstClientName);
        var secondClient = factory.CreateClient(SecondClientName);

        firstClient.BaseAddress.Should().NotBeNull();
        secondClient.BaseAddress.Should().NotBeNull();
        firstClient.BaseAddress.Should().NotBe(secondClient.BaseAddress);
    }
}
