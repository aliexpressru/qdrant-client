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
}
