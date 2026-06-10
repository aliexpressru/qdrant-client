using Aer.QdrantClient.Http.Infrastructure.Snapshots;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses.CompoundOperations;

/// <summary>
/// Represents a result of the full collection recovery from a snapshot process start.
/// Note that successful response status does not indicate that
/// the recovery actually took place. It only indicates that all the preparation work had been done.
/// Use <see cref="CollectionRecoverer"/> from the result property to execute restoration queue.
/// You can check the planned restorations by inspecting <see cref="CollectionRecoverer.RecoveryPlan"/>.
/// </summary>
public class RecoverCollectionFromSnapshotsResponse : QdrantResponseBase<CollectionRecoverer>
{
}
