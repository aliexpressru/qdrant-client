using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a multivector that consists of multiple dense vectors.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public class MultiVector : VectorBase
{
    /// <summary>
    /// The multiple vectors array.
    /// </summary>
    public required float[][] Vectors { init; get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override VectorKind VectorKind => VectorKind.Multi;

    /// <inheritdoc/>
    [JsonIgnore]
    public override float[] Default
        =>
            Vectors.Length > 0
                ? Vectors[0]
                : throw new InvalidOperationException("Multivector is empty");

    /// <inheritdoc/>
    public override VectorBase this[string vectorName]
        =>
            throw new NotSupportedException($"Vector names are not supported for multivector values {GetType()}");

    /// <inheritdoc/>
    public override VectorBase FirstOrDefault()
        =>
            throw new NotSupportedException(
                $"Getting default vector from multivector {GetType()} is not supported since multivector is a multi-component value");

    /// <inheritdoc/>
    public override bool ContainsVector(string vectorName)
        =>
            throw new NotSupportedException(
                $"Vector names are not supported for multivector values {GetType()}");
}
