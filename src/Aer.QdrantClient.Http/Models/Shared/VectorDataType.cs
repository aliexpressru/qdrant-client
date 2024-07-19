using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a vector datatype to represent vectors in the storage.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum VectorDataType
{
    /// <summary>
    /// Vectors are stored as single-precision floating point numbers, 4bytes.
    /// </summary>
    Float32,

    /// <summary>
    /// Vectors are stored as half-precision floating point numbers, 2 bytes.
    /// </summary>
    Float16,

    /// <summary>
    /// Vectors are stored as unsigned 8-bit integers, 1byte. Vector elements are expected to be in range [0, 255].
    /// </summary>
    Uint8
}
