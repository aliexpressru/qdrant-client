// ReSharper disable UnusedMember.Global

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents the operation ordering guarantee.
/// </summary>
/// <remarks> Can be used with update and delete operations to ensure that the operations are executed
/// in the same order on all replicas. If this option is used, qdrant will route the operation to the
/// leader replica of the shard and wait for the response before responding to the client.
/// This option is useful to avoid data inconsistency in case of concurrent updates of the same documents.
/// This options is preferred if read operations are more frequent than update and if search performance is critical.</remarks>
public enum OrderingType
{
    /// <summary>
    /// Ordering does not provide any additional guarantees, so write operations can be freely reordered.
    /// </summary>
    Weak,

    /// <summary>
    /// Ordering serializes all write operations through a dynamically elected leader, which might cause
    /// minor inconsistencies in case of leader change.
    /// </summary>
    Medium,

    /// <summary>
    /// ordering serializes all write operations through the permanent leader, which provides strong
    /// consistency, but write operations may be unavailable if the leader is down.
    /// </summary>
    Strong
}
