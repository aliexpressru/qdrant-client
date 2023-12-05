using System.Text.Json;
using Aer.QdrantClient.Http.Infrastructure.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal class FieldMatchAnyCondition<T> : FilterConditionBase
{
    private readonly IEnumerable<T> _any;

    public FieldMatchAnyCondition(string payloadFieldName, IEnumerable<T> matchAnyValues)
        : base(payloadFieldName)
    {
        _any = matchAnyValues;
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("match");
        jsonWriter.WriteStartObject();

        jsonWriter.WritePropertyName("any");

        JsonSerializer.Serialize(jsonWriter, _any, JsonSerializerConstants.SerializerOptions);

        jsonWriter.WriteEndObject();
    }
}
