using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a multivector that consists of multiple dense vectors.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class MultiVector : VectorBase, IEquatable<VectorBase>, IEquatable<MultiVector>
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

    /// <inheritdoc/>
    public override void WriteToStream(StreamWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        writer.Write('[');

        for (int vectorIndex = 0; vectorIndex < Vectors.Length; vectorIndex++)
        {
            if (vectorIndex > 0)
            {
                writer.Write(',');
            }

            writer.Write('[');

            for (int i = 0; i < Vectors[vectorIndex].Length; i++)
            {
                if (i > 0)
                {
                    writer.Write(',');
                }

                writer.Write(Vectors[vectorIndex][i].ToString(CultureInfo.InvariantCulture));
            }

            writer.Write(']');
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
        
        writer.Write(Vectors.Length);

        for (int vectorIndex = 0; vectorIndex < Vectors.Length; vectorIndex++)
        {
            if (vectorIndex > 0)
            {
                writer.Write(',');
            }

            writer.Write('[');

            writer.Write(Vectors[vectorIndex].Length);

            for (int i = 0; i < Vectors[vectorIndex].Length; i++)
            {
                if (i > 0)
                {
                    writer.Write(',');
                }

                writer.Write(Vectors[vectorIndex][i]);
            }

            writer.Write(']');
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

        int vectorsCount = reader.ReadInt32();
        float[][] vectors = new float[vectorsCount][];

        for (int vectorIndex = 0; vectorIndex < vectorsCount; vectorIndex++)
        {
            if (vectorIndex > 0)
            {
                // ","
                reader.ReadChar();
            }

            // "["
            reader.ReadChar();

            int vectorLength = reader.ReadInt32();
            vectors[vectorIndex] = new float[vectorLength];

            for (int i = 0; i < vectorLength; i++)
            {
                if (i > 0)
                {
                    // ","
                    reader.ReadChar();
                }

                vectors[vectorIndex][i] = reader.ReadSingle();
            }

            // "]"
            reader.ReadChar();
        }

        // "]"
        reader.ReadString();

        return new MultiVector()
        {
            Vectors = vectors
        };
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
        
        return other is MultiVector otherMultiVector && Equals(otherMultiVector);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj) 
            || obj is MultiVector other && Equals(other);
    }

    /// <inheritdoc/>
    public bool Equals(MultiVector other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }
        
        if(Vectors.Length != other.Vectors.Length)
        {
            return false;
        }
        
        for(int i=0; i<Vectors.Length; i++)
        {
            if (Vectors[i].Length != other.Vectors[i].Length)
            {
                return false;
            }

            var componentVectorEquals = Vectors[i].SequenceEqual(other.Vectors[i]);

            if (!componentVectorEquals)
            { 
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (Vectors is null or {Length: 0})
        {
            return 0;
        }
        
        HashCode hashCode = new HashCode();
        
        foreach (var vector in Vectors)
        {
            foreach (var value in vector)
            {
                hashCode.Add(value);
            }
        }
        
        return hashCode.ToHashCode();
    }
}
