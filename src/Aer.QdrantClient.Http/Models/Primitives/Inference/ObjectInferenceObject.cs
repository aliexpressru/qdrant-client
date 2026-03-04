using Aer.QdrantClient.Http.Infrastructure.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Inference;

/// <summary>
/// Represents an arbitrary data and inference model name and settings.
/// </summary>
public sealed class ObjectInferenceObject : InferenceObject, IEquatable<InferenceObject>, IEquatable<ObjectInferenceObject>
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override InferenceObjectKind ObjectKind => InferenceObjectKind.Object;

    /// <summary>
    /// Arbitrary data, used as input for the embedding model.
    /// Used if the model requires more than one input or a custom input.
    /// </summary>
    public required object Object { get; init; }

    /// <inheritdoc/>
    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializerConstants.DefaultSerializerOptions);

    /// <inheritdoc/>
    public override void WriteToStream(StreamWriter writer)
    {
        JsonSerializer.Serialize(writer.BaseStream, this, JsonSerializerConstants.DefaultSerializerOptions);
    }

    /// <inheritdoc/>
    public bool Equals(InferenceObject other)
    {
        return EqualsCore(other)
            && other is ObjectInferenceObject o
            && Equals(o);
    }

    /// <inheritdoc/>
    public bool Equals(ObjectInferenceObject other)
    {
        if (!EqualsCore(other))
        {
            return false;
        }

        return Object.Equals(other.Object);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is ObjectInferenceObject o && Equals(o);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hashCode = new();

        hashCode.Add(Object);

        return HashCode.Combine(
            GetHashCodeCore(),
            hashCode.ToHashCode());
    }
}
