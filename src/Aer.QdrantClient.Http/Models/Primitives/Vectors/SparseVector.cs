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
public sealed class SparseVector : VectorBase, IEquatable<VectorBase>, IEquatable<SparseVector>
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
    public override void WriteToStream(StreamWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        writer.Write("{\"indexes\":[");

        int indexNumber = 0;
        foreach (var index in Indices)
        {
            if (indexNumber > 0)
            {
                writer.Write(',');
            }

            writer.Write(index);

            indexNumber++;
        }

        writer.Write("],");

        writer.Write("\"values\":[");

        for (int i = 0; i < Values.Length; i++)
        {
            if (i > 0)
            {
                writer.Write(',');
            }

            writer.Write(Values[i].ToString(CultureInfo.InvariantCulture));
        }

        writer.Write("]}");
    }

    /// <inheritdoc/>
    public override void WriteToStream(BinaryWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        writer.Write('{');

        writer.Write(Indices.Count);

        writer.Write("\"indexes\":[");

        int indexNumber = 0;
        foreach (var index in Indices)
        {
            if (indexNumber > 0)
            {
                writer.Write(',');
            }

            writer.Write(index);

            indexNumber++;
        }

        writer.Write("],");

        writer.Write(Values.Length);

        writer.Write("\"values\":[");

        for (int i = 0; i < Values.Length; i++)
        {
            if (i > 0)
            {
                writer.Write(',');
            }

            writer.Write(Values[i]);
        }

        writer.Write("]}");
    }

    /// <summary>
    /// Reads a <see cref="DenseVector"/> instance from a binary stream.
    /// </summary>
    /// <param name="reader">The reader to read vector from.</param>
    public static VectorBase ReadFromStream(BinaryReader reader)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        // "{"
        reader.ReadChar();

        int indicesCount = reader.ReadInt32();
        var indices = new uint[indicesCount];

        // "indexes":[
        reader.ReadString();

        for (int i = 0; i < indicesCount; i++)
        {
            if (i > 0)
            {
                // ,
                reader.ReadChar();
            }

            indices[i] = reader.ReadUInt32();
        }

        // ],
        reader.ReadString();

        int valuesCount = reader.ReadInt32();
        var values = new float[valuesCount];

        // "values":[
        reader.ReadString();

        for (int i = 0; i < valuesCount; i++)
        {
            if (i > 0)
            {
                // ,
                reader.ReadChar();
            }

            values[i] = reader.ReadSingle();
        }

        // ]}
        reader.ReadString();

        return new SparseVector(indices, values);
    }

    /// <inheritdoc/>
    public override bool Equals(VectorBase other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return other is SparseVector otherSparse && Equals(otherSparse);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj)
            || (obj is SparseVector other && Equals(other));
    }

    /// <inheritdoc/>
    public bool Equals(SparseVector other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other)
            || (
                Indices.SequenceEqual(other.Indices, EqualityComparer<uint>.Default)
                && Values.SequenceEqual(other.Values, EqualityComparer<float>.Default)
            );
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if((Indices == null && Values == null)
           || (Indices!.Count == 0 && Values.Length == 0))
        {
            return 0;
        }               
        
        HashCode hashCode = new HashCode();
        
        foreach (var index in Indices)
        {
            hashCode.Add(index);
        }
        
        foreach (var value in Values)
        {
            hashCode.Add(value);
        }
        
        return hashCode.ToHashCode();
    }
}
