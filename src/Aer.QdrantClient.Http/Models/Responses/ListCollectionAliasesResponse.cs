using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the result of listing qdrant collection aliases.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class ListCollectionAliasesResponse
    : QdrantResponseBase<ListCollectionAliasesResponse.CollectionAliasesResult>
{
    /// <summary>
    /// Represents collection aliases result.
    /// </summary>
    public sealed class CollectionAliasesResult
    {
        /// <summary>
        /// The existing collection aliases.
        /// </summary>
        public CollectionAlias[] Aliases { set; get; }

        /// <summary>
        /// The collection aliases by collection names.
        /// A collection can have more than one alias.
        /// </summary>
        public ILookup<string, string> CollectionAliases =>
            Aliases?.ToLookup(a => a.CollectionName, a => a.AliasName);

        /// <summary>
        /// Collection names by alias names.
        /// </summary>
        public Dictionary<string, string> CollectionNamesByAliases =>
            Aliases?.ToDictionary(a => a.AliasName, a => a.CollectionName);
    }

    /// <summary>
    /// Represents a collection alias information.
    /// </summary>
    public sealed class CollectionAlias
    {
        /// <summary>
        /// The collection alias.
        /// </summary>
        public string AliasName { set; get; }

        /// <summary>
        /// The collection original name.
        /// </summary>
        public string CollectionName { set; get; }
    }
}
