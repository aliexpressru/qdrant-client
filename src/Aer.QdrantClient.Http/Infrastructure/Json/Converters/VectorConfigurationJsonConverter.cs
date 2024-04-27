using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class VectorConfigurationJsonConverter : JsonConverter<VectorConfigurationBase>
{
    public override VectorConfigurationBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var configurationObject = JsonNode.Parse(ref reader);

        try
        {
            // try read as multiple vectors configuration
            // since this method is never expected to be on the hot path - we use try-catch here to
            // determine which vector configuration we are reading

            var namedVectorsConfigurations = configurationObject.Deserialize<
                Dictionary<string, VectorConfigurationBase.SingleVectorConfiguration>>(JsonSerializerConstants.SerializerOptions);

            return new VectorConfigurationBase.NamedVectorsConfiguration(namedVectorsConfigurations);
        }
        catch (JsonException)
        {
            // means this configuration is a single vector configuration

            try
            {
                var singleVectorConfiguration =
                    configurationObject.Deserialize<VectorConfigurationBase.SingleVectorConfiguration>(
                        JsonSerializerConstants.SerializerOptions);

                return singleVectorConfiguration;
            }
            catch (JsonException ex)
            {
                throw new QdrantJsonParsingException($"Can't deserialize vector configuration. Exception : {ex}");
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, VectorConfigurationBase value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case VectorConfigurationBase.SingleVectorConfiguration sv:
                JsonSerializer.Serialize(writer, sv, JsonSerializerConstants.SerializerOptions);
                return;
            case VectorConfigurationBase.NamedVectorsConfiguration nv:
                JsonSerializer.Serialize(writer, nv.NamedVectors, JsonSerializerConstants.SerializerOptions);
                return;
            default:
                throw new QdrantJsonSerializationException($"Can't serialize {value} vector configuration of type {value.GetType()}");
        }
    }
}
