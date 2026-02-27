namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared.Inference;

/// <summary>
/// Represents an arbitrary data and inference model name and settings.
/// </summary>
public class ObjectInferenceObject : InferenceObjectBase
{
    /// <summary>
    /// Arbitrary data, used as input for the embedding model.
    /// Used if the model requires more than one input or a custom input.
    /// </summary>
    public object Object { get; init; }
}
