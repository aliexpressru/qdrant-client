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
    ) => throw new NotSupportedException($"Reading {nameof(CollectionMetadata)} instances via Newtonsoft.Json serializer is not supported");

    public override void WriteJson(JsonWriter writer, CollectionMetadata value, JsonSerializer serializer)
    {
        var serializedMetadata = System.Text.Json.JsonSerializer.Serialize(value.RawMetadata ?? CollectionMetadata.Empty.RawMetadata, JsonSerializerConstants.DefaultSerializerOptions);

        writer.WriteRawValue(serializedMetadata);
    }
}
