using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class SparseVectorConfigurationJsonConverter : JsonConverter<SparseVectorConfiguration>
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
            var sparseVectorIndexConfigurationObject =
                configurationObject["index"].AsObject();

            var onDiskPayload = sparseVectorIndexConfigurationObject["on_disk"]!.GetValue<bool>();
            var fullScanThreshold = sparseVectorIndexConfigurationObject["full_scan_threshold"]?.GetValue<ulong?>();

            var vectorDataType = ParseEnumOrDefault(
                sparseVectorIndexConfigurationObject["datatype"],
                VectorDataType.Float32,
                "sparse vector data type");

            var sparseVectorModifier = ParseEnumOrDefault(
                configurationObject["modifier"],
                SparseVectorModifier.None,
                "sparse vector modifier");

            var ret = new SparseVectorConfiguration(
                onDiskPayload,
                fullScanThreshold,
                vectorDataType,
                sparseVectorModifier);

            return ret;
        }
        catch (InvalidOperationException iox)
        {
            throw new QdrantJsonParsingException(
                $"Can't deserialize sparse vector configuration {configurationObject.ToJsonString()}. Exception : {iox}");
        }
        catch (JsonException ex)
        {
            throw new QdrantJsonParsingException(
                $"Can't deserialize sparse vector configuration {configurationObject.ToJsonString()}. Exception : {ex}");
        }
    }

    private TEnum ParseEnumOrDefault<TEnum>(
        JsonNode objectToParse,
        TEnum defaultValue,
        string enumName)
        where TEnum : struct
    {
        var enumValueString = objectToParse?.GetValue<string>();

        var ret = string.IsNullOrEmpty(enumValueString)
            ? defaultValue
            : Enum.TryParse<TEnum>(enumValueString, ignoreCase: true, out var parsedEnumValue)
                ? parsedEnumValue
                : throw new QdrantJsonParsingException(
                    $"Can't deserialize {enumName} {enumValueString}");

        return ret;
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

                writer.WritePropertyName("datatype");
                JsonSerializer.Serialize(writer, value.VectorDataType, JsonSerializerConstants.DefaultSerializerOptions);
            }
            writer.WriteEndObject();

            writer.WritePropertyName("modifier");
            JsonSerializer.Serialize(writer, value.Modifier, JsonSerializerConstants.DefaultSerializerOptions);
        }
        writer.WriteEndObject();
    }
}
