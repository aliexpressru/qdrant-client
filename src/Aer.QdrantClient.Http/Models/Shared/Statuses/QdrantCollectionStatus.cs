namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The overall status of the qdrant collection.
/// </summary>
public enum QdrantCollectionStatus
{
    /// <summary>
    /// An error occurred which the engine could not recover from.
    /// </summary>
    Red = 0,

    /// <summary>
    /// All the points are processed and indexing is done, collection is ready.
    /// </summary>
    Green = 1,

    /// <summary>
    /// Optimization process is still running.
    /// </summary>
    Yellow = 2,

    /// <summary>
    /// Optimizations are pending after restart.
    /// </summary>
    Grey = 3
}
