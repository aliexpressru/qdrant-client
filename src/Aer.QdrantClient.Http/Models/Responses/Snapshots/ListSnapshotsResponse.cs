using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents a result of a snapshots listing operation.
/// </summary>
public sealed class ListSnapshotsResponse : QdrantResponseBase<SnapshotInfo[]>
{ }
