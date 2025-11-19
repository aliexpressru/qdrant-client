using Aer.QdrantClient.Http.Models.Responses;

namespace Aer.QdrantClient.Http.Abstractions;

public partial interface IQdrantHttpClient
{
    /// <summary>
    /// A compound operation that lists all snapshots for all collections and shards in the storage.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="includeStorageSnapshots">
    /// If set to <c>true</c> includes storage snapshots to listing.
    /// Default value is <c>false</c> because multi-node snapshot tests are discouraged.
    /// </param>
    Task<ListSnapshotsResponse> ListAllSnapshots(
        CancellationToken cancellationToken,
        bool includeStorageSnapshots = false);

    /// <summary>
    /// A compound operation that deletes all existing storage snapshots.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<DefaultOperationResponse> DeleteAllStorageSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult = true);

    /// <summary>
    /// A compound operation that deletes all existing collection snapshots.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<DefaultOperationResponse> DeleteAllCollectionSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult = true);

    /// <summary>
    /// A compound operation that deletes all existing collection shard snapshots.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    Task<DefaultOperationResponse> DeleteAllCollectionShardSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult = true);
}
