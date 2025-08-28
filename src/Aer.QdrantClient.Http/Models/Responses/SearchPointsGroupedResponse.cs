using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the search points grouped operation response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class SearchPointsGroupedResponse : QdrantResponseBase<SearchPointsGroupedResponse.GroupedPointsResponse>
{
    /// <summary>
    /// Represents the search points grouped operation response.
    /// </summary>
    public sealed class GroupedPointsResponse
    {
        /// <summary>
        /// Found search groups.
        /// </summary>
        public SearchGroupsUnit[] Groups { set; get; }

        /// <summary>
        /// Search groups.
        /// </summary>
        public sealed class SearchGroupsUnit
        {
            /// <summary>
            /// Scored points that have the same value of the group_by key.
            /// </summary>
            public ScoredPoint[] Hits { set; get; }

            /// <summary>
            /// Value of the group_by key, shared across all the hits in the group.
            /// </summary>
            [JsonConverter(typeof(SearchGroupIdJsonConverter))]
            public SearchGroupId Id { set; get; }

            /// <summary>
            /// Record that has been looked up using the group id
            /// </summary>
            public Point Lookup { set; get; }
        }
    }
}
