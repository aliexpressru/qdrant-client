using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class QuantizationConfigurationJsonConverter : JsonConverter<QuantizationConfiguration>
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

        var quantizationConfigurationObject = JsonNode.Parse(ref reader);

        QuantizationConfiguration ret;

        switch (quantizationMethodName)
        {
            case QuantizationConfiguration.ScalarQuantizationConfiguration.QuantizationMethodName:
                ret = quantizationConfigurationObject.Deserialize<QuantizationConfiguration.ScalarQuantizationConfiguration>(
                    JsonSerializerConstants.SerializerOptions);
                break;
            case QuantizationConfiguration.ProductQuantizationConfiguration.QuantizationMethodName:
                ret = quantizationConfigurationObject.Deserialize<QuantizationConfiguration.ProductQuantizationConfiguration>(
                    JsonSerializerConstants.SerializerOptions);
                break;
            case QuantizationConfiguration.BinaryQuantizationConfiguration.QuantizationMethodName:
                ret = quantizationConfigurationObject.Deserialize<QuantizationConfiguration.BinaryQuantizationConfiguration>(
                    JsonSerializerConstants.SerializerOptions);
                break;
            default:
                throw new InvalidOperationException($"Unknown quantization method name {quantizationMethodName}");
        }

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

        writer.WriteStartObject();
        {
            switch (value)
            {
                case QuantizationConfiguration.ScalarQuantizationConfiguration sc:
                    writer.WritePropertyName(QuantizationConfiguration.ScalarQuantizationConfiguration.QuantizationMethodName);

                    JsonSerializer.Serialize(writer, sc, JsonSerializerConstants.SerializerOptions);

                    break;
                case QuantizationConfiguration.ProductQuantizationConfiguration pc:
                    writer.WritePropertyName(QuantizationConfiguration.ProductQuantizationConfiguration.QuantizationMethodName);

                    JsonSerializer.Serialize(writer, pc, JsonSerializerConstants.SerializerOptions);

                    break;
                case QuantizationConfiguration.BinaryQuantizationConfiguration bc:
                    writer.WritePropertyName(QuantizationConfiguration.BinaryQuantizationConfiguration.QuantizationMethodName);

                    JsonSerializer.Serialize(writer, bc, JsonSerializerConstants.SerializerOptions);

                    break;
                default:
                    throw new QdrantJsonSerializationException(
                        "Unable to serialize Qdrant collection quantization configuration value");
            }
        }
        writer.WriteEndObject();
    }
}
