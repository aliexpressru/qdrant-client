using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;

/// <summary>
/// Represents "all nested conditions must match" filter condition group.
/// </summary>
internal class MustCondition : FilterConditionBase
{
    internal readonly List<FilterConditionBase> Conditions = [];

    public MustCondition(FilterConditionBase condition) : base(DiscardPayloadFieldName)
    {
        AddCondition(condition);
    }

    public MustCondition(IEnumerable<FilterConditionBase> conditions) : base(DiscardPayloadFieldName)
    {
        foreach (var condition in conditions)
        {
            AddCondition(condition);
        }
    }

    private void AddCondition(FilterConditionBase condition)
    {
        if (condition is FilterGroupCondition fgc)
        {
            foreach (var groupCondition in fgc.Conditions)
            {
                if (groupCondition is MustCondition gmc)
                {
                    Conditions.AddRange(gmc.Conditions);
                }
                else
                {
                    Conditions.Add(groupCondition);
                }
            }
        }
        else if (condition is MustCondition mc)
        {
            Conditions.AddRange(mc.Conditions);
        }
        else
        {
            Conditions.Add(condition);
        }
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WritePropertyName("must");
        jsonWriter.WriteStartArray();

        foreach (var condition in Conditions)
        {
            jsonWriter.WriteStartObject();

            condition.WriteConditionJson(jsonWriter);

            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndArray();
    }
}
