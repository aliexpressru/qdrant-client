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

        return new DenseVector() {VectorValues = values};
    }
}
