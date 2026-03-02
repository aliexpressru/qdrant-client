namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared.Inference;

/// <summary>
/// Represents a text and inference model name and settings.
/// </summary>
public sealed class DocumentInferenceObject : InferenceObject
{
    /// <summary>
    /// Text of the document. This field will be used as input for the embedding model.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Additional options for the BM25 model.
    /// If set, overrides <see cref="InferenceObject.Options"/>.
    /// </summary>
    public Bm25Config Bm25Options { get; init; }
}
