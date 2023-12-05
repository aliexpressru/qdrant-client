using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Responses.Base;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the scoll points operation response.
/// </summary>
public sealed class ScrollPointsResponse : QdrantResponseBase<ScrollPointsResponse.ScrollResult>
{
    /// <summary>
    /// Represents the returned points.
    /// </summary>
    public sealed class ScrollResult
    {
        /// <summary>
        /// The point values.
        /// </summary>
        public Point[] Points { get; set; }

        /// <summary>
        /// The next page offset - used in pagination.
        /// </summary>
        [JsonConverter(typeof(PointIdJsonConverter))]
        public PointId NextPageOffset { get; set; }
    }
}
