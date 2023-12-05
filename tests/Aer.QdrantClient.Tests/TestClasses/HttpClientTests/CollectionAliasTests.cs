using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Tests.Base;

namespace Aer.QdrantClient.Tests.TestClasses.HttpClientTests;

public class CollectionAliasTests : QdrantTestsBase
{
    private QdrantHttpClient _qdrantHttpClient;

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

    [Test]
    public async Task CreateAlias_EmptyRequest()
    {
        var aliasesUpdateAct =
            async () => await _qdrantHttpClient.UpdateCollectionsAliases(
                UpdateCollectionAliasesRequest.Create(),
                CancellationToken.None);

        await aliasesUpdateAct.Should().ThrowAsync<QdrantEmptyBatchRequestException>();
    }

    [Test]
    public async Task CreateAlias_CollectionDoesNotExist()
    {
        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create().CreateAlias(TestCollectionName, "a"),
            CancellationToken.None);

        updateAliasesResult.Status.IsSuccess.Should().BeFalse();
        updateAliasesResult.Status.Error.Should().Contain("Collection `test_collection` doesn't exist!");
    }

    [Test]
    public async Task RenameAlias_AliasDoesNotExist()
    {
        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName);

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .RenameAlias("non-existent-alias", TestCollectionAlias2),
            CancellationToken.None);

        updateAliasesResult.Status.IsSuccess.Should().BeFalse();
        updateAliasesResult.Status.Error.Should().Contain("Alias non-existent-alias does not exists!");
    }

    [Test]
    public async Task DeleteAlias_AliasDoesNotExist()
    {
        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .DeleteAlias(TestCollectionAlias),
            CancellationToken.None);

        // API lets delete non-existent aliases
        updateAliasesResult.Status.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateAlias_TwoAliasesForTwoDifferentCollections()
    {
        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName);

        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName2);

        // create 2 aliases for two different collecitons

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, TestCollectionAlias)
                .CreateAlias(TestCollectionName2, TestCollectionAlias2),
            CancellationToken.None);

        updateAliasesResult.Status.IsSuccess.Should().BeTrue();
        updateAliasesResult.Result.Should().BeTrue();

        var allAliases = await _qdrantHttpClient.ListCollectionAliases(CancellationToken.None);

        allAliases.Status.IsSuccess.Should().BeTrue();
        allAliases.Result.Aliases.Length.Should().Be(2);
    }

    [Test]
    public async Task CreateAlias_SameAliasForDifferentCollections_LastOneWins()
    {
        await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName);

        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName2);

        // create 2 aliases for one colleciton

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, TestCollectionAlias)
                .CreateAlias(TestCollectionName2, TestCollectionAlias),
            CancellationToken.None);

        updateAliasesResult.Status.IsSuccess.Should().BeTrue();
        updateAliasesResult.Result.Should().BeTrue();

        var allAliases = await _qdrantHttpClient.ListCollectionAliases(CancellationToken.None);

        allAliases.Status.IsSuccess.Should().BeTrue();

        allAliases.Result.Aliases.Length.Should().Be(1);
        allAliases.Result.Aliases[0].CollectionName.Should().Be(TestCollectionName2);
        allAliases.Result.Aliases[0].AliasName.Should().Be(TestCollectionAlias);
    }

    [Test]
    public async Task CreateAlias()
    {
        var (_, _, upserPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName);

        // create 2 aliases for one colleciton

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, TestCollectionAlias)
                .CreateAlias(TestCollectionName, TestCollectionAlias2),
            CancellationToken.None);

        updateAliasesResult.Status.IsSuccess.Should().BeTrue();
        updateAliasesResult.Result.Should().BeTrue();

        // list aliases for collection

        var listCollectionAliasesResult = await _qdrantHttpClient.ListCollectionAliases(TestCollectionName, CancellationToken.None);

        listCollectionAliasesResult.Status.IsSuccess.Should().BeTrue();
        listCollectionAliasesResult.Result.Aliases.Length.Should().Be(2); // 2 aliases created

        foreach (var alias in listCollectionAliasesResult.Result.Aliases)
        {
            alias.CollectionName.Should().Be(TestCollectionName);
            alias.AliasName.Should().ContainAny(TestCollectionAlias, TestCollectionAlias2);
        }

        // list all aliases

        var listAllCollectionAliasesResult = await _qdrantHttpClient.ListCollectionAliases(CancellationToken.None);

        listAllCollectionAliasesResult.Status.IsSuccess.Should().BeTrue();
        listAllCollectionAliasesResult.Result.Aliases.Length.Should().Be(2);

        foreach (var alias in listAllCollectionAliasesResult.Result.Aliases)
        {
            alias.CollectionName.Should().Be(TestCollectionName);
            alias.AliasName.Should().ContainAny(TestCollectionAlias, TestCollectionAlias2);
        }

        // try to read data using aliases

        var readPointsUsingCollectionName = await _qdrantHttpClient.GetPoints(
            TestCollectionName,
            upserPointIds,
            withPayload: false,
            CancellationToken.None);

        readPointsUsingCollectionName.EnsureSuccess();
        readPointsUsingCollectionName.Result.Length.Should().Be(upserPointIds.Count);

        var readPointsUsingCollectionAlias1 = await _qdrantHttpClient.GetPoints(
            TestCollectionAlias,
            upserPointIds,
            withPayload: false,
            CancellationToken.None);

        readPointsUsingCollectionAlias1.EnsureSuccess();
        readPointsUsingCollectionAlias1.Result.Length.Should().Be(upserPointIds.Count);

        var readPointsUsingCollectionAlias2 = await _qdrantHttpClient.GetPoints(
            TestCollectionAlias2,
            upserPointIds,
            withPayload: false,
            CancellationToken.None);

        readPointsUsingCollectionAlias2.EnsureSuccess();
        readPointsUsingCollectionAlias2.Result.Length.Should().Be(upserPointIds.Count);
    }

    [Test]
    public async Task CreateAlias_DeleteCollection_AliasShouldBeDeleted()
    {
        await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName);

        // create alias

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, TestCollectionAlias),
            CancellationToken.None);

        updateAliasesResult.Status.IsSuccess.Should().BeTrue();
        updateAliasesResult.Result.Should().BeTrue();

        // list asliases
        var listAllCollectionAliasesResult = await _qdrantHttpClient.ListCollectionAliases(CancellationToken.None);

        listAllCollectionAliasesResult.Status.IsSuccess.Should().BeTrue();
        listAllCollectionAliasesResult.Result.Aliases.Length.Should().Be(1);

        // delete collection

        var deleteCollection = await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);
        deleteCollection.EnsureSuccess();

        // list asliases again
        listAllCollectionAliasesResult = await _qdrantHttpClient.ListCollectionAliases(CancellationToken.None);

        listAllCollectionAliasesResult.Status.IsSuccess.Should().BeTrue();
        listAllCollectionAliasesResult.Result.Aliases.Length.Should().Be(0);
    }

    [Test]
    public async Task CreateAlias_DeleteAlias()
    {
        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName);

        // create 1 alias for colleciton

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, TestCollectionAlias),
            CancellationToken.None);

        updateAliasesResult.EnsureSuccess();

        // get all aliases

        var getAllAliasesResponse =
            await _qdrantHttpClient.ListCollectionAliases(TestCollectionName, CancellationToken.None);

        getAllAliasesResponse.EnsureSuccess();
        getAllAliasesResponse.Result.Aliases.Length.Should().Be(1);

        // create 1 more alias for same collection and delete previous one

        updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, TestCollectionAlias2)
                .DeleteAlias(TestCollectionAlias),
            CancellationToken.None);

        updateAliasesResult.Status.IsSuccess.Should().BeTrue();

        // get all aliases
        getAllAliasesResponse =
            await _qdrantHttpClient.ListCollectionAliases(CancellationToken.None);

        getAllAliasesResponse.EnsureSuccess();

        getAllAliasesResponse.Result.Aliases.Length.Should().Be(1);
        getAllAliasesResponse.Result.Aliases[0].CollectionName.Should().Be(TestCollectionName);
        getAllAliasesResponse.Result.Aliases[0].AliasName.Should().Be(TestCollectionAlias2);
    }

    [Test]
    public async Task CreateAlias_RenameAlias()
    {
        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName);

        // create alias for colleciton

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, TestCollectionAlias),
            CancellationToken.None);

        updateAliasesResult.EnsureSuccess();

        // get all aliases

        var getAllAliasesResponse =
            await _qdrantHttpClient.ListCollectionAliases(TestCollectionName, CancellationToken.None);

        getAllAliasesResponse.EnsureSuccess();
        getAllAliasesResponse.Result.Aliases.Length.Should().Be(1);

        getAllAliasesResponse.Result.Aliases[0].AliasName.Should().Be(TestCollectionAlias);

        // reaname alias

        updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .RenameAlias(TestCollectionAlias, TestCollectionAlias2),
            CancellationToken.None);

        updateAliasesResult.EnsureSuccess();

        getAllAliasesResponse =
            await _qdrantHttpClient.ListCollectionAliases(CancellationToken.None);

        getAllAliasesResponse.Result.Aliases.Length.Should().Be(1);
        getAllAliasesResponse.Result.Aliases[0].AliasName.Should().Be(TestCollectionAlias2);
    }

    [Test]
    public async Task CreateAlias_RenameAlias_DuplicateAlias()
    {
        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName);

        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName2);

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, TestCollectionAlias)
                .CreateAlias(TestCollectionName2, TestCollectionAlias2)
                // rename alias for collection TestCollectionName to be
                // the same as for TestCollectionName2

                // this should overwrite info that colelction TestCollectionName2 has any aliases
                // leaving us with only one alias for collection TestCollectionName
                .RenameAlias(TestCollectionAlias, TestCollectionAlias2),
            CancellationToken.None);

        updateAliasesResult.EnsureSuccess();

        var getAllAliasesResponse =
            await _qdrantHttpClient.ListCollectionAliases(CancellationToken.None);

        getAllAliasesResponse.EnsureSuccess();
        getAllAliasesResponse.Result.Aliases.Length.Should().Be(1);

        getAllAliasesResponse.Result.Aliases[0].CollectionName.Should().Be(TestCollectionName);
        getAllAliasesResponse.Result.Aliases[0].AliasName.Should().Be(TestCollectionAlias2);
    }

    [Test]
    public async Task CreateAlias_RenameAlias_DeleteAlias_NoAliasesLeft()
    {
        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName);

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, TestCollectionAlias2)
                .CreateAlias(TestCollectionName, TestCollectionAlias)
                .RenameAlias(TestCollectionAlias2, TestCollectionAlias)
                .DeleteAlias(TestCollectionAlias),
            CancellationToken.None);

        updateAliasesResult.Status.IsSuccess.Should().BeTrue();

        var getAllAliasesResponse =
            await _qdrantHttpClient.ListCollectionAliases(CancellationToken.None);

        getAllAliasesResponse.EnsureSuccess();
        getAllAliasesResponse.Result.Aliases.Length.Should().Be(0);
    }

    [Test]
    public async Task CreateAlias_RenameAlias_DeleteAlias()
    {
        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName);

        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName2);

        var lastAliasName = "a";

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, TestCollectionAlias)
                .CreateAlias(TestCollectionName2, TestCollectionAlias2)
                .RenameAlias(TestCollectionAlias2, lastAliasName)
                .DeleteAlias(TestCollectionAlias),
            CancellationToken.None);

        updateAliasesResult.Status.IsSuccess.Should().BeTrue();

        var getAllAliasesResponse =
            await _qdrantHttpClient.ListCollectionAliases(CancellationToken.None);

        getAllAliasesResponse.EnsureSuccess();
        getAllAliasesResponse.Result.Aliases.Length.Should().Be(1);

        getAllAliasesResponse.Result.Aliases[0].CollectionName.Should().Be(TestCollectionName2);
        getAllAliasesResponse.Result.Aliases[0].AliasName.Should().Be(lastAliasName);
    }
}
