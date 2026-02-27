namespace Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints.RelevanceFeedback;

internal class NaiveFeedbackStrategy : FeedbackStrategy
{
    public required double A { get; init; }

    public required double B { get; init; }

    public required double C { get; init; }
}
