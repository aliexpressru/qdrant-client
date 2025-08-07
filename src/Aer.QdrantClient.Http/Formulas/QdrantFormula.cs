using System.Diagnostics.CodeAnalysis;
using System.Text;
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
	/// Creates the qdrant formula instance from a formula expression.
	/// </summary>
	/// <param name="expression">The expression to create formula from.</param>
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
			_rawExpressionString = formula
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

		_expression.WriteExpressionJson(jsonWriter);
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(true);

	/// <summary>
	/// Build qdrant formula string representation. For debug and testing purposes.
	/// </summary>
	/// <param name="isIndentFormulaSyntax">Determines whether the resulting formula string should be indented. Default value is <c>false</c>.</param>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public string ToString(bool isIndentFormulaSyntax)
	{
		if (!string.IsNullOrWhiteSpace(_rawExpressionString))
		{
			return _rawExpressionString;
		}
		
		using var stream = new MemoryStream();

		Utf8JsonWriter jsonWriter = new Utf8JsonWriter(
			stream,
			new JsonWriterOptions()
			{
				Indented = isIndentFormulaSyntax,
				SkipValidation = true
			});

		WriteFormulaJson(jsonWriter);

		jsonWriter.Flush();

		var builtFormula = Encoding.UTF8.GetString(stream.ToArray());
		
		return builtFormula;
	}
}
