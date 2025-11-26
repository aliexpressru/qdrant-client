using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Conditions;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a filter condition expression.
/// </summary>
internal sealed class FilterConditionExpression(FilterConditionBase condition) : ExpressionBase
{
    public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WriteStartObject();

        condition.WriteConditionJson(jsonWriter);

        jsonWriter.WriteEndObject();
    }
}
