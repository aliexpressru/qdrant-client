using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class QuantizationConfigurationJsonConverter : JsonConverter<QuantizationConfiguration>
{
    public override QuantizationConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new QdrantJsonParsingException("Unable to deserialize Qdrant collection quantization configuration value");
        }

        // move to the quantization method name
        reader.Read();

        var quantizationMethodName = reader.GetString();

        // move to the quantization configuration object
        reader.Read();

        var quantizationConfigurationObject = JsonElement.ParseValue(ref reader);

        QuantizationConfiguration ret = quantizationMethodName switch
        {
            QuantizationConfiguration.ScalarQuantizationConfiguration.QuantizationMethodName =>
                quantizationConfigurationObject.Deserialize<QuantizationConfiguration.ScalarQuantizationConfiguration>(
                    JsonSerializerConstants.DefaultSerializerOptions),

            QuantizationConfiguration.ProductQuantizationConfiguration.QuantizationMethodName =>
                quantizationConfigurationObject.Deserialize<QuantizationConfiguration.ProductQuantizationConfiguration>(
                    JsonSerializerConstants.DefaultSerializerOptions),

            QuantizationConfiguration.BinaryQuantizationConfiguration.QuantizationMethodName =>
                quantizationConfigurationObject.Deserialize<QuantizationConfiguration.BinaryQuantizationConfiguration>(
                    JsonSerializerConstants.DefaultSerializerOptions),

            QuantizationConfiguration.TurboQuantizationConfiguration.QuantizationMethodName =>
                quantizationConfigurationObject.Deserialize<QuantizationConfiguration.TurboQuantizationConfiguration>(
                    JsonSerializerConstants.DefaultSerializerOptions),

            _ => throw new InvalidOperationException($"Unknown quantization method name {quantizationMethodName}"),
        };

        // move out of quantization configuration to not leave partially read object
        reader.Read();

        return ret;
    }

    public override void Write(Utf8JsonWriter writer, QuantizationConfiguration value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            return;
        }

        using (writer.WriteObject())
        {
            switch (value)
            {
                case QuantizationConfiguration.ScalarQuantizationConfiguration config:
                    writer.WritePropertyName(config.Method);

                    JsonSerializer.Serialize(writer, config, JsonSerializerConstants.DefaultSerializerOptions);

                    break;

                case QuantizationConfiguration.ProductQuantizationConfiguration config:
                    writer.WritePropertyName(config.Method);

                    JsonSerializer.Serialize(writer, config, JsonSerializerConstants.DefaultSerializerOptions);

                    break;

                case QuantizationConfiguration.BinaryQuantizationConfiguration config:
                    writer.WritePropertyName(config.Method);

                    JsonSerializer.Serialize(writer, config, JsonSerializerConstants.DefaultSerializerOptions);

                    break;

                case QuantizationConfiguration.TurboQuantizationConfiguration config:
                    writer.WritePropertyName(config.Method);

                    JsonSerializer.Serialize(writer, config, JsonSerializerConstants.DefaultSerializerOptions);

                    break;

                default:
                    throw new QdrantJsonSerializationException(
                        "Unable to serialize Qdrant collection quantization configuration value");
            }
        }
    }
}
