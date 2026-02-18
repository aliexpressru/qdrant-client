using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class ObjectPayloadEnumerableJsonConverter : JsonConverter<IEnumerable<object>>
{
    private static readonly JsonSerializerOptions _serializerOptions =
        JsonSerializerConstants.CreateSerializerOptions(new ObjectPayloadJsonConverter());

    public override IEnumerable<object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException("Reading enumerable point payload instances is not supported");

    public override void Write(Utf8JsonWriter writer, IEnumerable<object> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var payload in value)
        {
            JsonSerializer.Serialize(writer, payload, _serializerOptions);
        }

        writer.WriteEndArray();
    }
}
