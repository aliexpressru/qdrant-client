namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared.Inference;

/// <summary>
/// Represents an arbitrary data and inference model name and settings.
/// </summary>
public sealed class ObjectInferenceObject : InferenceObject
{
    /// <summary>
    /// Arbitrary data, used as input for the embedding model.
    /// Used if the model requires more than one input or a custom input.
    /// </summary>
    public required object Object { get; init; }
}
