using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// The base class for all expression types used in Qdrant formulas.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public abstract class ExpressionBase
{
	/// <summary>
	/// Write out the expression json to specified writer.
	/// </summary>
	/// <param name="jsonWriter">The json writer to write expression json to.</param>
	public abstract void WriteExpressionJson(Utf8JsonWriter jsonWriter);
	
	/// <summary>
	/// Implicit conversion from string to ExpressionBase.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	public static implicit operator ExpressionBase(string value) 
		=> new ConstantExpression(value);

	/// <summary>
	/// Implicit conversion from double to ExpressionBase.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	public static implicit operator ExpressionBase(double value)
		=> new ConstantExpression(value);
}
