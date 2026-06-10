using Aer.QdrantClient.Http.Abstractions;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Responses.CompoundOperations;
using Aer.QdrantClient.Http.Models.Shared;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Aer.QdrantClient.Http.Infrastructure.Snapshots;

/// <summary>
/// A component responsible for full collection recovery from multiple snapshots.
/// </summary>
public class CollectionRecoverer
{
    private readonly string _targetCollectionName;

    private ConcurrentQueue<ScheduledPeerCollectionRecovery> _recoveryPlan;

    /// <summary>
    /// Returns the planned shard replications. If no replication required returns an empty collection.
    /// </summary>
    public IReadOnlyCollection<ScheduledPeerCollectionRecovery> RecoveryPlan => _recoveryPlan ?? [];

    internal CollectionRecoverer(string targetCollectionName)
    {
        _targetCollectionName = targetCollectionName;
    }

    internal RecoverCollectionFromSnapshotsResponse Plan(
        IQdrantHttpClient[] peerClients,
        Uri[] snapshotLocationUris,
        string[] snapshotChecksums = null)
    {
        _recoveryPlan = new();

        for (int i = 0; i < snapshotLocationUris.Length; i++)
        {
            var snapshotUri = snapshotLocationUris[i];
            var snapshotChecksum = snapshotChecksums?[i];
            var peerClient = peerClients[i];

            _recoveryPlan.Enqueue(new(peerClient, snapshotUri, snapshotChecksum));
        }

        return new RecoverCollectionFromSnapshotsResponse() { Result = this, Status = QdrantStatus.Success() };
    }

    /// <summary>
    /// Asynchronously executes the whole <see cref="RecoveryPlan"/>. Executes the next recovery step on each iteration.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous replication operation.
    /// </param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <remarks>
    /// It is recommended to check on the returned recovery status
    /// before continuing with the next recovery iteration step.
    /// </remarks>
    public async IAsyncEnumerable<DefaultOperationResponse> ExecuteRecoveries(
        [EnumeratorCancellation] CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null
    )
    {
        if (_recoveryPlan is null or { IsEmpty: true })
        {
            yield break;
        }

        while (!_recoveryPlan.IsEmpty)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _recoveryPlan.TryDequeue(out var nextRecoveryStep);

            var shardReplicationResult = await ExecuteNextRecoveryInternal(
                nextRecoveryStep,
                isWaitForResult,
                snapshotPriority,
                cancellationToken
            );

            yield return shardReplicationResult;
        }
    }

    /// <summary>
    /// Asynchronously executes the next recovery from a <see cref="RecoveryPlan"/> and returns its result.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous replication operation.
    /// </param>
    /// <param name="isWaitForResult">If <c>true</c>, wait for changes to actually happen. If <c>false</c> - let changes happen in background.</param>
    /// <param name="snapshotPriority">Defines which data should be used as a source of truth if there are other replicas in the cluster.</param>
    /// <remarks>
    /// It is recommended to check on the returned recovery status
    /// before continuing with the next recovery step.
    /// </remarks>
    public Task<DefaultOperationResponse> ExecuteNextRecovery(
        CancellationToken cancellationToken,
        bool isWaitForResult = true,
        SnapshotPriority? snapshotPriority = null
    )
    {
        if (_recoveryPlan is null or { IsEmpty: true })
        {
            return Task.FromResult(DefaultOperationResponse.Fail(QdrantStatus.Fail("No replications to execute"), time: 0));
        }

        _recoveryPlan.TryDequeue(out var nextRestorationStep);

        return ExecuteNextRecoveryInternal(
            nextRestorationStep,
            isWaitForResult,
            snapshotPriority,
            cancellationToken
        );
    }

    private async Task<DefaultOperationResponse> ExecuteNextRecoveryInternal(
        ScheduledPeerCollectionRecovery nextRecoveryStep,
        bool isWaitForResult,
        SnapshotPriority? snapshotPriority,
        CancellationToken cancellationToken
    )
    {
        var recoveryResult = await nextRecoveryStep.PeerClient.RecoverCollectionFromSnapshot(
            _targetCollectionName,
            nextRecoveryStep.SnapshotLocationUri,
            cancellationToken,
            isWaitForResult,
            snapshotPriority,
            nextRecoveryStep.SnapshotChecksum
        );

        return recoveryResult;
    }
}
