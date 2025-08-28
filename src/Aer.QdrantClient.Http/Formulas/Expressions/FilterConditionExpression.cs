using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Conditions;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a filter condition expression.
/// </summary>
internal sealed class FilterConditionExpression : ExpressionBase
{
	private readonly FilterConditionBase _condition;

	public FilterConditionExpression(FilterConditionBase condition)
	{
		_condition = condition;
	}

	public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
	{
		jsonWriter.WriteStartObject();
		
		_condition.WriteConditionJson(jsonWriter);
		
		jsonWriter.WriteEndObject();
	}
}
