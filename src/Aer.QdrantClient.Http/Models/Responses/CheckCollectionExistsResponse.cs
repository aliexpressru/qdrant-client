using Aer.QdrantClient.Http.Models.Responses.Base;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the check collection exists operation response.
/// </summary>
public class CheckCollectionExistsResponse : QdrantResponseBase<CheckCollectionExistsResponse.CollectionExistenceResult>
{
    /// <summary>
    /// Represents the state of existence of a collection.
    /// </summary>
    public class CollectionExistenceResult
    {
        /// <summary>
        /// State of existence of a collection, <c>true</c> means the collection exists,
        /// <c>flase</c> means it does not exist.
        /// </summary>
        public bool Exists { set; get; }
    }
}
