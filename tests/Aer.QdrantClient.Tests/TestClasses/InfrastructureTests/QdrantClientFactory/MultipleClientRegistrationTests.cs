using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Abstractions;
using Aer.QdrantClient.Http.DependencyInjection;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Infrastructure;

namespace Aer.QdrantClient.Tests.TestClasses.InfrastructureTests.QdrantClientFactory;

internal class MultipleClientRegistrationTests : QdrantTestsBase
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

    [Test]
    public async Task UninitializedClient()
    {
        var uninitializedQdrantClient = new QdrantHttpClient();

        var getUnderlyingClientAct = () => uninitializedQdrantClient.ApiClient;
        getUnderlyingClientAct.Should().Throw<QdrantClientUninitializedException>();

        var makeCallWithUninitializedClientAct = async () => await uninitializedQdrantClient.GetInstanceDetails(CancellationToken.None);
        await makeCallWithUninitializedClientAct.Should().ThrowAsync<QdrantClientUninitializedException>();
    }

    [Test]
    public async Task CustomClient()
    {
        var httpClientFactory = ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var customHttpClient = httpClientFactory.CreateClient(FirstClientName);

        var customQdrantClient = new CustomQdrantHttpClient(customHttpClient);

        customQdrantClient.ApiClient.Should().NotBeNull();

        var instanceDetailsResponse = await customQdrantClient.GetInstanceDetails(CancellationToken.None);
        instanceDetailsResponse.Should().NotBeNull();
        instanceDetailsResponse.Title.Should().NotBeNullOrEmpty();
        instanceDetailsResponse.Version.Should().NotBeNullOrEmpty();
        instanceDetailsResponse.Commit.Should().NotBeNullOrEmpty();
    }
}
