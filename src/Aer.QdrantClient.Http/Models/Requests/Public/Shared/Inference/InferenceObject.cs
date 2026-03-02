namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared.Inference;

/// <summary>
/// Represents an object and a model info that is used to generate vector from that object.
/// </summary>
public abstract class InferenceObject
{
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
    /// <param name="object">Arbitrary data, used as input for the embedding model.
    /// Used if the model requires more than one input or a custom input.</param>
    /// <param name="model">Name of the model used to generate the vector. List of available models depends on a provider.</param>
    /// <param name="options">Additional options for the model, will be passed to the inference service as-is. See model cards for available options.</param>
    public static InferenceObject CreateFromObject(
        object @object,
        string model,
        Dictionary<string, object> options = null) =>
        new ObjectInferenceObject()
        {
            Object = @object,
            Model = model,
            Options = options
        };
}
