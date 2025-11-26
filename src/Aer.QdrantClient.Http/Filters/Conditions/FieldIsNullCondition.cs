using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldIsNullCondition(string payloadFieldName) : FilterConditionBase(payloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => null;

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WritePropertyName("is_null");
        jsonWriter.WriteStartObject();

        WritePayloadFieldName(jsonWriter);

        jsonWriter.WriteEndObject();
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldIsNullCondition(this);
}
