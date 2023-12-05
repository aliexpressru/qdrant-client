// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a single unnamed vector.
/// </summary>
public sealed class Vector : VectorBase
{
    /// <summary>
    /// The vector values array.
    /// </summary>
    public float[] VectorValues { internal init; get; }

    /// <inheritdoc/>
    public override float[] Default => VectorValues;

    /// <inheritdoc/>
    public override float[] this[string vectorName] =>
        throw new NotSupportedException(
            $"Named vectors are not supported for single vector values {GetType()}");

    /// <inheritdoc/>
    public override float[] FirstOrDefault() => Default;

    /// <inheritdoc/>
    public override bool ContainsVector(string vectorName) =>
        throw new NotSupportedException(
            $"Named vectors are not supported for single vector values {GetType()}");
}
