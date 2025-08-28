using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents a result of a snapshots listing operation.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class ListSnapshotsResponse : QdrantResponseBase<SnapshotInfo[]>
{ }
