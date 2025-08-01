using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Models.Responses.Base;

/// <summary>
/// Represents the generic qdrant operation result.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class QdrantOperationResult
{
    /// <summary>
    /// Sequential number of the operation.
    /// </summary>
    public ulong OperationId { get; set; }

    /// <summary>
    /// The operation status.
    /// </summary>
    public QdrantOperationStatus Status { get; set; }
}
