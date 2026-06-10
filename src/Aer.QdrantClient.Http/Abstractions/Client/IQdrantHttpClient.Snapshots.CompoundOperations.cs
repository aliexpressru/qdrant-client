using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Responses.CompoundOperations;
using Aer.QdrantClient.Http.Models.Shared;
using Microsoft.Extensions.Logging;

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
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<ListSnapshotsResponse> ListAllSnapshots(
        CancellationToken cancellationToken,
        bool includeStorageSnapshots = false,
        string clusterName = null);

    /// <summary>
    /// A compound operation that deletes all existing storage snapshots.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<DefaultOperationResponse> DeleteAllStorageSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        string clusterName = null);

    /// <summary>
    /// A compound operation that deletes all existing collection snapshots.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<DefaultOperationResponse> DeleteAllCollectionSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        string clusterName = null);

    /// <summary>
    /// A compound operation that deletes all existing collection shard snapshots.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<DefaultOperationResponse> DeleteAllCollectionShardSnapshots(
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        string clusterName = null);

    /// <summary>
    /// Recover collection on all cluster peers from the provided snapshot URIs.
    /// </summary>
    /// <param name="collectionName">The name of the collection to recover.</param>
    /// <param name="peerClients">The target peer qdrant node clients.</param>
    /// <param name="snapshotLocationUris">The snapshot URIs to recover collection from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="logger">The optional logger for state logging.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <param name="snapshotChecksums">
    /// Optional SHA256 checksums to verify snapshot integrity before recovery.
    /// <paramref name="snapshotChecksums"/>[i] is expected to correspond to <paramref name="snapshotLocationUris"/>[i].
    /// </param>
    /// <param name="clusterName">The optional cluster name for multi-cluster client scenarios.</param>
    Task<RecoverCollectionFromSnapshotsResponse> RecoverCollectionFromSnapshots(
        string collectionName,
        IQdrantHttpClient[] peerClients,
        Uri[] snapshotLocationUris,
        CancellationToken cancellationToken,
        ILogger logger = null,
        SnapshotPriority? snapshotPriority = null,
        string[] snapshotChecksums = null,
        string clusterName = null
    );
}
