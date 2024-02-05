// ReSharper disable UnusedMember.Global

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Shard transfer method.
/// </summary>
public enum ShardTransferMethod
{
    /// <summary>
    /// Transfer shard by streaming just its records to the target node in batches.
    /// This is the default shard transfer method.
    /// </summary>
    StreamRecords,

    /// <summary>
    /// Transfer shard including its index and quantized data by utilizing a snapshot automatically.
    /// </summary>
    Snapshot
}
