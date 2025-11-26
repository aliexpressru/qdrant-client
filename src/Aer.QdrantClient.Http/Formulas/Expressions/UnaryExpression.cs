using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a unary expression in Qdrant formulas.
/// </summary>
internal sealed class UnaryExpression(string unaryOperator, ExpressionBase operand) : ExpressionBase
{
    public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WriteStartObject();
        {
            jsonWriter.WritePropertyName(unaryOperator);

            operand.WriteExpressionJson(jsonWriter);
        }
        jsonWriter.WriteEndObject();
    }
}
