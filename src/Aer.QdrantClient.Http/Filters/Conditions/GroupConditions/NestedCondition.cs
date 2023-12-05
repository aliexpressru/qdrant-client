using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;

/// <summary>
/// Represents "all nested conditions must satisfy specified filters" filter condition group.
/// </summary>
internal class NestedCondition : FilterConditionBase
{
    private readonly IEnumerable<FilterConditionBase> _conditions;

    public NestedCondition(string payloadFieldName, IEnumerable<FilterConditionBase> conditions) : base(payloadFieldName)
    {
        _conditions = conditions;
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WritePropertyName("nested");
        jsonWriter.WriteStartObject();

        WritePayloadFieldName(jsonWriter);

        jsonWriter.WritePropertyName("filter");
        jsonWriter.WriteStartObject();

        foreach (var condition in _conditions)
        {
            condition.WriteConditionJson(jsonWriter);
        }

        jsonWriter.WriteEndObject();

        jsonWriter.WriteEndObject();
    }
}
