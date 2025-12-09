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
public sealed class SearchPointsDistanceMatrixOffsetsResponse
    : QdrantResponseBase<SearchPointsDistanceMatrixOffsetsResponse.PointsDistanceMatrixOffsetsUnit>
{
    /// <summary>
    /// Represents the distance matrix.
    /// </summary>
    public sealed class PointsDistanceMatrixOffsetsUnit
    {
        /// <summary>
        /// Row indices of the matrix.
        /// </summary>
        public ulong[] OffsetsRow { get; init; }

        /// <summary>
        /// Column indices of the matrix
        /// </summary>
        public ulong[] OffsetsCol { get; init; }

        /// <summary>
        /// Scores associated with matrix coordinates.
        /// </summary>
        public double[] Scores { get; init; }

        /// <summary>
        /// Ids of the points in order
        /// </summary>
        [JsonConverter(typeof(PointIdCollectionJsonConverter))]
        public IReadOnlyList<PointId> Ids { get; init; }
    }
}
