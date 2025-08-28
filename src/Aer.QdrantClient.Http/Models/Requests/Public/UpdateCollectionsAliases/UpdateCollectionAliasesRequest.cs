using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the batch update collection aliases request.
/// All operations are executed in the order of their definition.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class UpdateCollectionAliasesRequest
{
    /// <summary>
    /// Points operations to apply.
    /// </summary>
    [JsonInclude]
    internal List<UpdateCollectionAliasOperationBase> Actions { get; } = [];

    /// <summary>
    /// Returns the count of operations defined so far on this request.
    /// </summary>
    public int OperationsCount => Actions.Count;

    /// <summary>
    /// Private ctor to enforce fluent interface usage.
    /// </summary>
    private UpdateCollectionAliasesRequest()
    { }

    /// <summary>
    /// Factory method for fluent interface support.
    /// </summary>
    public static UpdateCollectionAliasesRequest Create()
    {
        return new UpdateCollectionAliasesRequest();
    }

    /// <summary>
    /// Append a "create collection alternative name (alias)" operation to batch.
    /// </summary>
    /// <param name="collectionName">The name of the collection to create alias for.</param>
    /// <param name="aliasName">The collection alias name.</param>
    public UpdateCollectionAliasesRequest CreateAlias(string collectionName, string aliasName)
    {
        var action = new CreateAliasOperation(collectionName, aliasName);

        Actions.Add(action);

        return this;
    }

    /// <summary>
    /// Append a "delete alternative name (alias)" operation to batch.
    /// </summary>
    /// <param name="aliasName">The collection alias name.</param>
    public UpdateCollectionAliasesRequest DeleteAlias(string aliasName)
    {
        var action = new DeleteAliasOperation(aliasName);

        Actions.Add(action);

        return this;
    }

    /// <summary>
    /// Append a "rename collection alternative name (alias)" operation to batch.
    /// </summary>
    /// <param name="oldAliasName">The old collection alias name to change.</param>
    /// <param name="newAliasName">The new collection alias name to change old name to.</param>
    public UpdateCollectionAliasesRequest RenameAlias(string oldAliasName, string newAliasName)
    {
        var action = new RenameAliasOperation(oldAliasName, newAliasName);

        Actions.Add(action);

        return this;
    }
}
