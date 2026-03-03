using System.Text;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Inference;

/// <summary>
/// Represents an image and inference model name and settings.
/// </summary>
public sealed class ImageInferenceObject : InferenceObject, IEquatable<InferenceObject>, IEquatable<ImageInferenceObject>
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override InferenceObjectKind ObjectKind => InferenceObjectKind.Image;

    /// <summary>
    /// Image data: base64 encoded image or an URL
    /// </summary>
    public required string Image { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine($$"""
            {
            Image: {{Image}}
            """);

        sb.AppendLine(ToStringCore());

        sb.Append('}');

        return sb.ToString();
    }

    /// <inheritdoc/>
    public override void WriteToStream(StreamWriter writer)
    {
        writer.WriteLine($$"""
            {
            Image: {{Image}}
            """);

        WriteToStreamCore(writer);

        writer.Write('}');
    }

    /// <inheritdoc/>
    public bool Equals(InferenceObject other)
    {
        return EqualsCore(other)
            && other is ImageInferenceObject o
            && Equals(o);
    }

    /// <inheritdoc/>
    public bool Equals(ImageInferenceObject other)
    {
        if (!EqualsCore(other))
        {
            return false;
        }

        return Image.Equals(other.Image);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is ImageInferenceObject o && Equals(o);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hashCode = new();

        hashCode.Add(Image);

        return HashCode.Combine(
            GetHashCodeCore(),
            hashCode.ToHashCode());
    }
}
