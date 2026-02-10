using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a dense vector.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class DenseVector : VectorBase, IEquatable<VectorBase>, IEquatable<DenseVector>
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

    /// <summary>
    /// Initializes a new instance of <see cref="DenseVector"/> from provided element values.
    /// </summary>
    /// <param name="vectorValues">The vector elements values.</param>
    [JsonConstructor]
    public DenseVector(float[] vectorValues)
    {
        if (vectorValues is null or { Length: 0 })
        {
            throw new ArgumentException($"{nameof(vectorValues)} array can't be null or empty", nameof(vectorValues));
        }

        VectorValues = vectorValues;
    }

    /// <inheritdoc/>
    public override VectorBase this[string vectorName] =>
        throw new NotSupportedException($"Vector names are not supported for single vector values {GetType()}");

    /// <inheritdoc/>
    public override VectorBase FirstOrDefault() => Default;

    /// <inheritdoc/>
    public override bool ContainsVector(string vectorName) =>
        throw new NotSupportedException($"Vector names are not supported for single vector values {GetType()}");

    /// <inheritdoc/>
    public override string ToString() =>
        $"[{string.Join(",", VectorValues.Select(v => v.ToString(CultureInfo.InvariantCulture)))}]";

    /// <inheritdoc/>
    public override void WriteToStream(StreamWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        writer.Write('[');

        for (int i = 0; i < VectorValues.Length; i++)
        {
            if (i > 0)
            {
                writer.Write(',');
            }

            writer.Write(VectorValues[i].ToString(CultureInfo.InvariantCulture));
        }

        writer.Write(']');
    }

    /// <inheritdoc/>
    public override void WriteToStream(BinaryWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        writer.Write("[");

        writer.Write(VectorValues.Length);

        for (int i = 0; i < VectorValues.Length; i++)
        {
            if (i > 0)
            {
                writer.Write(',');
            }

            writer.Write(VectorValues[i]);
        }

        writer.Write("]");
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

        // "["
        reader.ReadString();

        int length = reader.ReadInt32();
        var values = new float[length];

        for (int i = 0; i < length; i++)
        {
            // ","
            if (i > 0)
            {
                reader.ReadChar();
            }

            values[i] = reader.ReadSingle();
        }

        // "]"
        reader.ReadString();

        return new DenseVector(values);
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

        return other is DenseVector denseVector && Equals(denseVector);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj)
            || (obj is DenseVector denseVector && Equals(denseVector));
    }

    /// <inheritdoc/>
    public bool Equals(DenseVector other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other)
            || VectorValues.SequenceEqual(other.VectorValues, EqualityComparer<float>.Default);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (VectorValues is null or { Length: 0 })
        {
            return 0;
        }

        HashCode hashCode = new();

        foreach (var value in VectorValues)
        {
            hashCode.Add(value);
        }

        return hashCode.ToHashCode();
    }
}
