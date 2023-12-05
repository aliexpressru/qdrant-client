using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal class FieldIsNullCondition : FilterConditionBase
{
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
