using System.Text.Json;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal class FieldIsNullOrEmptyCondition : FilterConditionBase
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => null;
    
    public FieldIsNullOrEmptyCondition(string payloadFieldName) : base(payloadFieldName)
    { }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WritePropertyName("is_empty");
        jsonWriter.WriteStartObject();

        WritePayloadFieldName(jsonWriter);

        jsonWriter.WriteEndObject();
    }
}
