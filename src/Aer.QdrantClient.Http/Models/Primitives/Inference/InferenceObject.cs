using Aer.QdrantClient.Http.Infrastructure.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Models.Primitives.Inference;

/// <summary>
/// Represents an object and a model info that is used to generate vector from that object.
/// </summary>
public abstract class InferenceObject
{
    /// <summary>
    /// Gets the kind of the inference object that is represented by this <see cref="InferenceObject"/> instance.
    /// </summary>
    [JsonIgnore]
    public abstract InferenceObjectKind ObjectKind { get; }

    /// <summary>
    /// Name of the model used to generate the vector. List of available models depends on a provider.
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Additional options for the model, will be passed to the inference service as-is. See model cards for available options.
    /// </summary>
    public Dictionary<string, object> Options { get; init; }

    /// <summary>
    /// Creates a text inference object.
    /// </summary>
    /// <param name="text">Text of the document. This field will be used as input for the embedding model.</param>
    /// <param name="model">Name of the model used to generate the vector. List of available models depends on a provider.</param>
    /// <param name="options">Additional options for the model, will be passed to the inference service as-is. See model cards for available options.</param>
    /// <param name="bm25Options">Additional options for the BM25 model.
    /// If set, overrides <paramref name="options"/>.</param>
    public static InferenceObject CreateFromDocument(
        string text,
        string model,
        Dictionary<string, object> options = null,
        Bm25Config bm25Options = null) =>
        new DocumentInferenceObject()
        {
            Text = text,
            Model = model,
            Options = options,
            Bm25Options = bm25Options
        };

    /// <summary>
    /// Creates an image inference object.
    /// </summary>
    /// <param name="image">Image data: base64 encoded image or an URL.</param>
    /// <param name="model">Name of the model used to generate the vector. List of available models depends on a provider.</param>
    /// <param name="options">Additional options for the model, will be passed to the inference service as-is. See model cards for available options.</param>
    public static InferenceObject CreateFromImage(
        string image,
        string model,
        Dictionary<string, object> options = null) =>
        new ImageInferenceObject()
        {
            Image = image,
            Model = model,
            Options = options
        };

    /// <summary>
    /// Creates an arbitrary data object inference object.
    /// </summary>
    /// <param name="dataObject">Arbitrary data, used as input for the embedding model.
    /// Used if the model requires more than one input or a custom input.</param>
    /// <param name="model">Name of the model used to generate the vector. List of available models depends on a provider.</param>
    /// <param name="options">Additional options for the model, will be passed to the inference service as-is. See model cards for available options.</param>
    public static InferenceObject CreateFromObject(
        object dataObject,
        string model,
        Dictionary<string, object> options = null) =>
        new ObjectInferenceObject()
        {
            Object = dataObject,
            Model = model,
            Options = options
        };

    /// <inheritdoc/>
    public abstract override string ToString();

    private protected string ToStringCore()
    {
        StringBuilder sb = new();

        sb.AppendLine($"Model: \"{Model}\"");

        if (Options is null or { Count: 0 })
        {
            return sb.ToString();
        }

        sb.AppendLine("Options:");

        sb.Append(JsonSerializer.Serialize(Options, JsonSerializerConstants.DefaultSerializerOptions));

        return sb.ToString();
    }

    /// <summary>
    /// Writes the string inference object representation to the provided <see cref="StreamWriter"/>.
    /// </summary>
    /// <param name="writer">The writer to write inference object representation to.</param>
    public abstract void WriteToStream(StreamWriter writer);

    private protected void WriteToStreamCore(StreamWriter writer)
    {
        writer.WriteLine($"Model: \"{Model}\"");

        if (Options is null or { Count: 0 })
        {
            return;
        }

        writer.WriteLine("Options:");

        writer.Write(JsonSerializer.Serialize(Options, JsonSerializerConstants.DefaultSerializerOptions));
    }

    /// <inheritdoc/>
    public abstract override bool Equals(object other);

    private protected bool EqualsCore(InferenceObject other, bool isCompareOptions = true)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (!Model.Equals(Model))
        {
            return false;
        }

        if (isCompareOptions)
        {
            if (Options == null && other.Options == null)
            {
                return true;
            }

            if (Options == null && other.Options != null)
            {
                return false;
            }

            if (Options.Count == 0 && other.Options.Count == 0)
            {
                return true;
            }

            if (Options.Count != other.Options.Count)
            {
                return false;
            }

            foreach (var option in Options)
            {
                if (!other.Options.TryGetValue(option.Key, out var otherValue))
                {
                    return false;
                }

                if (!otherValue.Equals(option.Value))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public abstract override int GetHashCode();

    private protected int GetHashCodeCore(bool isComputeOptionsHashCode = true)
    {
        HashCode hashCode = new();

        hashCode.Add(Model);

        if (isComputeOptionsHashCode
            && Options is not null)
        {
            foreach (var optionKey in Options.Keys.OrderBy(k => k))
            {
                hashCode.Add(optionKey, StringComparer.Ordinal);
                hashCode.Add(Options[optionKey]);
            }
        }

        return hashCode.ToHashCode();
    }
}
