namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Reresents the location used to lookup vectors.
/// </summary>
public class VectorsLookupLocation
{
    /// <summary>
    /// The name of the collection to lookup vectors in.
    /// </summary>
    public string Collection { set; get; }

    /// <summary>
    /// Optional name of the vector field within the collection.
    /// If not provided, the default vector field will be used.
    /// </summary>
    public string Vector { set; get; }
}
