using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Responses.Base;

namespace Aer.QdrantClient.Http.Models.Responses;

/// <summary>
/// Represents the vector distance matrix response.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class SearchPointsDistanceMatrixPairsResponse : QdrantResponseBase<SearchPointsDistanceMatrixPairsResponse.PointsDistanceMatrixPairsUnit>
{
    /// <summary>
    /// Represents the distance matrix.
    /// </summary>
    public class PointsDistanceMatrixPairsUnit
    {
        /// <summary>
        /// The distance matrix pairs.
        /// </summary>
        public PointsDistanceMatrixPair[] Pairs { get; set; }

        /// <summary>
        /// THe pair of points in distance matrix.
        /// </summary>
        public class PointsDistanceMatrixPair
        {
            /// <summary>
            /// First point id.
            /// </summary>
            [JsonConverter(typeof(PointIdJsonConverter))]
            public PointId A { set; get; }

            /// <summary>
            /// Second point id.
            /// </summary>
            [JsonConverter(typeof(PointIdJsonConverter))]
            public PointId B { set; get; }

            /// <summary>
            /// The point distance score.
            /// </summary>
            public double Score { get; set; }
        }
    }
}
