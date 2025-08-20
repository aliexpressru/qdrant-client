using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// Represents the points batch search request.
/// </summary>
/// <remarks>
/// The batch search API enables to perform multiple search requests via a single request.
/// Its semantic is straightforward : 1 batched search request is equivalent to n singular search requests.
/// </remarks>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class SearchPointsBatchedRequest
{
    /// <summary>
    /// The individual searches to execute as batch.
    /// </summary>
    public SearchPointsRequest[] Searches { get; }

    /// <summary>
    /// Initializes new instance of <see cref="SearchPointsBatchedRequest"/>
    /// </summary>
    /// <param name="searches">The individual searches to execute as batch.</param>
    /// <exception cref="ArgumentNullException">Happens when <paramref name="searches"/> is <c>null</c>.</exception>
    public SearchPointsBatchedRequest(params SearchPointsRequest[] searches)
    {
        Searches = searches ?? throw new ArgumentNullException(nameof(searches));
    }
}
