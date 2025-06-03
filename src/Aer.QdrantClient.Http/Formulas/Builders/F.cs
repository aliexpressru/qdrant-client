using Aer.QdrantClient.Http.Filters.Builders;
using Aer.QdrantClient.Http.Filters.Conditions;
using Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;
using Aer.QdrantClient.Http.Formulas.Expressions;

namespace Aer.QdrantClient.Http.Formulas.Builders;

/// <summary>
/// Helper class for building formulas.
/// </summary>
public static class F
{
	/// <summary>
	/// Create a constant string value formula.
	/// </summary>
	/// <param name="value">The constant value.</param>
	public static ExpressionBase Constant(string value) 
		=> new ConstantExpression(value);

	/// <summary>
	/// Create a constant double value formula.
	/// </summary>
	/// <param name="value">The constant value.</param>
	public static ExpressionBase Constant(double value)
		=> new ConstantExpression(value);

	/// <summary>
	/// Create a filter condition formula. Filter condition is a restricted set of standard Qdrant conditions.
	/// Filter conditions are built using <see cref="Q"/> builder class.
	/// If the condition is met, it becomes 1.0, otherwise 0.0
	/// </summary>
	/// <param name="condition">The filter condition.</param>
	public static ExpressionBase Filter(FilterConditionBase condition)
	{
		var ret = condition switch
		{
			FilterGroupCondition
				or MinimumShouldCondition
				or MustCondition
				or MustNotCondition
				or NestedCondition
				or ShouldCondition => throw new InvalidOperationException(
					"Filter group conditions can't be used in formulas"),
			_ => new FilterConditionExpression(condition)
		};
		
		return ret;
	}
	
	/// <summary>
	/// Reference to the score of the point in the specific prefetch expression.
	/// </summary>
	/// <param name="prefetchNumber">The prefetch number to reference to.</param>
	public static ExpressionBase PrefetchReference(uint prefetchNumber)
		=> new PrefetchScoreReferenceExpression(prefetchNumber);

	/// <summary>
	/// Reference to the score of the point in the single prefetch expression.
	/// </summary>
	public static ExpressionBase PrefetchReference()
		=> new PrefetchScoreReferenceExpression(0);
	
	/// <summary>
	/// Multiply an array of expressions.
	/// </summary>
	/// <param name="expressions">The expression results to multiply.</param>
	public static ExpressionBase Multiply(params ICollection<ExpressionBase> expressions)
		=> new MultiplyExpression(expressions);

	/// <summary>
	/// Sum an array of expressions.
	/// </summary>
	/// <param name="expressions">The expression results to sum.</param>
	public static ExpressionBase Sum(params ICollection<ExpressionBase> expressions)
		=> new SumExpression(expressions);
}
