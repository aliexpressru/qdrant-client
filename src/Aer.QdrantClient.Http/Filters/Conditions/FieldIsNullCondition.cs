using System.Text.Json;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal class FieldIsNullCondition : FilterConditionBase
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => null;
    
    public FieldIsNullCondition(string payloadFieldName) : base(payloadFieldName)
    { }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WritePropertyName("is_null");
        jsonWriter.WriteStartObject();

        WritePayloadFieldName(jsonWriter);

        jsonWriter.WriteEndObject();
    }
}
