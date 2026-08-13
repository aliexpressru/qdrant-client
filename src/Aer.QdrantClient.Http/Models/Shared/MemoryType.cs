namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The type of the memory placement of data structures such as vector, vector index, payload and payload index.
/// </summary>
public enum MemoryType
{
    /// <summary>
    /// Qdrant doesn’t pre-load the data into RAM.
    /// Startup is faster and uses less memory, but the first access to any page requires a disk read until the OS caches it.
    /// </summary>
    Cold,

    /// <summary>
    /// Qdrant pre-loads the data into the disk cache when it starts up, so the first request is fast.
    /// Under memory pressure, the operating system can evict this data if it decides another component’s data is used more often.
    /// </summary>
    Cached,

    /// <summary>
    /// Qdrant loads the data onto the heap and never evicts it. Requests stay fast, but the structure must fit in RAM at all times.
    /// Because it allocates data on the heap, it’s only available for structures that support a heap-backed in-RAM representation.
    /// It is not supported for dense vectors and payloads.
    /// </summary>
    Pinned
}
