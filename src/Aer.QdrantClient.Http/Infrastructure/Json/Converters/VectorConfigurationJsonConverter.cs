﻿using System.Text.Json;
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
            // try read as multiple vectors configuraiton

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
        if (value is VectorConfigurationBase.SingleVectorConfiguration sv)
        {
            JsonSerializer.Serialize(writer, sv, JsonSerializerConstants.SerializerOptions);
            return;
        }

        if (value is VectorConfigurationBase.NamedVectorsConfiguration nv)
        {
            JsonSerializer.Serialize(writer, nv.NamedVectors, JsonSerializerConstants.SerializerOptions);
            return;
        }

        throw new QdrantJsonSerializationException($"Can't serialize {value} vector configuration of type {value.GetType()}");
    }
}
