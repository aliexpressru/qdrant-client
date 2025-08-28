using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents the constant double of string value.
/// </summary>
internal sealed class ConstantExpression : ExpressionBase
{
	private readonly double _valueNumber;
	private readonly string _valueString;
	private readonly bool _isNumber;
	
	public ConstantExpression(double value)
	{
		_valueNumber = value;
		_isNumber = true;
	}
	
	public ConstantExpression(string value)
	{
		_valueString = value;
		_isNumber = false;
	}
	
	public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
	{
		if (_isNumber)
		{
			jsonWriter.WriteNumberValue(_valueNumber);
		}
		else
		{
			jsonWriter.WriteStringValue(_valueString);
		}
	}
}
