using Aer.QdrantClient.Http.Filters.Introspection;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;

/// <summary>
/// Represents "At least minimum amount of given conditions should match" filter condition group.
/// </summary>
internal sealed class MinimumShouldCondition : FilterGroupConditionBase
{
    /// <summary>
    /// Minimal number of conditions that should match to render this filter matched.
    /// </summary>
    private readonly int _minCount;

    public MinimumShouldCondition(int minCount, IEnumerable<FilterConditionBase> conditions) : base(DiscardPayloadFieldName)
    {
        if (minCount < 0)
        {
            throw new InvalidOperationException("Minimum matched condition count should be greater than 0");
        }

        _minCount = minCount;

        foreach (var condition in conditions)
        {
            if (condition is FilterGroupCondition fgc)
            {
                foreach (var groupCondition in fgc.Conditions)
                {
                    if (groupCondition is MinimumShouldCondition gsc
                        && gsc._minCount == _minCount)
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
                && sc._minCount == _minCount)
            {
                Conditions.AddRange(sc.Conditions);
            }
            else
            {
                Conditions.Add(condition);
            }
        }
    }

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WritePropertyName("min_should");
        jsonWriter.WriteStartObject();
        {
            jsonWriter.WriteNumber("min_count", _minCount);

            jsonWriter.WritePropertyName("conditions");
            jsonWriter.WriteStartArray();

            foreach (var condition in Conditions)
            {
                jsonWriter.WriteStartObject();
                {
                    condition.WriteJson(jsonWriter);
                }
                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
        }

        jsonWriter.WriteEndObject();
    }

    internal override void Accept(FilterConditionVisitor visitor)
    {
        visitor.VisitMinimumShouldCondition(this);

        foreach (var condition in Conditions)
        {
            condition.Accept(visitor);
        }
    }
}
