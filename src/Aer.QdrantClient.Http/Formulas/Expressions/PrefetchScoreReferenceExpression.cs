using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents the reference to the score of the point in the prefetch.
/// </summary>
internal sealed class PrefetchScoreReferenceExpression : ExpressionBase
{
	private readonly string _referenceValue;
	
	public PrefetchScoreReferenceExpression(uint prefetchNumber)
	{
		_referenceValue = prefetchNumber == 0
			? "$score"
			: $"$score[{prefetchNumber}]";
	}
	
	public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
	{
		jsonWriter.WriteStringValue(_referenceValue);
	}
}
