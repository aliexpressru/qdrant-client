// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Primitives.Vectors;

/// <summary>
/// Represents a sparse vector.
/// </summary>
public class SparseVector : VectorBase
{
    /// <summary>
    /// Gets the positions of the non-zero values in the sparse vector.
    /// </summary>
    public HashSet<uint> Indices { get; init; }

    /// <summary>
    /// Gets the values of the non-zero sparse vector elements.
    /// </summary>
    public float[] Values { get; init; }

    public override float[] Default => throw new NotImplementedException();

    public override float[] this[string vectorName] => throw new NotImplementedException();

    public override float[] FirstOrDefault()
    {
        throw new NotImplementedException();
    }

    public override bool ContainsVector(string vectorName)
    {
        throw new NotImplementedException();
    }
}
