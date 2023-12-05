// ReSharper disable UnusedMember.Global

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The overall status of the qdrant collection.
/// </summary>
public enum QdrantCollectionStatus
{
    /// <summary>
    /// Means there were some errors the engine could not recover from.
    /// </summary>
    Red,

    /// <summary>
    /// Means all the points are processed and indexing is done.
    /// </summary>
    Green,

    /// <summary>
    /// Means the optimization is still running.
    /// </summary>
    Yellow
}
