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
    public async Task GetClientByName_NonExistsntName()
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

        await ResetStorage(firstClient);
        await ResetStorage(secondClient);

        // We use created collections as markers that the clients are indeed different

        await PrepareCollection(firstClient, TestCollectionName);
        await PrepareCollection(secondClient, TestCollectionName2);

        var firstClientCollections = (await firstClient.ListCollections(CancellationToken.None)).EnsureSuccess();
        var secondClientCollections = (await secondClient.ListCollections(CancellationToken.None)).EnsureSuccess();

        firstClientCollections.Collections.Length.Should().Be(1);
        secondClientCollections.Collections.Length.Should().Be(1);

        firstClientCollections.Collections.Should().ContainSingle(c => c.Name == TestCollectionName);
        firstClientCollections.Collections.Should().NotContain(c => c.Name == TestCollectionName2);

        secondClientCollections.Collections.Should().ContainSingle(c => c.Name == TestCollectionName2);
        secondClientCollections.Collections.Should().NotContain(c => c.Name == TestCollectionName);

        // Cleanup

        await ResetStorage(firstClient);
        await ResetStorage(secondClient);
    }
}
