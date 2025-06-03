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
		=>
			new ConstantExpression(value);

	/// <summary>
	/// Create a constant double value formula.
	/// </summary>
	/// <param name="value">The constant value.</param>
	public static ExpressionBase Constant(double value)
		=>
			new ConstantExpression(value);

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
		=>
			new PrefetchScoreReferenceExpression(prefetchNumber);

	/// <summary>
	/// Reference to the score of the point in the single prefetch expression.
	/// </summary>
	public static ExpressionBase PrefetchReference()
		=>
			new PrefetchScoreReferenceExpression(0);

	/// <summary>
	/// Multiply an array of expressions.
	/// </summary>
	/// <param name="expressions">The expression results to multiply.</param>
	public static ExpressionBase Multiply(params ICollection<ExpressionBase> expressions)
		=>
			new CollectionExpression("mult", expressions);

	/// <summary>
	/// Sum an array of expressions.
	/// </summary>
	/// <param name="expressions">The expression results to sum.</param>
	public static ExpressionBase Sum(params ICollection<ExpressionBase> expressions)
		=>
			new CollectionExpression("sum", expressions);

	/// <summary>
	/// Divide an expression by another expression.
	/// </summary>
	/// <param name="left">Expression to divide.</param>
	/// <param name="right">Expression to divide to.</param>
	/// <param name="divideByZeroDefaultValue">The optional value for cases when division by zero is detected.</param>
	public static ExpressionBase Divide(
		ExpressionBase left,
		ExpressionBase right,
		double? divideByZeroDefaultValue = null)
		=>
			new DivideExpression(left, right, divideByZeroDefaultValue);

	/// <summary>
	/// Raise an expression to the power of another expression.
	/// </summary>
	/// <param name="baseExpression">The base expression to exponentiate.</param>
	/// <param name="exponentExpression">The power expression.</param>
	public static ExpressionBase Power(ExpressionBase baseExpression, ExpressionBase exponentExpression)
		=>
			new PowerExpression(baseExpression, exponentExpression);

	/// <summary>
	/// Absolute value of an expression.
	/// </summary>
	/// <param name="expression">The target expression.</param>
	public static ExpressionBase Abs(ExpressionBase expression)
		=>
			new UnaryExpression("abs", expression);

	/// <summary>
	/// Negates the value of expression.
	/// </summary>
	/// <param name="expression">The target expression.</param>
	public static ExpressionBase Negate(ExpressionBase expression)
		=>
			new UnaryExpression("neg", expression);

	/// <summary>
	/// Exponential function of an expression (e^x).
	/// </summary>
	/// <param name="expression">The target expression.</param>
	public static ExpressionBase Exponent(ExpressionBase expression)
		=> new UnaryExpression("exp", expression);

	/// <summary>
	/// Square root of an expression.
	/// </summary>
	/// <param name="expression">The target expression.</param>
	public static ExpressionBase Sqrt(ExpressionBase expression)
		=>
			new UnaryExpression("sqrt", expression);

	/// <summary>
	/// Base 10 logarithm of an expression.
	/// </summary>
	/// <param name="expression">The target expression.</param>
	public static ExpressionBase Log10(ExpressionBase expression)
		=>
			new UnaryExpression("log10", expression);

	/// <summary>
	/// Natural logarithm of an expression.
	/// </summary>
	/// <param name="expression">The target expression.</param>
	public static ExpressionBase Ln(ExpressionBase expression)
		=>
			new UnaryExpression("ln", expression);

	/// <summary>
	/// Parse a datetime string (see formats https://qdrant.tech/documentation/concepts/payload/#datetime), and use it as a POSIX timestamp, in seconds.
	/// </summary>
	/// <param name="dateTime">The datetime string to parse.</param>
	public static ExpressionBase DateTimeValue(string dateTime)
		=>
			new ValueExpression("datetime", dateTime);

	/// <summary>
	/// Specify that a payload key contains a datetime string to be parsed into POSIX seconds.
	/// </summary>
	/// <param name="dateTimePayloadFieldName">The datetime payload field name.</param>
	public static ExpressionBase DateTimeKey(string dateTimePayloadFieldName)
		=>
			new ValueExpression("datetime_key", dateTimePayloadFieldName);
}
