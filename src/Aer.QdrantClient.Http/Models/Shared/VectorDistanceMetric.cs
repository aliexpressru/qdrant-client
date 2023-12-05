// ReSharper disable UnusedMember.Global

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents the vector distance metric.
/// </summary>
public enum VectorDistanceMetric
{
    /// <summary>
    /// Cosine vector distance metric.
    /// </summary>
    Cosine,

    /// <summary>
    /// Dot product vector distance metric.
    /// </summary>
    Dot,

    /// <summary>
    /// Eucludean vector distance metric.
    /// </summary>
    Euclid
}
