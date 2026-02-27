namespace Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints.RelevanceFeedback;

/// <summary>
/// Represents a base class with factory methods for feedback strategies.
/// </summary>
public class FeedbackStrategy
{
    /// <summary>
    /// Creates na instance of naive feedback strategy.
    /// </summary>
    /// <param name="a">The a parameter.</param>
    /// <param name="b">The b parameter.</param>
    /// <param name="c">The c parameter.</param>
    public static FeedbackStrategy Naive(double a, double b, double c) =>
        new NaiveFeedbackStrategy()
        {
            A = a,
            B = b,
            C = c
        };
}
