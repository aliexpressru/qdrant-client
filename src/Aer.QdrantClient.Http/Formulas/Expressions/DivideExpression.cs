using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a divide expression that divides left part to its right optionally specifying divide by zero default value.
/// </summary>
internal sealed class DivideExpression(ExpressionBase left, ExpressionBase right, double? divideByZeroDefaultValue = null) : ExpressionBase
{
    private readonly ExpressionBase _left = left ?? throw new ArgumentNullException(nameof(left));
    private readonly ExpressionBase _right = right ?? throw new ArgumentNullException(nameof(right));
    private readonly double _divideByZeroDefaultValue = divideByZeroDefaultValue ?? 0;

    public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WriteStartObject();
        {
            jsonWriter.WritePropertyName("div");

            jsonWriter.WriteStartObject();
            {
                jsonWriter.WritePropertyName("left");

                _left.WriteExpressionJson(jsonWriter);

                jsonWriter.WritePropertyName("right");

                _right.WriteExpressionJson(jsonWriter);

                jsonWriter.WritePropertyName("by_zero_default");

                jsonWriter.WriteNumberValue(_divideByZeroDefaultValue);
            }
            jsonWriter.WriteEndObject();
        }
        jsonWriter.WriteEndObject();
    }
}
