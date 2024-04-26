using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;

/// <summary>
/// Represents a plain group of conditions that are located on one level.
/// </summary>
internal class FilterGroupCondition : FilterConditionBase
{
    internal readonly List<FilterConditionBase> Conditions = [];

    public FilterGroupCondition(params FilterConditionBase[] conditions) : this((IEnumerable<FilterConditionBase>)conditions)
    { }

    public FilterGroupCondition(IEnumerable<FilterConditionBase> conditions) : base(DiscardPayloadFieldName)
    {
        foreach (var condition in conditions)
        {
            if (condition is null)
            {
                continue;
            }

            QdrantFilter.CheckTopLevelConditionIsGroup(condition);

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

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        foreach (var condition in Conditions)
        {
            condition.WriteConditionJson(jsonWriter);
        }
    }
}
