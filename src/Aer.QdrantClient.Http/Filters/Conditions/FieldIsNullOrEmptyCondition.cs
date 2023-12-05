using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal class FieldIsNullOrEmptyCondition : FilterConditionBase
{
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
