using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;

/// <summary>
/// Represents the discover points batch request.
/// </summary>
/// <remarks>
/// The batch discover API enables to perform multiple discover requests via a single request.
/// Its semantic is straightforward : 1 batched discover request is equivalent to n singular discover requests.
/// </remarks>
/// <remarks>
/// Initializes new instance of <see cref="DiscoverPointsBatchedRequest"/>
/// </remarks>
/// <param name="searches">The individual discover requests to execute as batch.</param>
/// <exception cref="ArgumentNullException">Happens when <paramref name="searches"/> is <c>null</c>.</exception>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class DiscoverPointsBatchedRequest(params DiscoverPointsRequest[] searches)
{
    /// <summary>
    /// The individual recommend requests to execute as batch.
    /// </summary>
    public DiscoverPointsRequest[] Searches { get; } = searches ?? throw new ArgumentNullException(nameof(searches));
}
