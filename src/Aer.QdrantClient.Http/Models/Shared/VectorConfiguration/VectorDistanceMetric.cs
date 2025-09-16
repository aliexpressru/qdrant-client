namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents the vector distance metric.
/// </summary>
public enum VectorDistanceMetric
{
    /// <summary>
    /// Cosine vector distance metric.
    /// </summary>
    /// <remarks>
    /// For search efficiency, Cosine similarity is implemented as dot-product over normalized vectors.
    /// Vectors are automatically normalized during upload.
    /// </remarks>
    Cosine,

    /// <summary>
    /// Dot product vector distance metric.
    /// </summary>
    Dot,

    /// <summary>
    /// Euclidean vector distance metric.
    /// </summary>
    Euclid,

    /// <summary>
    /// Manhattan vector distance metric.
    /// </summary>
    Manhattan
}
