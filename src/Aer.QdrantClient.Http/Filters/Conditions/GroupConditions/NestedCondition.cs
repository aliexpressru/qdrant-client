using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
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
        using (jsonWriter.WriteObject("nested"))
        {
            WritePayloadFieldName(jsonWriter);

            using (jsonWriter.WriteObject("filter"))
            {
                foreach (var condition in Conditions)
                {
                    condition.WriteJson(jsonWriter);
                }
            }
        }
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
