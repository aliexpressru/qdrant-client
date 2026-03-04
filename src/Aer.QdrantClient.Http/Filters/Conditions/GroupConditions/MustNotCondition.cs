using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;

/// <summary>
/// Represents "all nested conditions must not match" filter condition group.
/// </summary>
internal sealed class MustNotCondition : FilterGroupConditionBase
{
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

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        using (jsonWriter.WriteArray("must_not"))
        {
            foreach (var condition in Conditions)
            {
                using (jsonWriter.WriteObject())
                {
                    condition.WriteJson(jsonWriter);
                }
            }
        }
    }

    internal override void Accept(FilterConditionVisitor visitor)
    {
        visitor.VisitMustNotCondition(this);

        foreach (var condition in Conditions)
        {
            condition.Accept(visitor);
        }
    }
}
