namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared.Inference;

/// <summary>
/// Represents an object and a model info that is used to generate vector from that object.
/// </summary>
public abstract class InferenceObjectBase
{
    /// <summary>
    /// Name of the model used to generate the vector. List of available models depends on a provider.
    /// </summary>
    public string Model { get; init; }

    /// <summary>
    /// Additional options for the model, will be passed to the inference service as-is. See model cards for available options.
    /// </summary>
    public Dictionary<string, object> Options { get; init; }
}
