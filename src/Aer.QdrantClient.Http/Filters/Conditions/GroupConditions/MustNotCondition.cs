using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;

/// <summary>
/// Represents "all nested conditions must not match" filter condition group.
/// </summary>
internal class MustNotCondition : FilterConditionBase
{
    internal readonly List<FilterConditionBase> Conditions = [];

    public MustNotCondition(FilterConditionBase singleCondition) : base(DiscardPayloadFieldName)
    {
        if (singleCondition is FilterGroupCondition fgc)
        {
            foreach (var groupCondition in fgc.Conditions)
            {
                if (groupCondition is MustNotCondition gmnc)
                {
                    Conditions.AddRange(gmnc.Conditions);
                }
                else
                {
                    Conditions.Add(groupCondition);
                }
            }
        }
        else if (singleCondition is MustNotCondition mnc)
        {
            Conditions.AddRange(mnc.Conditions);
        }
        else
        {
            Conditions.Add(singleCondition);
        }
    }

    public MustNotCondition(IEnumerable<FilterConditionBase> conditions) : base(DiscardPayloadFieldName)
    {
        foreach (var condition in conditions)
        {
            if (condition is FilterGroupCondition fgc)
            {
                foreach (var groupCondition in fgc.Conditions)
                {
                    if (groupCondition is MustNotCondition gmnc)
                    {
                        Conditions.AddRange(gmnc.Conditions);
                    }
                    else
                    {
                        Conditions.Add(groupCondition);
                    }
                }
            }
            else if (condition is MustNotCondition mnc)
            {
                Conditions.AddRange(mnc.Conditions);
            }
            else
            {
                Conditions.Add(condition);
            }
        }
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WritePropertyName("must_not");
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
