using Aer.QdrantClient.Http.Models.Primitives;
using Newtonsoft.Json;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class CollectionMetadataNewtonsoftJsonConverter : JsonConverter<CollectionMetadata>
{
    public override CollectionMetadata ReadJson(
        JsonReader reader,
        Type objectType,
        CollectionMetadata existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    ) => throw new NotSupportedException($"Reading {nameof(CollectionMetadata)} instances via Newtonsoft.Json is not supported");

    public override void WriteJson(JsonWriter writer, CollectionMetadata value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value.RawMetadata ?? CollectionMetadata.Empty.RawMetadata);
    }
}
