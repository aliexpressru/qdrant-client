using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class PointIdOrQueryVectorJsonConverter : JsonConverter<PointIdOrQueryVector>
{
    public override PointIdOrQueryVector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Reading {nameof(PointIdOrQueryVector)} instances is not supported");
    }

    public override void Write(Utf8JsonWriter writer, PointIdOrQueryVector value, JsonSerializerOptions options)
    {
        if (value.PointId is not null)
        {
            JsonSerializer.Serialize(writer, value.PointId, JsonSerializerConstants.SerializerOptions);
        }

        if (value.QueryVector is not null)
        {
            JsonSerializer.Serialize(writer, value.QueryVector, JsonSerializerConstants.SerializerOptions);
        }
    }
}
