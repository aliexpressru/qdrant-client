using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the result of listing qdrant collection aliases.
/// </summary>
public sealed class ListCollectionAliasesResponse : QdrantResponseBase<ListCollectionAliasesResponse.CollectionAliasesResult>
{
    /// <summary>
    /// Represents colleciton aliases result.
    /// </summary>
    public class CollectionAliasesResult
    {
        /// <summary>
        /// The existsding collection aliases.
        /// </summary>
        public CollectionAlias[] Aliases { set; get; }
    }
}
