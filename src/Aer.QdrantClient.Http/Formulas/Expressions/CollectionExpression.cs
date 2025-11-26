using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a collection expression in Qdrant formulas.
/// </summary>
internal sealed class CollectionExpression(string collectionOperator, params ICollection<ExpressionBase> expressions) : ExpressionBase
{
    private readonly string _collectionOperator = collectionOperator ?? throw new ArgumentNullException(nameof(collectionOperator));
    private readonly ICollection<ExpressionBase> _expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));

    public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WriteStartObject();

        jsonWriter.WritePropertyName(_collectionOperator);

        jsonWriter.WriteStartArray();

        foreach (var expression in _expressions)
        {
            if (expression is null)
            {
                throw new InvalidOperationException("Expression cannot be null.");
            }

            expression.WriteExpressionJson(jsonWriter);
        }

        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();
    }
}
