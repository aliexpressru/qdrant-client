using Aer.QdrantClient.Http.Filters.Introspection;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;

/// <summary>
/// Represents a plain group of conditions that are located on one level.
/// </summary>
internal sealed class FilterGroupCondition : FilterGroupConditionBase
{
    public FilterGroupCondition(params FilterConditionBase[] conditions) : this((IEnumerable<FilterConditionBase>)conditions)
    {
    }

    public FilterGroupCondition(IEnumerable<FilterConditionBase> conditions) : base(DiscardPayloadFieldName)
    {
        foreach (var condition in conditions)
        {
            if (condition is null)
            {
                continue;
            }

            if (condition is FilterGroupCondition fgc)
            {
                Conditions.AddRange(fgc.Conditions);
            }
            else
            {
                Conditions.Add(condition);
            }
        }
    }

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        foreach (var condition in Conditions)
        {
            condition.WriteJson(jsonWriter);
        }
    }

    internal override void Accept(FilterConditionVisitor visitor)
    {
        visitor.VisitFilterGroupCondition(this);

        foreach (var condition in Conditions)
        {
            condition.Accept(visitor);
        }
    }
}
