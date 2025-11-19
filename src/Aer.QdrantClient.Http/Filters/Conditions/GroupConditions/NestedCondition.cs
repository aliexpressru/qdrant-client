using Aer.QdrantClient.Http.Filters.Introspection;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;

/// <summary>
/// Represents "all nested conditions must satisfy specified filters" filter condition group.
/// </summary>
internal sealed class NestedCondition : FilterGroupConditionBase
{
    public NestedCondition(string payloadFieldName, IEnumerable<FilterConditionBase> conditions) : base(payloadFieldName)
    {
        Conditions.AddRange(conditions);
    }

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WritePropertyName("nested");
        jsonWriter.WriteStartObject();

        WritePayloadFieldName(jsonWriter);

        jsonWriter.WritePropertyName("filter");
        jsonWriter.WriteStartObject();

        foreach (var condition in Conditions)
        {
            condition.WriteJson(jsonWriter);
        }

        jsonWriter.WriteEndObject();

        jsonWriter.WriteEndObject();
    }

    internal override void Accept(FilterConditionVisitor visitor)
    {
        visitor.VisitNestedCondition(this);

        foreach (var condition in Conditions)
        {
            condition.Accept(visitor);
        }
    }
}
