using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class QuantizationConfigurationDiffJsonConverter : JsonConverter<QuantizationConfigurationDiff>
{
    public override QuantizationConfigurationDiff Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new QdrantJsonParsingException("Unable to deserialize Qdrant collection quantization configuration diff value");
        }

        // move to the quantization method name
        reader.Read();

        var quantizationMethodName = reader.GetString();

        // move to the quantization configuration object
        reader.Read();

        var quantizationConfigurationObject = JsonElement.ParseValue(ref reader);

        QuantizationConfigurationDiff ret = quantizationMethodName switch
        {
            QuantizationConfigurationDiff.ScalarQuantizationConfigurationDiff.QuantizationMethodName =>
                quantizationConfigurationObject.Deserialize<QuantizationConfigurationDiff.ScalarQuantizationConfigurationDiff>(
                    JsonSerializerConstants.DefaultSerializerOptions),

            QuantizationConfigurationDiff.ProductQuantizationConfigurationDiff.QuantizationMethodName =>
                quantizationConfigurationObject.Deserialize<QuantizationConfigurationDiff.ProductQuantizationConfigurationDiff>(
                    JsonSerializerConstants.DefaultSerializerOptions),

            QuantizationConfigurationDiff.BinaryQuantizationConfigurationDiff.QuantizationMethodName =>
                quantizationConfigurationObject.Deserialize<QuantizationConfigurationDiff.BinaryQuantizationConfigurationDiff>(
                    JsonSerializerConstants.DefaultSerializerOptions),

            QuantizationConfigurationDiff.TurboQuantizationConfigurationDiff.QuantizationMethodName =>
                quantizationConfigurationObject.Deserialize<QuantizationConfigurationDiff.TurboQuantizationConfigurationDiff>(
                    JsonSerializerConstants.DefaultSerializerOptions),

            QuantizationConfigurationDiff.DisabledQuantizationConfigurationDiff.QuantizationMethodName =>
                new QuantizationConfigurationDiff.DisabledQuantizationConfigurationDiff(),

            _ => throw new InvalidOperationException($"Unknown quantization method name {quantizationMethodName}"),
        };

        // move out of quantization configuration to not leave partially read object
        reader.Read();

        return ret;
    }

    public override void Write(Utf8JsonWriter writer, QuantizationConfigurationDiff value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            return;
        }

        using (writer.WriteObject())
        {
            switch (value)
            {
                case QuantizationConfigurationDiff.ScalarQuantizationConfigurationDiff config:
                    writer.WritePropertyName(config.Method);

                    JsonSerializer.Serialize(writer, config, JsonSerializerConstants.DefaultSerializerOptions);

                    break;

                case QuantizationConfigurationDiff.ProductQuantizationConfigurationDiff config:
                    writer.WritePropertyName(config.Method);

                    JsonSerializer.Serialize(writer, config, JsonSerializerConstants.DefaultSerializerOptions);

                    break;

                case QuantizationConfigurationDiff.BinaryQuantizationConfigurationDiff config:
                    writer.WritePropertyName(config.Method);

                    JsonSerializer.Serialize(writer, config, JsonSerializerConstants.DefaultSerializerOptions);

                    break;

                case QuantizationConfigurationDiff.TurboQuantizationConfigurationDiff config:
                    writer.WritePropertyName(config.Method);

                    JsonSerializer.Serialize(writer, config, JsonSerializerConstants.DefaultSerializerOptions);

                    break;

                case QuantizationConfigurationDiff.DisabledQuantizationConfigurationDiff config:
                    writer.WritePropertyName(config.Method);

                    writer.WriteStartObject();
                    writer.WriteEndObject();

                    break;

                default:
                    throw new QdrantJsonSerializationException(
                        "Unable to serialize Qdrant collection quantization configuration diff value");
            }
        }
    }
}
