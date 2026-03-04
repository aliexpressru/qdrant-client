namespace Aer.QdrantClient.Http.Models.Primitives.Inference;

/// <summary>
/// Represents an inference object kind.
/// </summary>
public enum InferenceObjectKind
{
    /// <summary>
    /// A text inference object.
    /// </summary>
    Document,

    /// <summary>
    /// An image inference object.
    /// </summary>
    Image,

    /// <summary>
    /// An arbitrary data inference object.
    /// </summary>
    Object
}
