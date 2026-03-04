using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Util;

/// <summary>
/// Represent a utility class with convenience operations over qdrant requests.
/// </summary>
public static class QdrantOperationHelper
{
    /// <summary>
    /// Runs specified operations sequentially over qdrant and ensures that each operation succeeds.
    /// </summary>
    /// <param name="qdrantOperations">The operations to run and check successfulness.</param>
    public static async Task EnsureSuccess(
        params IEnumerable<Task<DefaultOperationResponse>> qdrantOperations)
    {
        foreach (var operation in qdrantOperations)
        {
            var operationResult = await operation;

            operationResult.EnsureSuccess();
        }
    }

    /// <summary>
    /// Runs specified operations sequentially over qdrant and ensures that each operation succeeds.
    /// </summary>
    /// <param name="qdrantOperations">The operations to run and check successfulness.</param>
    public static async Task EnsureSuccess<TResult>(
        params IEnumerable<Task<QdrantResponseBase<TResult>>> qdrantOperations)
    {
        foreach (var operation in qdrantOperations)
        {
            var operationResult = await operation;

            operationResult.EnsureSuccess();
        }
    }
}
