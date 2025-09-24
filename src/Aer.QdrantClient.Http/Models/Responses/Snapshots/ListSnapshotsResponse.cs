using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents a result of a snapshots listing operation.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class ListSnapshotsResponse : QdrantResponseBase<ICollection<SnapshotInfo>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListSnapshotsResponse"/> class.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "For deserialization purposes")]
    public ListSnapshotsResponse()
    { }

    internal ListSnapshotsResponse(QdrantResponseBase childResponse) : base(childResponse)
    { }
}
