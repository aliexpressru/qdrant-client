using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class PayloadJsonConverter : JsonConverter<Payload>
{
    public override Payload Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var payloadObject = JsonNode.Parse(ref reader)!.AsObject();

        if (payloadObject.Count == 0)
        {
            return null;
        }

        return new Payload()
        {
            RawPayload = payloadObject
        };
    }

    public override void Write(Utf8JsonWriter writer, Payload value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            return;
        }

        // just serialize as object
        JsonSerializer.Serialize(writer, value, JsonSerializerConstants.DefaultSerializerOptions);
    }
}
