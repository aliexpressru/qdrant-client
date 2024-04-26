using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a sparse vector of float32 values.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class SparseFloatVector : VectorBase
{
    /// <summary>
    /// Gets the positions of the non-zero values in the sparse vector.
    /// </summary>
    public HashSet<uint> Indices { get; init; }

    /// <summary>
    /// Gets the values of the non-zero sparse vector elements.
    /// </summary>
    public float[] Values { get; init; }

    /// <summary>
    /// Deconstructs the sparse vector into its Indices and Values components.
    /// </summary>
    public void Deconstruct(out HashSet<uint> indices, out float[] values)
    {
        indices = Indices;
        values = Values;
    }

    /// <inheritdoc/>
    public override VectorBase this[string vectorName]
        =>
            throw new NotSupportedException(
                $"Vector names are not supported for sparse vector values {GetType()}");

    /// <inheritdoc/>
    public override VectorBase FirstOrDefault()
        =>
            throw new NotSupportedException(
                $"Getting default vector from sparse vector {GetType()} is not supported since sparse vector is a two-component value");

    /// <inheritdoc/>
    public override bool ContainsVector(string vectorName)
        =>
            throw new NotSupportedException(
                $"Vector names are not supported for sparse vector values {GetType()}");
}
