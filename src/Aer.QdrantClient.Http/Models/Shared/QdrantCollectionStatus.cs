// ReSharper disable UnusedMember.Global

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The overall status of the qdrant collection.
/// </summary>
public enum QdrantCollectionStatus
{
    /// <summary>
    /// An error occurred which the engine could not recover from.
    /// </summary>
    Red,

    /// <summary>
    /// All the points are processed and indexing is done, collection is ready.
    /// </summary>
    Green,

    /// <summary>
    /// Optimization process is still running.
    /// </summary>
    Yellow,

    /// <summary>
    /// Optimizations are pending after restart.
    /// </summary>
    Grey
}
