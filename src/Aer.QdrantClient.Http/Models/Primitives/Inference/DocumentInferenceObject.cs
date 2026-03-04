using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Inference;

/// <summary>
/// Represents a text and inference model name and settings.
/// </summary>
public sealed class DocumentInferenceObject : InferenceObject, IEquatable<InferenceObject>, IEquatable<DocumentInferenceObject>
{
    private static readonly JsonSerializerOptions _serializerOptions =
        JsonSerializerConstants.CreateSerializerOptions(new FullTextIndexStemmingAlgorithmJsonConverter());

    /// <inheritdoc/>
    [JsonIgnore]
    public override InferenceObjectKind ObjectKind => InferenceObjectKind.Document;

    /// <summary>
    /// Text of the document. This field will be used as input for the embedding model.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Additional options for the BM25 model.
    /// If set, overrides <see cref="InferenceObject.Options"/>.
    /// </summary>
    public Bm25Config Bm25Options { get; init; }

    /// <inheritdoc/>
    public override string ToString() => JsonSerializer.Serialize(this, _serializerOptions);

    /// <inheritdoc/>
    public override void WriteToStream(StreamWriter writer)
    {
        JsonSerializer.Serialize(writer.BaseStream, this, _serializerOptions);
    }

    /// <inheritdoc/>
    public bool Equals(InferenceObject other)
    {
        // We compare Options components only if this vector does not have Bm25Options
        return EqualsCore(other, isCompareOptions: Bm25Options is null)
            && other is DocumentInferenceObject o
            && Equals(o);
    }

    /// <inheritdoc/>
    public bool Equals(DocumentInferenceObject other)
    {
        // We compare Options components only if this vector does not have Bm25Options
        if (!EqualsCore(other, isCompareOptions: Bm25Options is null))
        {
            return false;
        }

        if (!Text.Equals(other.Text))
        {
            return false;
        }

        if (Bm25Options is null && other.Bm25Options is null)
        {
            return true;
        }

        return Bm25Options.Equals(other.Bm25Options);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is DocumentInferenceObject o && Equals(o);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hashCode = new();

        hashCode.Add(Text);

        if (Bm25Options is not null)
        {
            hashCode.Add(Bm25Options);
        }

        // We compute Options hash code only if this instance does not have Bm25Options defined
        return HashCode.Combine(
            GetHashCodeCore(isComputeOptionsHashCode: Bm25Options is null),
            hashCode.ToHashCode());
    }
}
