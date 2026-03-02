using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class PointIdOrQueryVectorCollectionJsonConverter : JsonConverter<ICollection<PointIdOrQueryVector>>
{
    private static readonly JsonSerializerOptions _serializerOptions =
        JsonSerializerConstants.CreateSerializerOptions(new PointIdOrQueryVectorJsonConverter());

    public override ICollection<PointIdOrQueryVector> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException($"Reading {nameof(IEnumerable<>)} instances is not supported");

    public override void Write(Utf8JsonWriter writer, ICollection<PointIdOrQueryVector> value, JsonSerializerOptions options)
    {
        using (writer.WriteArray())
        {
            foreach (var typedValue in value)
            {
                JsonSerializer.Serialize(writer, typedValue, _serializerOptions);
            }
        }
    }
}
