using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldMatchAnyConditionFast<T> : FilterConditionBase
{
    private readonly ShouldCondition _optimizedShouldCondition;

    protected internal override PayloadIndexedFieldType? PayloadFieldType { get; } = GetPayloadFieldType<T>();

    public FieldMatchAnyConditionFast(string payloadFieldName, IEnumerable<T> matchAnyValues)
        : base(payloadFieldName)
    {
        List<FilterConditionBase> splitMatchConditions = [];

        foreach (var value in matchAnyValues)
        {
            splitMatchConditions.Add(new FieldMatchCondition<T>(payloadFieldName, value));
        }

        _optimizedShouldCondition = new ShouldCondition(conditions: splitMatchConditions.ToArray());
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        _optimizedShouldCondition.WriteConditionJson(jsonWriter);
    }
}
