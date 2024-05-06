using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the points delete operation.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal sealed class DeletePointsOperation : BatchUpdatePointsOperationBase
{
    /// <summary>
    /// Delete points request.
    /// </summary>
    public required DeletePointsRequest Delete { set; get; }
}
