namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a single unnamed byte vector with values of [0-255].
/// </summary>
public sealed class ByteVector : VectorBase
{
    /// <summary>
    /// The vector values array.
    /// </summary>
    public byte[] Values { internal init; get; }

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
