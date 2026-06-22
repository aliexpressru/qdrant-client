using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class VectorConfigurationDiffJsonConverter : JsonConverter<VectorConfigurationDiff>
{
    public override VectorConfigurationDiff Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (
            !JsonElement.TryParseValue(ref reader, out var configurationObject)
            || !configurationObject.HasValue
            || configurationObject.Value.GetPropertyCount() == 0
        )
        {
            return null;
        }

        try
        {
            // try read as multiple vectors configuration diff
            // since this method is never expected to be on the hot path - we use try-catch here to
            // determine which vector configuration diff we are reading

            var namedVectorsConfigurations = configurationObject.Value.Deserialize<
                Dictionary<string, VectorConfigurationDiff.SingleVectorConfigurationDiff>
            >(JsonSerializerConstants.DefaultSerializerOptions);

            return new VectorConfigurationDiff.NamedVectorsConfigurationDiff(namedVectorsConfigurations);
        }
        catch (JsonException)
        {
            // means this configuration is a single vector configuration diff

            try
            {
                var singleVectorConfiguration =
                    configurationObject.Value.Deserialize<VectorConfigurationDiff.SingleVectorConfigurationDiff>(
                        JsonSerializerConstants.DefaultSerializerOptions
                    );

                return singleVectorConfiguration;
            }
            catch (JsonException ex)
            {
                throw new QdrantJsonParsingException($"Can't deserialize vector configuration diff. Exception : {ex}");
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, VectorConfigurationDiff value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case VectorConfigurationDiff.SingleVectorConfigurationDiff sv:
                JsonSerializer.Serialize(writer, sv, JsonSerializerConstants.DefaultSerializerOptions);
                return;
            case VectorConfigurationDiff.NamedVectorsConfigurationDiff nv:
                JsonSerializer.Serialize(writer, nv.NamedVectors, JsonSerializerConstants.DefaultSerializerOptions);
                return;
            default:
                throw new QdrantJsonSerializationException(
                    $"Can't serialize {value} vector configuration diff of type {value.GetType()}"
                );
        }
    }
}
