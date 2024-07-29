using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class PointIdOrQueryVectorCollectionJsonConverter : JsonConverter<ICollection<PointIdOrQueryVector>>
{
    private static readonly JsonSerializerOptions _serializerOptions =
        JsonSerializerConstants.CreateSerializerOptions(new PointIdOrQueryVectorJsonConverter());

    public override ICollection<PointIdOrQueryVector> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Reading {nameof(IEnumerable<PointIdOrQueryVector>)} instances is not supported");
    }

    public override void Write(Utf8JsonWriter writer, ICollection<PointIdOrQueryVector> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var typedValue in value)
        {
            JsonSerializer.Serialize(writer, typedValue, _serializerOptions);
        }

        writer.WriteEndArray();
    }
}
