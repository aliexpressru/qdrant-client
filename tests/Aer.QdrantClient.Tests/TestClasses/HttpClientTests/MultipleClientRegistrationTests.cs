using Aer.QdrantClient.Http.Abstractions;
using Aer.QdrantClient.Http.DependencyInjection;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class MultipleClientRegistrationTests : QdrantTestsBase
{
    [OneTimeSetUp]
    public void Setup()
    {
        Initialize(isAddMultipleQdrantClients: true);
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

        firstClient.Should().NotBeNull();
        secondClient.Should().NotBeNull();

        firstClient.Should().NotBeSameAs(secondClient);

        // We use created collections as markers that the clients are indeed different

        await PrepareCollection(firstClient, TestCollectionName);
        await PrepareCollection(secondClient, TestCollectionName2);

        // Should be present
        var firstCollectionFirstClient = await firstClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);
        // Created on another client, should not be present
        var secondCollectionFirstClient = await firstClient.GetCollectionInfo(TestCollectionName2, CancellationToken.None);

        // Creted on another client, should not be present
        var firstCollectionSecondClient = await secondClient.GetCollectionInfo(TestCollectionName, CancellationToken.None);
        // Should be present
        var secondCollectionSecondClient = await secondClient.GetCollectionInfo(TestCollectionName2, CancellationToken.None);

        firstCollectionFirstClient.Result.Should().NotBeNull();
        secondCollectionSecondClient.Result.Should().NotBeNull();

        // These collections were created on another client, should not be present

        secondCollectionFirstClient.Status.IsSuccess.Should().BeFalse();
        secondCollectionFirstClient.Result.Should().BeNull();

        firstCollectionSecondClient.Status.IsSuccess.Should().BeFalse();
        firstCollectionSecondClient.Result.Should().BeNull();

        await ResetStorage(firstClient);
        await ResetStorage(secondClient);
    }
}
