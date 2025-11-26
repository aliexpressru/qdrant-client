using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a named vectors collection.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class NamedVectors : VectorBase, IEquatable<VectorBase>, IEquatable<NamedVectors>
{
    /// <summary>
    /// The name to vector mapping.
    /// </summary>
    public required Dictionary<string, VectorBase> Vectors { init; get; } = [];

    /// <inheritdoc/>
    [JsonIgnore]
    public override VectorKind VectorKind => VectorKind.Named;

    /// <inheritdoc/>
    [JsonIgnore]
    public override VectorBase Default
    {
        get {
            EnsureNotEmpty();

            if (Vectors.TryGetValue(DefaultVectorName, out VectorBase defaultVector))
            {
                return defaultVector;
            }

            throw new QdrantDefaultVectorNotFoundException(DefaultVectorName);
        }
    }

    /// <inheritdoc/>
    public override VectorBase FirstOrDefault()
    {
        EnsureNotEmpty();

        return Vectors.First().Value;
    }

    /// <inheritdoc/>
    public override bool ContainsVector(string vectorName)
    {
        EnsureNotEmpty();

        return Vectors.ContainsKey(vectorName);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine("{");

        int vectorNumber = 0;
        foreach (var (name, vector) in Vectors)
        {
            sb.Append($"\"{name}\"");
            sb.Append(':');
            sb.Append(vector);

            if (vectorNumber != Vectors.Count - 1)
            {
                sb.AppendLine(",");
            }
            else
            {
                // For pretty printing
                sb.AppendLine();
            }

            vectorNumber++;
        }

        sb.Append('}');

        return sb.ToString();
    }

    /// <inheritdoc/>
    public override void WriteToStream(StreamWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        writer.Write('{');

        int vectorNumber = 0;
        foreach (var (name, vector) in Vectors)
        {
            if (vectorNumber > 0)
            {
                writer.Write(',');
            }

            writer.Write($"\"{name}\":");
            vector.WriteToStream(writer);

            vectorNumber++;
        }

        writer.Write('}');
    }

    /// <inheritdoc/>
    public override void WriteToStream(BinaryWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        writer.Write('{');

        writer.Write(Vectors.Count);

        int vectorNumber = 0;
        foreach (var (name, vector) in Vectors)
        {
            if (vectorNumber > 0)
            {
                writer.Write(',');
            }

            writer.Write($"\"{name}\":");

            writer.Write(vector.VectorKind.ToString());

            vector.WriteToStream(writer);

            vectorNumber++;
        }

        writer.Write('}');
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

        int vectorCount = reader.ReadInt32();

        var vectors = new Dictionary<string, VectorBase>(vectorCount);

        for (int i = 0; i < vectorCount; i++)
        {
            if (i > 0)
            {
                // ","
                reader.ReadChar();
            }

            // "\"name\":"
            var namePart = reader.ReadString();

            var name = namePart[1..^2]; // Remove quotes and colon

            var vectorKindString = reader.ReadString();

            if (!Enum.TryParse<VectorKind>(vectorKindString, out var vectorKind))
            {
                throw new InvalidOperationException($"Vector kind '{vectorKindString}' is not supported");
            }

            VectorBase vector = vectorKind switch
            {
                VectorKind.Dense => DenseVector.ReadFromStream(reader),
                VectorKind.Sparse => SparseVector.ReadFromStream(reader),
                VectorKind.Multi => MultiVector.ReadFromStream(reader),
                VectorKind.Named => ReadFromStream(reader),
                _ => throw new InvalidOperationException($"Vector kind '{vectorKind}' is not supported")
            };

            vectors.Add(name, vector);
        }

        // "}"
        reader.ReadChar();

        return new NamedVectors()
        {
            Vectors = vectors
        };
    }

    /// <inheritdoc/>
    public override VectorBase this[string vectorName]
    {
        get {
            EnsureNotEmpty();

            if (Vectors.TryGetValue(vectorName, out var vector))
            {
                return vector;
            }

            throw new KeyNotFoundException($"Named vector {vectorName} for point is not found");
        }
    }

    private void EnsureNotEmpty()
    {
        if (Vectors.Count is 0)
        {
            throw new InvalidOperationException("Named vectors collection for point is empty");
        }
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

        return other is NamedVectors otherNamedVectors && Equals(otherNamedVectors);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj)
            || obj is NamedVectors other && Equals(other);
    }

    /// <inheritdoc/>
    public bool Equals(NamedVectors other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (Vectors.Count != other.Vectors.Count)
        {
            return false;
        }

        foreach (var (vectorName, vector) in Vectors)
        {
            if (!other.Vectors.TryGetValue(vectorName, out VectorBase otherVector))
            {
                return false;
            }

            if (!vector.Equals(otherVector))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (Vectors is null or { Count: 0 })
        {
            return 0;
        }

        HashCode hashCode = new();

        foreach (var (vectorName, vector) in Vectors)
        {
            hashCode.Add(vectorName);
            hashCode.Add(vector);
        }

        return hashCode.ToHashCode();
    }
}
