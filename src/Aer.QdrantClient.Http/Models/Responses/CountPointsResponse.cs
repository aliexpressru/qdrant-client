using Aer.QdrantClient.Http.Models.Responses.Base;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the count points operation response.
/// </summary>
public sealed class CountPointsResponse : QdrantResponseBase<CountPointsResponse.CountPointsResult>
{
    /// <summary>
    /// Represents the count points result.
    /// </summary>
    public class CountPointsResult
    {
        /// <summary>
        /// Number of points which satisfy the count points conditions.
        /// </summary>
        public ulong Count { set; get; }
    }
}
