using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a dense vector.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class DenseVector : VectorBase
{
    /// <summary>
    /// The vector values array.
    /// </summary>
    public float[] VectorValues { internal init; get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override VectorKind VectorKind => VectorKind.Dense;

    /// <inheritdoc/>
    [JsonIgnore]
    public override VectorBase Default => this;

    /// <inheritdoc/>
    public override VectorBase this[string vectorName]
        =>
            throw new NotSupportedException(
                $"Vector names are not supported for single vector values {GetType()}");

    /// <inheritdoc/>
    public override VectorBase FirstOrDefault() => Default;

    /// <inheritdoc/>
    public override bool ContainsVector(string vectorName)
        =>
            throw new NotSupportedException(
                $"Vector names are not supported for single vector values {GetType()}");

    /// <inheritdoc/>
    public override string ToString() =>
        $"[{string.Join(",", VectorValues.Select(v => v.ToString(CultureInfo.InvariantCulture)))}]";
}
