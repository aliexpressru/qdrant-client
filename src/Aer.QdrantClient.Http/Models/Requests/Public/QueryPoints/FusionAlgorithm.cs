namespace Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints;

/// <summary>
/// The fusion algorithm type. Fusion algorithm allows to combine results of multiple prefetches.
/// </summary>
public enum FusionAlgorithm
{
    /// <summary>
    /// Rank Reciprocal Fusion. Considers the positions of results within each query,
    /// and boosts the ones that appear closer to the top in multiple of them.
    /// </summary>
    Rrf,

    /// <summary>
    /// Distribution-Based Score Fusion. Normalizes the scores of the points in each query,
    /// using the mean +/- the 3rd standard deviation as limits, and then
    /// sums the scores of the same point across different queries.
    /// </summary>
    /// <remarks>
    /// Dbsf is stateless and calculates the normalization limits only based
    /// on the results of each query, not on all the scores that it has seen.
    /// </remarks>
    Dbsf
}
