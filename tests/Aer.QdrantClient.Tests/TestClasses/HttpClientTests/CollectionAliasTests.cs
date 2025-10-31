using Aer.QdrantClient.Http;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public;
using Aer.QdrantClient.Tests.Base;
using Aer.QdrantClient.Tests.Model;

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

        // create 2 aliases for two different collections

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, TestCollectionAlias)
                .CreateAlias(TestCollectionName2, TestCollectionAlias2),
            CancellationToken.None);

        updateAliasesResult.Status.IsSuccess.Should().BeTrue();
        updateAliasesResult.Result.Should().BeTrue();

        var allAliases = await _qdrantHttpClient.ListAllAliases(CancellationToken.None);

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

        // create same alias for two different collections

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, TestCollectionAlias)
                .CreateAlias(TestCollectionName2, TestCollectionAlias),
            CancellationToken.None);

        updateAliasesResult.Status.IsSuccess.Should().BeTrue();
        updateAliasesResult.Result.Should().BeTrue();

        var allAliases = await _qdrantHttpClient.ListAllAliases(CancellationToken.None);

        allAliases.Status.IsSuccess.Should().BeTrue();

        allAliases.Result.Aliases.Length.Should().Be(1);
        allAliases.Result.Aliases[0].CollectionName.Should().Be(TestCollectionName2);
        allAliases.Result.Aliases[0].AliasName.Should().Be(TestCollectionAlias);
    }

    [Test]
    public async Task CreateAlias_SameAliasForDifferentCollections_TwoDifferentOperations_LastOneWins()
    {
        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName);

        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName2);

        // create alias for the first collection
        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, TestCollectionAlias),
            CancellationToken.None);

        updateAliasesResult.EnsureSuccess().Should().BeTrue();

        var allAliases = (await _qdrantHttpClient.ListAllAliases(CancellationToken.None)).EnsureSuccess();

        allAliases.Aliases.Length.Should().Be(1);
        allAliases.CollectionNamesByAliases[TestCollectionAlias].Should().Be(TestCollectionName);

        // create the same alias for the second collection
        updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName2, TestCollectionAlias),
            CancellationToken.None);

        updateAliasesResult.EnsureSuccess().Should().BeTrue();

        allAliases = (await _qdrantHttpClient.ListAllAliases(CancellationToken.None)).EnsureSuccess();

        allAliases.Aliases.Length.Should().Be(1);
        allAliases.CollectionNamesByAliases[TestCollectionAlias].Should().Be(TestCollectionName2);
    }

    [Test]
    public async Task CreateAlias()
    {
        var (_, _, upsertPointIds) =
            await PrepareCollection(
                _qdrantHttpClient,
                TestCollectionName);

        // create 2 aliases for one collection

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

        var listAllCollectionAliasesResult = await _qdrantHttpClient.ListAllAliases(CancellationToken.None);

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
            upsertPointIds,
            withPayload: false,
            CancellationToken.None);

        readPointsUsingCollectionName.EnsureSuccess();
        readPointsUsingCollectionName.Result.Length.Should().Be(upsertPointIds.Count);

        var readPointsUsingCollectionAlias1 = await _qdrantHttpClient.GetPoints(
            TestCollectionAlias,
            upsertPointIds,
            withPayload: false,
            CancellationToken.None);

        readPointsUsingCollectionAlias1.EnsureSuccess();
        readPointsUsingCollectionAlias1.Result.Length.Should().Be(upsertPointIds.Count);

        var readPointsUsingCollectionAlias2 = await _qdrantHttpClient.GetPoints(
            TestCollectionAlias2,
            upsertPointIds,
            withPayload: false,
            CancellationToken.None);

        readPointsUsingCollectionAlias2.EnsureSuccess();
        readPointsUsingCollectionAlias2.Result.Length.Should().Be(upsertPointIds.Count);
    }

    [Test]
    public async Task CreateAlias_RestrictedSymbols_Success()
    {
        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName);

        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName2);

        var testAlias1 = "test/alias";
        var testAlias2 = "test alias";

        // create aliases with restricted symbols

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .CreateAlias(TestCollectionName, testAlias1)
                .CreateAlias(TestCollectionName2, testAlias2),
            CancellationToken.None);

        updateAliasesResult.EnsureSuccess();

        var listAllCollectionAliasesResult = (await _qdrantHttpClient.ListAllAliases(CancellationToken.None)).EnsureSuccess();

        listAllCollectionAliasesResult.CollectionNamesByAliases.Should().ContainKey(testAlias1);
        listAllCollectionAliasesResult.CollectionNamesByAliases.Should().ContainKey(testAlias2);

        listAllCollectionAliasesResult.CollectionAliases[TestCollectionName].Count().Should().Be(1);
        listAllCollectionAliasesResult.CollectionAliases[TestCollectionName2].Count().Should().Be(1);
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

        // list aliases
        var listAllCollectionAliasesResult = await _qdrantHttpClient.ListAllAliases(CancellationToken.None);

        listAllCollectionAliasesResult.Status.IsSuccess.Should().BeTrue();
        listAllCollectionAliasesResult.Result.Aliases.Length.Should().Be(1);

        // delete collection

        var deleteCollection = await _qdrantHttpClient.DeleteCollection(TestCollectionName, CancellationToken.None);
        deleteCollection.EnsureSuccess();

        // list aliases again
        listAllCollectionAliasesResult = await _qdrantHttpClient.ListAllAliases(CancellationToken.None);

        listAllCollectionAliasesResult.Status.IsSuccess.Should().BeTrue();
        listAllCollectionAliasesResult.Result.Aliases.Length.Should().Be(0);
    }

    [Test]
    public async Task CreateAlias_DeleteAlias()
    {
        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName);

        // create 1 alias for collection

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
            await _qdrantHttpClient.ListAllAliases(CancellationToken.None);

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

        // create alias for collection

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

        // rename alias

        updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            UpdateCollectionAliasesRequest.Create()
                .RenameAlias(TestCollectionAlias, TestCollectionAlias2),
            CancellationToken.None);

        updateAliasesResult.EnsureSuccess();

        getAllAliasesResponse =
            await _qdrantHttpClient.ListAllAliases(CancellationToken.None);

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

                // this should overwrite info that collection TestCollectionName2 has any aliases
                // leaving us with only one alias for collection TestCollectionName
                .RenameAlias(TestCollectionAlias, TestCollectionAlias2),
            CancellationToken.None);

        updateAliasesResult.EnsureSuccess();

        var getAllAliasesResponse =
            await _qdrantHttpClient.ListAllAliases(CancellationToken.None);

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
            await _qdrantHttpClient.ListAllAliases(CancellationToken.None);

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
            await _qdrantHttpClient.ListAllAliases(CancellationToken.None);

        getAllAliasesResponse.EnsureSuccess();
        getAllAliasesResponse.Result.Aliases.Length.Should().Be(1);

        getAllAliasesResponse.Result.Aliases[0].CollectionName.Should().Be(TestCollectionName2);
        getAllAliasesResponse.Result.Aliases[0].AliasName.Should().Be(lastAliasName);
    }

    [Test]
    public async Task ListAliases()
    {
        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName);

        await PrepareCollection(
            _qdrantHttpClient,
            TestCollectionName2);

        Dictionary<string, List<string>> collectionAliases = new()
        {
            [TestCollectionName] = ["alias 1-1", "alias 1-2", "alias 1-3"],
            [TestCollectionName2] = ["alias 2-1", "alias 2-2", "alias 2-3"]
        };

        var allAliasesCount = collectionAliases.Values.Sum(v => v.Count);

        var request = UpdateCollectionAliasesRequest.Create();

        foreach (var (collection, aliases) in collectionAliases)
        {
            foreach (var alias in aliases)
            {
                request.CreateAlias(collection, alias);
            }
        }

        var updateAliasesResult = await _qdrantHttpClient.UpdateCollectionsAliases(
            request,
            CancellationToken.None);

        updateAliasesResult.EnsureSuccess();

        var allAliases = (await _qdrantHttpClient.ListAllAliases(CancellationToken.None)).EnsureSuccess();

        allAliases.Aliases.Length.Should().Be(allAliasesCount);

        allAliases.CollectionNamesByAliases.Count.Should().Be(allAliasesCount);

        foreach (var (collection, aliases) in collectionAliases)
        {
            var expectedCollectionName = collection;
            foreach (var expectedAlias in aliases)
            {
                allAliases.CollectionNamesByAliases.Should().ContainKey(expectedAlias);
                var gotCollectionName = allAliases.CollectionNamesByAliases[expectedAlias];

                gotCollectionName.Should().Be(expectedCollectionName);
            }
        }

        allAliases.CollectionAliases.Count.Should().Be(collectionAliases.Count);

        foreach (var (collection, expectedAliases) in collectionAliases)
        {
            allAliases.CollectionAliases.Contains(collection).Should().BeTrue();
            allAliases.CollectionAliases[collection].Should().BeEquivalentTo(expectedAliases);
        }
    }
}
