namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Additional value modifications for sparse vectors.
/// </summary>
public enum SparseVectorModifier
{
    /// <summary>
    /// No value modifications,
    /// </summary>
    None,

    /// <summary>
    /// Inverse document frequency value modifier. Consider how often an item occurs in a collection.
    /// The less frequently an item appears in a collection, the more important it is in a search.
    /// </summary>
    Idf
}
