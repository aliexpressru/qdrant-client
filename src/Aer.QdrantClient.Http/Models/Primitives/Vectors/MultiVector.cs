using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a multivector that consists of multiple dense vectors.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class MultiVector : VectorBase
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
    public override VectorBase Default
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

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("[");

        for (int vectorIndex = 0; vectorIndex < Vectors.Length; vectorIndex++)
        {
            sb.Append('[');
            sb.Append(string.Join(",", Vectors[vectorIndex].Select(v => v.ToString(CultureInfo.InvariantCulture))));
            sb.Append(']');

            if (vectorIndex < Vectors.Length - 1)
            {
                sb.AppendLine(",");
            }
            else
            {
                // For pretty printing
                sb.AppendLine();
            }
        }

        sb.Append(']');

        return sb.ToString();
    }
}
