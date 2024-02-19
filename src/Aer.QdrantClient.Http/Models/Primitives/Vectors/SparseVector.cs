// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a sparse vector.
/// </summary>
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
    public override float[] Default => Values;

    /// <inheritdoc/>
    public override float[] this[string vectorName] => throw new NotSupportedException(
        $"Vector names are not supported for sparse vector values {GetType()}");

    /// <inheritdoc/>
    public override float[] FirstOrDefault() => Default;

    /// <inheritdoc/>
    public override bool ContainsVector(string vectorName) => throw new NotSupportedException(
        $"Vector names are not supported for sparse vector values {GetType()}");
}
