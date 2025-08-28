using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a simple name-value expression.
/// </summary>
internal sealed class ValueExpression : ExpressionBase
{
	private readonly string _valueName;
	private readonly string _value;

	public ValueExpression(string valueName, string value)
	{
		_valueName = valueName ?? throw new ArgumentNullException(nameof(valueName));
		_value = value ?? throw new ArgumentNullException(nameof(value));
	}

	public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
	{
		jsonWriter.WriteStartObject();
		{
			jsonWriter.WritePropertyName(_valueName);
			jsonWriter.WriteStringValue(_value);
		}
		jsonWriter.WriteEndObject();
	}
}
