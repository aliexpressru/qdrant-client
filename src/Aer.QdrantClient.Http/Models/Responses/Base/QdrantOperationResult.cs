using Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Responses.Base;

/// <summary>
/// Represents the generic qdrant operation (sync or async) result.
/// </summary>
public class QdrantOperationResult
{
    /// <summary>
    /// The operation identifier.
    /// </summary>
    public ulong OperationId { get; set; }

    /// <summary>
    /// The operation status.
    /// </summary>
    public QdrantOperationStatus Status { get; set; }
}
