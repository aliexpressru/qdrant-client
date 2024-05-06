using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Create alternative name for a collection. Collection will be available under both names for search, retrieve.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class RenameAliasOperation : UpdateCollectionAliasOperationBase
{
    /// <summary>
    /// The rename alias operation definition.
    /// </summary>
    public RenameAliasDefinition RenameAlias { get; }

    /// <summary>
    /// Represents the rename alias operation definition.
    /// </summary>
    public class RenameAliasDefinition
    {
        /// <summary>
        /// The old collection alias name to change.
        /// </summary>
        public required string OldAliasName { get; init; }

        /// <summary>
        /// The new collection alias name to change old name to.
        /// </summary>
        public required string NewAliasName { get; init; }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CreateAliasOperation"/>.
    /// </summary>
    /// <param name="oldAliasName">The old collection alias name to change.</param>
    /// <param name="newAliasName">The new collection alias name to change old name to.</param>
    public RenameAliasOperation(string oldAliasName, string newAliasName)
    {
        RenameAlias = new RenameAliasDefinition()
        {
            OldAliasName = oldAliasName,
            NewAliasName = newAliasName
        };
    }
}
