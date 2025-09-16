namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// The type of the multivector comparer.
/// </summary>
public enum MultivectorComparator
{
    /// <summary>
    /// Comparator that uses a sum of maximum similarities between each pair of vectors in the matrices.
    /// </summary>
    MaxSim
}
