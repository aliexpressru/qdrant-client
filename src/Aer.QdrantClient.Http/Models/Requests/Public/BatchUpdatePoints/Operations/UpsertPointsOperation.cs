using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the points upsert operation.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class UpsertPointsOperation : BatchUpdatePointsOperationBase
{
    /// <summary>
    /// Upsert points request.
    /// </summary>
    /// <remarks>
    /// We don't use generic request here due to System.Text.Json limitations.
    /// But this class is internal and never created by end user so we are relatively safe.
    /// </remarks>
    public required object Upsert { set; get; }
}
