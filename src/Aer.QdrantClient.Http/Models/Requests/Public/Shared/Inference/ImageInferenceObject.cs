namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared.Inference;

/// <summary>
/// Represents an image and inference model name and settings.
/// </summary>
public class ImageInferenceObject : InferenceObjectBase
{
    /// <summary>
    /// Image data: base64 encoded image or an URL
    /// </summary>
    public string Image { get; init; }
}
