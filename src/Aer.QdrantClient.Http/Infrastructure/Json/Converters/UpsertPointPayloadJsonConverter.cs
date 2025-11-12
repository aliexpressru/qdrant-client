using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class ObjectPayloadJsonConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Reading upsert point payload object instances is not supported");
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        // This serializer exists to support raw JSON strings as upsert point payload values.
        // If the value is a string, we write it as raw JSON.
        // For all other types, we use the default serializer.
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                return;
            
            case string str:
                writer.WriteRawValue(str);
                return;

            default:
                JsonSerializer.Serialize(writer, value, JsonSerializerConstants.DefaultIndentedSerializerOptions);
                return;
        }
    }
}
