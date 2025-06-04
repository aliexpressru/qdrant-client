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
					$"Filter group conditions like {condition.GetType()} can't be used in formulas"),
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

	/// <summary>
	/// Haversine distance between two geographic points.
	/// </summary>
	/// <param name="originLongitude">The origin point longitude.</param>
	/// <param name="originLatitude">The origin point latitude.</param>
	/// <param name="toPayloadFieldName">The target point payload field name. Filed must contain the geo coordinate object.</param>
	public static ExpressionBase GeoDistance(double originLongitude, double originLatitude, string toPayloadFieldName)
		=>
			new GeoDistanceExpression(originLongitude, originLatitude, toPayloadFieldName);

	/// <summary>
	/// Apply a linear decay function to an expression, which clamps the output between 0 and 1
	/// </summary>
	/// <param name="x">The value to decay.</param>
	/// <param name="target">
	/// The value at which the decay will be at its peak. For distances, it is usually set at 0.0, but can be set to any value.
	/// If not set defaults to 0.0.
	/// </param>
	/// <param name="scale">
	/// The value at which the decay function will be equal to midpoint. This is in terms of x units,
	/// for example, if x is in meters, scale of 5000 means 5km. Must be a non-zero positive number.
	/// If not set, defaults to 1.0.
	/// </param>
	/// <param name="midpoint">
	/// Output is midpoint when x equals scale. Must be in the range (0.0, 1.0), exclusive.
	/// If not set, defaults to 0.5.
	/// </param>
	public static ExpressionBase LinearDecay(
		ExpressionBase x,
		ExpressionBase target = null,
		double? scale = null,
		double? midpoint = null)
		=>
			new DecayExpression("lin_decay", x, target, scale, midpoint);

	/// <summary>
	/// Apply an exponential decay function to an expression, which clamps the output between 0 and 1
	/// </summary>
	/// <param name="x">The value to decay.</param>
	/// <param name="target">
	/// The value at which the decay will be at its peak. For distances, it is usually set at 0.0, but can be set to any value.
	/// If not set defaults to 0.0.
	/// </param>
	/// <param name="scale">
	/// The value at which the decay function will be equal to midpoint. This is in terms of x units,
	/// for example, if x is in meters, scale of 5000 means 5km. Must be a non-zero positive number.
	/// If not set, defaults to 1.0.
	/// </param>
	/// <param name="midpoint">
	/// Output is midpoint when x equals scale. Must be in the range (0.0, 1.0), exclusive.
	/// If not set, defaults to 0.5.
	/// </param>
	public static ExpressionBase ExponentialDecay(
		ExpressionBase x,
		ExpressionBase target = null,
		double? scale = null,
		double? midpoint = null)
		=>
			new DecayExpression("exp_decay", x, target, scale, midpoint);

	/// <summary>
	/// Apply a gaussean decay function to an expression, which clamps the output between 0 and 1
	/// </summary>
	/// <param name="x">The value to decay.</param>
	/// <param name="target">
	/// The value at which the decay will be at its peak. For distances, it is usually set at 0.0, but can be set to any value.
	/// If not set defaults to 0.0.
	/// </param>
	/// <param name="scale">
	/// The value at which the decay function will be equal to midpoint. This is in terms of x units,
	/// for example, if x is in meters, scale of 5000 means 5km. Must be a non-zero positive number.
	/// If not set, defaults to 1.0.
	/// </param>
	/// <param name="midpoint">
	/// Output is midpoint when x equals scale. Must be in the range (0.0, 1.0), exclusive.
	/// If not set, defaults to 0.5.
	/// </param>
	public static ExpressionBase GaussDecay(
		ExpressionBase x,
		ExpressionBase target = null,
		double? scale = null,
		double? midpoint = null)
		=>
			new DecayExpression("gauss_decay", x, target, scale, midpoint);
}
