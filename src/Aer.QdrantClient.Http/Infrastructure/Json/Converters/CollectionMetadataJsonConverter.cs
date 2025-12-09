using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class CollectionMetadataJsonConverter : JsonConverter<CollectionMetadata>
{
    public override CollectionMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            var metadataDictionary =
                JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ref reader, JsonSerializerConstants.DefaultSerializerOptions);

            return new CollectionMetadata(metadataDictionary);
        }
        catch (JsonException)
        {
            throw new QdrantJsonParsingException($"Failed to deserialize {nameof(CollectionMetadata)}");
        }
    }

    public override void Write(Utf8JsonWriter writer, CollectionMetadata value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(
            writer,
            value.RawMetadata ?? CollectionMetadata.Empty.RawMetadata,
            JsonSerializerConstants.DefaultSerializerOptions
        );
}
