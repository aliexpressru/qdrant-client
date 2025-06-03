using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a sum expression that sums results of the multiple expressions.
/// </summary>
internal class SumExpression : ExpressionBase
{
	private readonly ICollection<ExpressionBase> _expressions;

	public SumExpression(params ICollection<ExpressionBase> expressions)
	{ 
		_expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
	}

	public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
	{
		jsonWriter.WriteStartObject();

		jsonWriter.WritePropertyName("sum");
		
		jsonWriter.WriteStartArray();
		
		foreach (var expression in _expressions)
		{
			if (expression is null)
			{
				throw new ArgumentNullException(nameof(expression), "Expression cannot be null.");
			}

			expression.WriteExpressionJson(jsonWriter);
		}
		
		jsonWriter.WriteEndArray();
		
		jsonWriter.WriteEndObject();
	}
}
