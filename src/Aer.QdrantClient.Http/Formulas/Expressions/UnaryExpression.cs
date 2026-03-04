using Aer.QdrantClient.Http.Infrastructure.Helpers;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a unary expression in Qdrant formulas.
/// </summary>
internal sealed class UnaryExpression(string unaryOperator, ExpressionBase operand) : ExpressionBase
{
    public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
    {
        using (jsonWriter.WriteObject())
        {
            jsonWriter.WritePropertyName(unaryOperator);

            operand.WriteExpressionJson(jsonWriter);
        }
    }
}
