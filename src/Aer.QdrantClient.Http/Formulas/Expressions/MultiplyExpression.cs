using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a multiplication expression that multiplies results of the multiple expressions.
/// </summary>
internal class MultiplyExpression : ExpressionBase
{
	private readonly ICollection<ExpressionBase> _expressions;

	public MultiplyExpression(params ICollection<ExpressionBase> expressions)
	{ 
		_expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
	}

	public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
	{
		jsonWriter.WriteStartObject();

		jsonWriter.WritePropertyName("mult");
		
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
