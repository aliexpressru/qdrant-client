using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Delete alternative name for a collection.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class DeleteAliasOperation : UpdateCollectionAliasOperationBase
{
    /// <summary>
    /// The delete alias operation definition.
    /// </summary>
    public DeleteAliasDefinition DeleteAlias { get; }

    /// <summary>
    /// Represents the delete alias operation definition.
    /// </summary>
    public sealed class DeleteAliasDefinition
    {
        /// <summary>
        /// The collection alias name.
        /// </summary>
        public required string AliasName { get; init; }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DeleteAliasOperation"/>.
    /// </summary>
    /// <param name="aliasName">The alias name to delete.</param>
    public DeleteAliasOperation(string aliasName)
    {
        DeleteAlias = new DeleteAliasDefinition()
        {
            AliasName = aliasName
        };
    }
}
