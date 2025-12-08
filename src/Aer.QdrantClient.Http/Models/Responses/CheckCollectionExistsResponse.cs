using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the check collection exists operation response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class CheckCollectionExistsResponse : QdrantResponseBase<CheckCollectionExistsResponse.CollectionExistenceResult>
{
    /// <summary>
    /// Represents the state of existence of a collection.
    /// </summary>
    public sealed class CollectionExistenceResult
    {
        /// <summary>
        /// State of existence of a collection, <c>true</c> means the collection exists,
        /// <c>false</c> means it does not exist.
        /// </summary>
        public bool Exists { set; get; }
    }
}
