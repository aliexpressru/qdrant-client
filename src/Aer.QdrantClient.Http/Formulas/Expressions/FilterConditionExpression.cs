using Aer.QdrantClient.Http.Filters.Conditions;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a filter condition expression.
/// </summary>
internal sealed class FilterConditionExpression(FilterConditionBase condition) : ExpressionBase
{
    public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
    {
        using (jsonWriter.WriteObject())
        {
            condition.WriteConditionJson(jsonWriter);
        }
    }
}
