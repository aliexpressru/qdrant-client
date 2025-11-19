using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldIsNullOrEmptyCondition : FilterConditionBase
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => null;

    public FieldIsNullOrEmptyCondition(string payloadFieldName) : base(payloadFieldName)
    {
    }

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WritePropertyName("is_empty");
        jsonWriter.WriteStartObject();

        WritePayloadFieldName(jsonWriter);

        jsonWriter.WriteEndObject();
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldIsNullOrEmptyCondition(this);
}
