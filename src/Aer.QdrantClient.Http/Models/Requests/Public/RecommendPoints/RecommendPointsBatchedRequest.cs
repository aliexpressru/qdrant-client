// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the recommend points batch request.
/// </summary>
/// <remarks>
/// The batch recommend API enables to perform multiple recommend requests via a single request.
/// Its semantic is straightforward : 1 batched recommend request is equivalent to n singular recommend requests.
/// </remarks>
public sealed class RecommendPointsBatchedRequest
{
    /// <summary>
    /// The individual recommend requests to execute as batch.
    /// </summary>
    public RecommendPointsByRequest[] Searches { get; }

    /// <summary>
    /// Initializes new instance of <see cref="RecommendPointsBatchedRequest"/>
    /// </summary>
    /// <param name="searches">The individual recommend requests to execute as batch.</param>
    /// <exception cref="ArgumentNullException">Happens when <paramref name="searches"/> is <c>null</c>.</exception>
    public RecommendPointsBatchedRequest(params RecommendPointsByRequest[] searches)
    {
        Searches = searches ?? throw new ArgumentNullException(nameof(searches));
    }
}
