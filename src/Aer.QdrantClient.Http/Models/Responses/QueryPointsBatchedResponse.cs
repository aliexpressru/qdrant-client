using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the universal query API points operation response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class QueryPointsBatchedResponse : QdrantResponseBase<QueryPointsResponse.QueryPointResults[]>
{
    /// <summary>
    /// The query points response.
    /// </summary>
    public class QueryPointResults
    {
        /// <summary>
        /// The found points.
        /// </summary>
        public ScoredPoint[] Points { get; init; }
    }
}
