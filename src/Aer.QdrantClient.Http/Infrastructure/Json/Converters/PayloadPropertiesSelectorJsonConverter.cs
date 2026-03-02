using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class PayloadPropertiesSelectorJsonConverter : JsonConverter<PayloadPropertiesSelector>
{
    public override PayloadPropertiesSelector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException($"Reading {nameof(PayloadPropertiesSelector)} instances is not supported");

    public override void Write(Utf8JsonWriter writer, PayloadPropertiesSelector value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case PayloadPropertiesSelector.AllPayloadPropertiesSelector aps:
                JsonSerializer.Serialize(writer, aps.AreAllPayloadPropertiesSelected);

                return;
            case PayloadPropertiesSelector.IncludePayloadPropertiesSelector ips:
                using (writer.WriteObject())
                {
                    writer.WritePropertyName("include");

                    JsonSerializer.Serialize(writer, ips.IncludedPayloadProperties, JsonSerializerConstants.DefaultSerializerOptions);
                }

                return;
            case PayloadPropertiesSelector.ExcludePayloadPropertiesSelector eps:
                writer.WriteObject();
                {
                    writer.WritePropertyName("exclude");

                    JsonSerializer.Serialize(writer, eps.ExcludedPayloadProperties, JsonSerializerConstants.DefaultSerializerOptions);
                }

                return;
            default:
                throw new QdrantJsonSerializationException($"Can't serialize {value} payload properties selector of type {value.GetType()}");
        }
    }
}
