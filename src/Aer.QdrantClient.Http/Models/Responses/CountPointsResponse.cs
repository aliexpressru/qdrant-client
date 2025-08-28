using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the count points operation response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class CountPointsResponse : QdrantResponseBase<CountPointsResponse.CountPointsResult>
{
    /// <summary>
    /// Represents the count points result.
    /// </summary>
    public sealed class CountPointsResult
    {
        /// <summary>
        /// Number of points which satisfy the count points conditions.
        /// </summary>
        public ulong Count { set; get; }
    }
}
