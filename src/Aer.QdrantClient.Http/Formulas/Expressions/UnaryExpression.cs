using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a unary expression in Qdrant formulas.
/// </summary>
internal sealed class UnaryExpression : ExpressionBase
{
	private readonly string _operator;
	private readonly ExpressionBase _operand;

	public UnaryExpression(string unaryOperator, ExpressionBase operand)
	{
		_operator = unaryOperator;
		_operand = operand;
	}
	
	public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
	{
		jsonWriter.WriteStartObject();
		{
			jsonWriter.WritePropertyName(_operator);

			_operand.WriteExpressionJson(jsonWriter);
		}
		jsonWriter.WriteEndObject();
	}
}
