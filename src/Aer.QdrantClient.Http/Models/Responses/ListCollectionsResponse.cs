using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the result of listing existing collection names.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class ListCollectionsResponse : QdrantResponseBase<ListCollectionsResponse.CollectionNamesUnit>
{
    /// <summary>
    /// Represents a set of existing collection names.
    /// </summary>
    public sealed class CollectionNamesUnit
    {
        /// <summary>
        /// The collection name objects.
        /// </summary>
        public CollectionName[] Collections { get; set; }

        /// <summary>
        /// Represents one existing collection name.
        /// </summary>
        /// <param name="Name">The name of the collection.</param>

        public record CollectionName(string Name);
    }
}

