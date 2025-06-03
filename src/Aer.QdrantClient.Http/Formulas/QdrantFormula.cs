using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Aer.QdrantClient.Http.Formulas.Expressions;

namespace Aer.QdrantClient.Http.Formulas;

/// <summary>
/// Represents a formula used to calculate a score boost.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantFormula
{
	private ExpressionBase _expression;
	private string _rawExpressionString;

	/// <summary>
	/// Creates the qdrant formula instance from formula expression.
	/// </summary>
	/// <param name="expression">The expression to create formula from.</param>
	/// <returns></returns>
	public static QdrantFormula Create(ExpressionBase expression)
	{ 
		if (expression is null)
		{
			throw new ArgumentNullException(nameof(expression));
		}

		return new QdrantFormula { _expression = expression };
	}

	/// <summary>
	/// Creates the qdrant formula instance directly from a formula string.
	/// </summary>
	public static QdrantFormula Create(string formula)
	{
		if (string.IsNullOrWhiteSpace(formula))
		{
			throw new ArgumentNullException(nameof(formula));
		}

		QdrantFormula ret = new()
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1
            _rawExpressionString = formula
#else
			_rawExpressionString = formula.ReplaceLineEndings()
#endif
		};
		
		return ret;
	}

	/// <summary>
	/// Creates a qdrant formula from a formula expression.
	/// </summary>
	/// <param name="expression">The formula expression to create formula from.</param>
	public static implicit operator QdrantFormula(ExpressionBase expression)
		=> Create(expression);

	internal void WriteFormulaJson(Utf8JsonWriter jsonWriter)
	{
		if (!string.IsNullOrWhiteSpace(_rawExpressionString))
		{
			jsonWriter.WriteRawValue(_rawExpressionString);
			return;
		}

		// jsonWriter.WriteStartObject();

		_expression.WriteExpressionJson(jsonWriter);

		// jsonWriter.WriteEndObject();
	}
}
