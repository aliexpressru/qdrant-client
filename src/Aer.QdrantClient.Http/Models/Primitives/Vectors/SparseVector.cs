using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;
#if NETSTANDARD2_0
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
#endif

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a sparse vector.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class SparseVector : VectorBase
{
    /// <summary>
    /// Gets the positions of the non-zero values in the sparse vector.
    /// </summary>
    public HashSet<uint> Indices { get; }

    /// <summary>
    /// Gets the values of the non-zero sparse vector elements.
    /// </summary>
    public float[] Values { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override VectorKind VectorKind => VectorKind.Sparse;

    /// <inheritdoc/>
    [JsonIgnore]
    public override VectorBase Default => this;

    /// <summary>
    /// Initializes a new instance of <see cref="SparseVector"/> from indices and values.
    /// </summary>
    /// <param name="indices">The indices of non-zero vector elements.</param>
    /// <param name="values">The non-zero vector elements.</param>
    [JsonConstructor]
    public SparseVector(HashSet<uint> indices, float[] values)
    {
        if (indices is not {Count: > 0})
        {
            throw new ArgumentException($"{nameof(indices)} array can't be empty", nameof(indices));
        }

        if (values is not {Length: > 0})
        {
            throw new ArgumentException($"{nameof(values)} array can't be empty", nameof(values));
        }

        if (indices.Count != values.Length)
        {
            throw new ArgumentException($"{nameof(indices)} and {nameof(values)} arrays must be the same length");
        }

        Indices = indices;
        Values = values;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SparseVector"/> from indices and values.
    /// </summary>
    /// <param name="indices">The indices of non-zero vector elements.</param>
    /// <param name="values">The non-zero vector elements.</param>
    public SparseVector(uint[] indices, float[] values)
    {
        if (indices is not {Length: > 0})
        {
            throw new ArgumentException($"{nameof(indices)} array can't be empty", nameof(indices));
        }

        if (values is not {Length: > 0})
        {
            throw new ArgumentException($"{nameof(values)} array can't be empty", nameof(values));
        }

        if (indices.Length != values.Length)
        {
            throw new ArgumentException($"{nameof(indices)} and {nameof(values)} arrays must be the same length");
        }

        Indices = indices.ToHashSet();
        Values = values;
    }

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
            throw new NotSupportedException($"Vector names are not supported for sparse vector values {GetType()}");

    /// <inheritdoc/>
    public override VectorBase FirstOrDefault()
        =>
            throw new NotSupportedException(
                $"Getting default vector from sparse vector {GetType()} is not supported since sparse vector is a two-component value");

    /// <inheritdoc/>
    public override bool ContainsVector(string vectorName)
        =>
            throw new NotSupportedException($"Vector names are not supported for sparse vector values {GetType()}");

    /// <inheritdoc/>
    public override string ToString() =>
        $$"""
        {
          "indexes":[{{string.Join(",", Indices)}}],
          "values":[{{string.Join(",", Values.Select(v => v.ToString(CultureInfo.InvariantCulture)))}}]
        }
        """;

    /// <inheritdoc/>
    public override void WriteToStream(StreamWriter streamWriter)
    {
        if (streamWriter == null) throw new ArgumentNullException(nameof(streamWriter));

        streamWriter.Write('{');

        streamWriter.Write("\"indexes\":[");
        
        for (int i = 0; i < Indices.Count; i++)
        {
            if (i > 0)
            {
                streamWriter.Write(',');
            }

            streamWriter.Write(Indices.ElementAt(i));
        }

        streamWriter.Write("],");

        streamWriter.Write("\"values\":[");
        
        for (int i = 0; i < Values.Length; i++)
        {
            if (i > 0)
            {
                streamWriter.Write(',');
            }

            streamWriter.Write(Values[i].ToString(CultureInfo.InvariantCulture));
        }

        streamWriter.Write(']');

        streamWriter.Write('}');
    }
}
