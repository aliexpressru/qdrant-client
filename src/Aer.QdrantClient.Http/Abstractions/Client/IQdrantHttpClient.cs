using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Abstractions;

/// <summary>
/// Interface for the Qdrant HTTP API client.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public partial interface IQdrantHttpClient
{
    /// <summary>
    /// Gets the base URI address of the qdrant node used by the API client.
    /// </summary>
    public Uri BaseAddress { get; }

    /// <summary>
    /// Asynchronously wait until the collection status becomes <see cref="QdrantCollectionStatus.Green"/>
    /// and collection optimizer status becomes <see cref="QdrantOptimizerStatus.Ok"/>.
    /// </summary>
    /// <param name="collectionName">The name of the collection to check status for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="pollingInterval">The collection status polling interval. Is not set the default polling interval is 1 second.</param>
    /// <param name="timeout">The timeout after which the collection considered not green and exception is thrown. The default timeout is 30 seconds.</param>
    /// <param name="requiredNumberOfGreenCollectionResponses">The number of green status responses to be received
    /// for collection status to be considered green. To increase the probability that every node has
    /// the same green status - set this parameter to a value greater than the number of nodes.</param>
    /// <param name="isCheckShardTransfersCompleted">
    /// If set to <c>true</c> check that all collection shard transfers are completed.
    /// The collection is not considered ready until all shard transfers are completed.
    /// </param>
    Task EnsureCollectionReady(
        string collectionName,
        CancellationToken cancellationToken,
        TimeSpan? pollingInterval = null,
        TimeSpan? timeout = null,
        uint requiredNumberOfGreenCollectionResponses = 1,
        bool isCheckShardTransfersCompleted = false);
}
