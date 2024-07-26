using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;

/// <summary>
/// Represents a universal query API request.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class QueryPointsBatchedRequest
{
    /// <summary>
    /// The individual searches to execute as batch.
    /// </summary>
    public QueryPointsRequest[] Searches { get; }

    /// <summary>
    /// Initializes new instance of <see cref="QueryPointsBatchedRequest"/>
    /// </summary>
    /// <param name="searches">The individual queries to execute as batch.</param>
    /// <exception cref="ArgumentNullException">Happens when <paramref name="searches"/> is <c>null</c>.</exception>
    public QueryPointsBatchedRequest(params QueryPointsRequest[] searches)
    {
        Searches = searches ?? throw new ArgumentNullException(nameof(searches));
    }
}
