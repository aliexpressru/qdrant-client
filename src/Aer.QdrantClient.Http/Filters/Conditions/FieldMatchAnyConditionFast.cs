using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldMatchAnyConditionFast<T> : FilterConditionBase
{
    internal readonly ShouldCondition _optimizedShouldCondition;

    protected internal override PayloadIndexedFieldType? PayloadFieldType { get; } = GetPayloadFieldType<T>();

    public FieldMatchAnyConditionFast(string payloadFieldName, IEnumerable<T> matchAnyValues)
        : base(payloadFieldName)
    {
        List<FilterConditionBase> splitMatchConditions = [];

        foreach (var value in matchAnyValues)
        {
            splitMatchConditions.Add(new FieldMatchCondition<T>(payloadFieldName, value));
        }

        _optimizedShouldCondition = new ShouldCondition(
            conditions:
                splitMatchConditions.Count == 1
                    ? [splitMatchConditions[0]]
                    : splitMatchConditions.ToArray()
        );
    }

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter) => _optimizedShouldCondition.WriteConditionJson(jsonWriter);

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldMatchAnyConditionFast(this);
}
