using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class JTokenJsonConverter : JsonConverter<JToken>
{
    public override bool CanConvert(Type typeToConvert) => typeof(JToken).IsAssignableFrom(typeToConvert);

    public override JToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Use JsonDocument to parse the JSON and create a JToken from it
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        return JToken.Parse(document.RootElement.GetRawText());
    }

    public override void Write(Utf8JsonWriter writer, JToken value, JsonSerializerOptions options)
    {
        // Write the raw JSON from the JToken to the writer
        writer.WriteRawValue(value.ToString());
    }
}
