namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Describes the kind of the vector that is contained in <see cref="VectorBase"/>.
/// </summary>
public enum VectorKind
{
    /// <summary>
    /// Dense float vector.
    /// </summary>
    Dense,

    /// <summary>
    /// A dictionary of named vectors by names.
    /// </summary>
    Named,

    /// <summary>
    /// A sparse vector.
    /// </summary>
    Sparse,

    /// <summary>
    /// A multivector : vector of vectors.
    /// </summary>
    Multi
}
