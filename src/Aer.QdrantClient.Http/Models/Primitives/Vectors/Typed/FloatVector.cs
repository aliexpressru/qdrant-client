// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a single unnamed vector of float32 values.
/// </summary>
public sealed class FloatVector : VectorBase
{
    /// <summary>
    /// The vector values array.
    /// </summary>
    public float[] Values { internal init; get; }

    /// <inheritdoc/>
    public override VectorBase this[string vectorName]
        =>
            throw new NotSupportedException(
                $"Vector names are not supported for single vector values {GetType()}");

    /// <inheritdoc/>
    public override VectorBase FirstOrDefault() => this;

    /// <inheritdoc/>
    public override bool ContainsVector(string vectorName)
        =>
            throw new NotSupportedException(
                $"Vector names are not supported for single vector values {GetType()}");
}
