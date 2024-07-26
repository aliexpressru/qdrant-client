using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Primitives;

/// <summary>
/// Represents a qdrant <see cref="Point"/> with additional score data from search process
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class ScoredPoint : Point
{
    /// <summary>
    /// Point version.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Points vector distance to the query vector.
    /// </summary>
    public float Score { get; set; }
}
