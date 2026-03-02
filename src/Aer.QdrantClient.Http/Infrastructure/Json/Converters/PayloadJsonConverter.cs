using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Primitives;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class PayloadJsonConverter : JsonConverter<Payload>
{
    public override Payload Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);

        var readString = jsonDoc.RootElement.GetRawText();

        return readString.Equals(Payload.EmptyString, StringComparison.OrdinalIgnoreCase)
            ? Payload.Empty
            : new Payload(readString);
    }

    public override void Write(Utf8JsonWriter writer, Payload value, JsonSerializerOptions options)
    {
        if (value is null
            || value.IsEmpty)
        {
            // Write empty object if payload is null or empty
            writer.WriteEmptyObject();

            return;
        }

        value.RawPayload.WriteTo(writer, options);
    }
}
