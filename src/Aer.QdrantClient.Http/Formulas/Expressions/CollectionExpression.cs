using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a collection expression in Qdrant formulas.
/// </summary>
internal class CollectionExpression: ExpressionBase
{
	private readonly string _collectionOperator;
	private readonly ICollection<ExpressionBase> _expressions;

	public CollectionExpression(string collectionOperator,params ICollection<ExpressionBase> expressions)
	{
		_collectionOperator = collectionOperator ?? throw new ArgumentNullException(nameof(collectionOperator));
		_expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
	}
	
	public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
	{
		jsonWriter.WriteStartObject();

		jsonWriter.WritePropertyName(_collectionOperator);

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
