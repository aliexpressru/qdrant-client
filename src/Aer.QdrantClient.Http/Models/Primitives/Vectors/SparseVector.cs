using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a sparse vector.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class SparseVector : VectorBase
{
    /// <summary>
    /// Gets the positions of the non-zero values in the sparse vector.
    /// </summary>
    public HashSet<uint> Indices { get; init; }

    /// <summary>
    /// Gets the values of the non-zero sparse vector elements.
    /// </summary>
    public float[] Values { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override VectorKind VectorKind => VectorKind.Sparse;

    /// <inheritdoc/>
    [JsonIgnore]
    public override float[] Default
        =>
            throw new NotSupportedException(
            $"Getting default vector from sparse vector {GetType()} is not supported since sparse vector is a two-component value");

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
