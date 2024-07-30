using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the scroll points operation response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
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
        public Point[] Points { get; init; }

        /// <summary>
        /// The next page offset - used in pagination.
        /// </summary>
        [JsonConverter(typeof(PointIdJsonConverter))]
        public PointId NextPageOffset { get; init; }
    }
}
