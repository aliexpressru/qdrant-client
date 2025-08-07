using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Optimization.Abstractions;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal class FieldMatchAnyCondition<T> : FilterConditionBase, IOptimizableCondition
{
    private readonly IEnumerable<T> _anyValuesToMatch;

    protected internal override PayloadIndexedFieldType? PayloadFieldType { get; } = GetPayloadFieldType<T>();

    public FieldMatchAnyCondition(string payloadFieldName, IEnumerable<T> matchAnyValuesToMatchValues)
        : base(payloadFieldName)
    {
        _anyValuesToMatch = matchAnyValuesToMatchValues;
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("match");
        jsonWriter.WriteStartObject();

        jsonWriter.WritePropertyName("any");

        JsonSerializer.Serialize(jsonWriter, _anyValuesToMatch, JsonSerializerConstants.DefaultSerializerOptions);

        jsonWriter.WriteEndObject();
    }

    public void Accept(IOptimizationVisitor visitor)
    {
        visitor.Visit(this);
    }
}
