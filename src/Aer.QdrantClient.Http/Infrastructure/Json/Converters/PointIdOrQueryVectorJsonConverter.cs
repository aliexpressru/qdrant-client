using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class PointIdOrQueryVectorJsonConverter : JsonConverter<PointIdOrQueryVector>
{
    private static readonly JsonSerializerOptions _serializerOptions =
        JsonSerializerConstants.CreateSerializerOptions(
            new PointIdJsonConverter(),
            new QueryVectorJsonConverter(),
            new InferenceObjectJsonConverter()
        );

    public override PointIdOrQueryVector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException($"Reading {nameof(PointIdOrQueryVector)} instances is not supported");

    public override void Write(Utf8JsonWriter writer, PointIdOrQueryVector value, JsonSerializerOptions options)
    {
        if (value.PointId is not null)
        {
            JsonSerializer.Serialize(writer, value.PointId, _serializerOptions);
        }

        if (value.QueryVector is not null)
        {
            JsonSerializer.Serialize(writer, value.QueryVector, _serializerOptions);
        }
    }
}
