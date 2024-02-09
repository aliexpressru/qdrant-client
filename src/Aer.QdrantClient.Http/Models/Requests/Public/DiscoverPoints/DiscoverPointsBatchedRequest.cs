// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public.DiscoverPoints;

/// <summary>
/// Represents the discover points batch request.
/// </summary>
/// <remarks>
/// The batch discover API enables to perform multiple discover requests via a single request.
/// Its semantic is straightforward : 1 batched discover request is equivalent to n singular discover requests.
/// </remarks>
public class DiscoverPointsBatchedRequest
{
    /// <summary>
    /// The individual recommend requests to execute as batch.
    /// </summary>
    public DiscoverPointsByRequest[] Searches { get; }

    /// <summary>
    /// Initializes new instance of <see cref="DiscoverPointsBatchedRequest"/>
    /// </summary>
    /// <param name="searches">The individual discover requests to execute as batch.</param>
    /// <exception cref="ArgumentNullException">Happens when <paramref name="searches"/> is <c>null</c>.</exception>
    public DiscoverPointsBatchedRequest(params DiscoverPointsByRequest[] searches)
    {
        Searches = searches ?? throw new ArgumentNullException(nameof(searches));
    }
}
