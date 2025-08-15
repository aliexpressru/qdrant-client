using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class FullTextIndexStemmingAlgorithmJsonConverter : JsonConverter<FullTextIndexStemmingAlgorithm>
{
    public override FullTextIndexStemmingAlgorithm Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new QdrantJsonParsingException(
                "Unable to deserialize Qdrant full-text stemming algorithm configuration value");
        }

        var stemmerConfigurationObject = JsonElement.ParseValue(ref reader);

        if (!stemmerConfigurationObject.TryGetProperty("type", out var stemmerTypeProperty)
            || stemmerTypeProperty.ValueKind != JsonValueKind.String)
        {
            throw new QdrantJsonParsingException(
                "No Qdrant full-text stemmer type property found in the configuration object or it has an invalid value");
        }

        var stemmingAlgorithmTypeString = stemmerTypeProperty.GetString();

        if (!Enum.TryParse<StemmingAlgorithmType>(
                stemmingAlgorithmTypeString,
                ignoreCase: true,
                out var stemmingAlgorithmType))
        {
            throw new QdrantJsonParsingException(
                $"Unknown Qdrant full-text stemmer algorithm type {stemmingAlgorithmTypeString}");
        }

        return stemmingAlgorithmType switch
        {
            StemmingAlgorithmType.Snowball => stemmerConfigurationObject
                    .Deserialize<FullTextIndexStemmingAlgorithm.SnowballStemmingAlgorithm>(
                        JsonSerializerConstants.DefaultSerializerOptions)
                ?? throw new QdrantJsonParsingException(
                    "Failed to deserialize Snowball stemming algorithm configuration"),

            _ => throw new QdrantJsonParsingException(
                $"Unsupported Qdrant full-text stemmer algorithm type {stemmingAlgorithmType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, FullTextIndexStemmingAlgorithm value, JsonSerializerOptions options)
    {
        if (value is FullTextIndexStemmingAlgorithm.SnowballStemmingAlgorithm snowballStemmingAlgorithm)
        {
            JsonSerializer.Serialize(writer, snowballStemmingAlgorithm, JsonSerializerConstants.DefaultSerializerOptions);
        }
        else
        {
            throw new QdrantJsonSerializationException(
                $"Unsupported Qdrant full-text stemmer algorithm type {value.GetType()}");
        }
    }
}
