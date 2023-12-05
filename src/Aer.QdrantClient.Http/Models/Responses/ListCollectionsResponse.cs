using Aer.QdrantClient.Http.Models.Responses.Base;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the result of listing existing collection names.
/// </summary>
public sealed class ListCollectionsResponse : QdrantResponseBase<ListCollectionsResponse.CollectionNamesUnit>
{
    /// <summary>
    /// Represents a set of existing collection names.
    /// </summary>
    public class CollectionNamesUnit
    {
        /// <summary>
        /// The collection name objects.
        /// </summary>
        public CollectionName[] Collections { get; set; }

        /// <summary>
        /// Represents one existing collecton name.
        /// </summary>
        /// <param name="Name">The name of the collection.</param>

        public record CollectionName(string Name);
    }
}

