using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class PointsDiscoveryContextCollectionJsonConverter : JsonConverter<ICollection<PointsDiscoveryContext>>
{
    private static readonly JsonSerializerOptions _serializerOptions =
        JsonSerializerConstants.CreateSerializerOptions(new PointIdOrQueryVectorJsonConverter());

    public override ICollection<PointsDiscoveryContext> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Reading {nameof(PointsDiscoveryContext)} instances is not supported");
    }

    public override void Write(
        Utf8JsonWriter writer,
        ICollection<PointsDiscoveryContext> value,
        JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var typedValue in value)
        {
            JsonSerializer.Serialize(writer, typedValue, _serializerOptions);
        }

        writer.WriteEndArray();
    }
}
