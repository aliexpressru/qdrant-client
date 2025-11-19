using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldMatchAnyCondition<T> : FilterConditionBase
{
    internal readonly IEnumerable<T> _anyValuesToMatch;

    protected internal override PayloadIndexedFieldType? PayloadFieldType { get; } = GetPayloadFieldType<T>();

    public FieldMatchAnyCondition(string payloadFieldName, IEnumerable<T> matchAnyValuesToMatchValues)
        : base(payloadFieldName)
    {
        _anyValuesToMatch = matchAnyValuesToMatchValues;
    }

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("match");
        jsonWriter.WriteStartObject();

        jsonWriter.WritePropertyName("any");

        JsonSerializer.Serialize(jsonWriter, _anyValuesToMatch, JsonSerializerConstants.DefaultSerializerOptions);

        jsonWriter.WriteEndObject();
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldMatchAnyCondition(this);
}
