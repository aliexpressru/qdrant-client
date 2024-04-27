using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class SparseVectorConfigurationJsonConverter : JsonConverter<SparseVectorConfiguration>
{
    public override SparseVectorConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var configurationObject = JsonNode.Parse(ref reader);

        if (configurationObject is null || configurationObject["index"] is null)
        {
            throw new QdrantJsonParsingException("Can't deserialize sparse vector configuration.");
        }

        try
        {
            var sparseVectorConfigurationObject =
                configurationObject["index"].AsObject();

            var onDiskPayload = sparseVectorConfigurationObject["on_disk"]!.GetValue<bool>();
            var fullScanThresold = sparseVectorConfigurationObject["full_scan_threshold"]?.GetValue<ulong?>();

            var ret = new SparseVectorConfiguration(onDiskPayload, fullScanThresold);

            return ret;
        }
        catch (InvalidOperationException iox)
        {
            throw new QdrantJsonParsingException(
                $"Can't deserialize sprse vector configuration {configurationObject.ToJsonString()}. Exception : {iox}");
        }
        catch (JsonException ex)
        {
            throw new QdrantJsonParsingException(
                $"Can't deserialize sprse vector configuration {configurationObject.ToJsonString()}. Exception : {ex}");
        }
    }

    public override void Write(Utf8JsonWriter writer, SparseVectorConfiguration value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        {
            writer.WritePropertyName("index");
            writer.WriteStartObject();
            {
                writer.WritePropertyName("on_disk");
                writer.WriteBooleanValue(value.OnDisk);
                if (value.FullScanThreshold is not null)
                {
                    writer.WritePropertyName("full_scan_threshold");
                    writer.WriteNumberValue(value.FullScanThreshold.Value);
                }
            }
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
    }
}
