using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Create alternative name for a collection. Collection will be available under both names for search, retrieve.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class CreateAliasOperation : UpdateCollectionAliasOperationBase
{
    /// <summary>
    /// The create alias operation definition.
    /// </summary>
    public CreateAliasDefinition CreateAlias { get; }

    /// <summary>
    /// Represents the create alias operation definition.
    /// </summary>
    public sealed class CreateAliasDefinition
    {
        /// <summary>
        /// The original collection name.
        /// </summary>
        public required string CollectionName { get; init; }

        /// <summary>
        /// The collection alias name.
        /// </summary>
        public required string AliasName { get; init; }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CreateAliasOperation"/>.
    /// </summary>
    /// <param name="collectionName">The name of the collection to create alias for.</param>
    /// <param name="aliasName">The collection alias name.</param>
    public CreateAliasOperation(string collectionName, string aliasName)
    {
        CreateAlias = new CreateAliasDefinition()
        {
            CollectionName = collectionName,
            AliasName = aliasName
        };
    }
}
