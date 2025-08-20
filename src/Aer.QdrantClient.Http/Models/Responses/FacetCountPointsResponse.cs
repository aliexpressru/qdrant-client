using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the facet count points operation response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class FacetCountPointsResponse : QdrantResponseBase<FacetCountPointsResponse.FacetCountHitsUnit>
{
    /// <summary>
    /// Represents all filed values facet counts.
    /// </summary>
    public class FacetCountHitsUnit
    {
        /// <summary>
        /// The facet fount for each existing field value.
        /// </summary>
        public FacetCountHitUnit[] Hits { get; set; }
    }

    /// <summary>
    /// Represents a one filed value facet count.
    /// </summary>
    public class FacetCountHitUnit
    {
        /// <summary>
        /// The field value to count facet for.
        /// </summary>
        public JsonElement Value { get; set; }

        /// <summary>
        /// Gets the value of the field as specified data type.
        /// </summary>
        /// <typeparam name="T">The type to convert value to.</typeparam>
        public T ValueAs<T>() => Value.Deserialize<T>(JsonSerializerConstants.DefaultSerializerOptions);

        /// <summary>
        /// The field value facet count.
        /// </summary>
        public ulong Count { get; set; }
    }
}
