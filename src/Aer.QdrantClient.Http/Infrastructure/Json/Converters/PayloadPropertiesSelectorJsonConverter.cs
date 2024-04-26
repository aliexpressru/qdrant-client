using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class PayloadPropertiesSelectorJsonConverter : JsonConverter<PayloadPropertiesSelector>
{
    public override PayloadPropertiesSelector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Reading {nameof(PayloadPropertiesSelector)} instances is not supported");
    }

    public override void Write(Utf8JsonWriter writer, PayloadPropertiesSelector value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case PayloadPropertiesSelector.AllPayloadPropertiesSelector aps:
                JsonSerializer.Serialize(writer, aps.AreAllPayloadPropertiesSelected);

                return;
            case PayloadPropertiesSelector.IncludePayloadPropertiesSelector ips:
                writer.WriteStartObject();

                writer.WritePropertyName("include");

                JsonSerializer.Serialize(writer, ips.IncludedPayloadProperties, JsonSerializerConstants.SerializerOptions);

                writer.WriteEndObject();

                return;
            case PayloadPropertiesSelector.ExcludePayloadPropertiesSelector eps:
                writer.WriteStartObject();

                writer.WritePropertyName("exclude");

                JsonSerializer.Serialize(writer, eps.ExcludedPayloadProperties, JsonSerializerConstants.SerializerOptions);

                writer.WriteEndObject();

                return;
            default:
                throw new QdrantJsonSerializationException($"Can't serialize {value} payload properties selector of type {value.GetType()}");
        }
    }
}
