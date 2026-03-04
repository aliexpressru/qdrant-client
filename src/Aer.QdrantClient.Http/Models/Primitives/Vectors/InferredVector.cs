using Aer.QdrantClient.Http.Models.Primitives.Inference;
using System.Text;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a vector which value will be inferred from specified inference object processed by specified model.
/// </summary>
public sealed class InferredVector : VectorBase, IEquatable<VectorBase>, IEquatable<InferredVector>
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override VectorKind VectorKind => VectorKind.Inferred;

    /// <summary>
    /// Represents an inference object from which a vector value will be obtained.
    /// </summary>
    public InferenceObject InferenceObject { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override VectorBase Default => this;

    /// <summary>
    /// Initializes a new instance of <see cref="InferredVector"/> from provided inference object.
    /// </summary>
    /// <param name="inferenceObject">The source inference object.</param>
    [JsonConstructor]
    public InferredVector(InferenceObject inferenceObject)
    {
        if (inferenceObject is null)
        {
            throw new ArgumentException($"{nameof(inferenceObject)} can't be null", nameof(inferenceObject));
        }

        InferenceObject = inferenceObject;
    }

    /// <inheritdoc/>
    public override VectorBase this[string vectorName] =>
        throw new NotSupportedException($"Vector names are not supported for inferred vector values {GetType()}");

    /// <inheritdoc/>
    public override VectorBase FirstOrDefault() => Default;

    /// <inheritdoc/>
    public override bool ContainsVector(string vectorName) =>
        throw new NotSupportedException($"Vector names are not supported for single vector values {GetType()}");

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine("{");

        sb.AppendLine("\"InferenceObject\": {");

        sb.AppendLine(InferenceObject.ToString());

        sb.AppendLine("}");

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <inheritdoc/>
    public override void WriteToStream(StreamWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        writer.Write("{\"InferenceObject\": {");

        InferenceObject.WriteToStream(writer);

        writer.Write("}}");
    }

    /// <summary>
    /// Implicitly converts an inference object to an inferred vector instance.
    /// </summary>
    /// <param name="inferenceObject">The inference object to create vector from.</param>
    public static implicit operator InferredVector(InferenceObject inferenceObject) =>
        new(inferenceObject);

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

        return other is InferredVector inferredVector && Equals(inferredVector);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj)
            || (obj is InferredVector inferredVector && Equals(inferredVector));
    }

    /// <inheritdoc/>
    bool Equals(InferredVector other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other)
            || InferenceObject.Equals(other.InferenceObject);
    }

    /// <inheritdoc/>
    bool IEquatable<InferredVector>.Equals(InferredVector other) => Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => InferenceObject.GetHashCode();
}
