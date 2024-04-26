using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;

/// <summary>
/// Represents "At least minimum amount of given conditions should match" filter condition group.
/// </summary>
internal class MinimumShouldCondition : FilterConditionBase
{
    internal readonly List<FilterConditionBase> Conditions = [];

    /// <summary>
    /// Minimal number of conditions that should match to render ths filter matched.
    /// </summary>
    internal readonly int MinCount;

    public MinimumShouldCondition(int minCount, IEnumerable<FilterConditionBase> conditions) : base(DiscardPayloadFieldName)
    {
        if (minCount < 0)
        {
            throw new InvalidOperationException("Minimum matched condition count should be greater than 0");
        }

        MinCount = minCount;

        foreach (var condition in conditions)
        {
            if (condition is FilterGroupCondition fgc)
            {
                foreach (var groupCondition in fgc.Conditions)
                {
                    if (groupCondition is MinimumShouldCondition gsc
                        && gsc.MinCount == MinCount)
                    {
                        // unfold nested groups only if the MinCount is the same
                        Conditions.AddRange(gsc.Conditions);
                    }
                    else
                    {
                        Conditions.Add(groupCondition);
                    }
                }
            }

            if (condition is MinimumShouldCondition sc
                && sc.MinCount == MinCount)
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
        jsonWriter.WritePropertyName("min_should");
        jsonWriter.WriteStartObject();
        {
            jsonWriter.WriteNumber("min_count", MinCount);

            jsonWriter.WritePropertyName("conditions");
            jsonWriter.WriteStartArray();

            foreach (var condition in Conditions)
            {
                jsonWriter.WriteStartObject();
                {
                    condition.WriteConditionJson(jsonWriter);
                }
                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
        }

        jsonWriter.WriteEndObject();
    }
}
