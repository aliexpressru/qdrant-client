using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a power expression that raises base expression to the power of another expression.
/// </summary>
internal class PowerExpression : ExpressionBase
{
	private readonly ExpressionBase _base;
	private readonly ExpressionBase _exponent;

	public PowerExpression(ExpressionBase baseExpression, ExpressionBase exponentExpression)
	{
		_base = baseExpression ?? throw new ArgumentNullException(nameof(baseExpression));
		_exponent = exponentExpression ?? throw new ArgumentNullException(nameof(exponentExpression));
	}

	public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
	{
		jsonWriter.WriteStartObject();
		{
			jsonWriter.WritePropertyName("pow");

			jsonWriter.WriteStartObject();
			{
				jsonWriter.WritePropertyName("base");

				_base.WriteExpressionJson(jsonWriter);

				jsonWriter.WritePropertyName("exponent");

				_exponent.WriteExpressionJson(jsonWriter);
			}
			jsonWriter.WriteEndObject();
		}
		jsonWriter.WriteEndObject();
	}
}
