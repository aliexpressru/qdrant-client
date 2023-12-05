using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;

/// <summary>
/// Represents "at least one nested condition should" filter condition group.
/// </summary>
internal class ShouldCondition : FilterConditionBase
{
    internal readonly List<FilterConditionBase> Conditions = new();

    public ShouldCondition(IEnumerable<FilterConditionBase> conditions) : base(DiscardPayloadFieldName)
    {
        foreach (var condition in conditions)
        {
            if (condition is FilterGroupCondition fgc)
            {
                foreach (var groupCondition in fgc.Conditions)
                {
                    if (groupCondition is ShouldCondition gsc)
                    {
                        Conditions.AddRange(gsc.Conditions);
                    }
                    else
                    {
                        Conditions.Add(groupCondition);
                    }
                }
            }
            if (condition is ShouldCondition sc)
            {
                Conditions.AddRange(sc.Conditions);
            }
            else
            {
                Conditions.Add(condition);
            }
        }
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WritePropertyName("should");
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
