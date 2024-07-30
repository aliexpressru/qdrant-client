namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents recommend points strategy. Affects the recommendation output.
/// </summary>
public enum RecommendStrategy
{
    /// <summary>
    /// Average positive and negative vectors and create a single query with the formula
    /// <c>query = avg_pos + avg_pos - avg_neg</c>. Then performs normal search.
    /// This is a default recommend points strategy.
    /// </summary>
    AverageVector,

    /// <summary>
    /// Uses custom search objective. Each candidate is compared against all examples,
    /// its score is then chosen from the <c>max(max_pos_score, max_neg_score)</c>.
    /// If the <c>max_neg_score</c> is chosen then it is squared and negated,
    /// otherwise it is just the <c>max_pos_score</c>.
    /// </summary>
    BestScore
}
