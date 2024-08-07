using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class PointIdCollectionJsonConverter : JsonConverter<IEnumerable<PointId>>
{
    private static readonly JsonSerializerOptions _serializerOptions =
        JsonSerializerConstants.CreateSerializerOptions(new PointIdJsonConverter());

    public override IEnumerable<PointId> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return Array.Empty<PointId>();
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new QdrantJsonParsingException($"Can't deserialize value {reader.GetString()} to {typeof(IEnumerable<PointId>)}");
        }

        JsonNode array = JsonNode.Parse(ref reader);

        List<PointId> collection = [];

        foreach (var arrayJElement in array!.AsArray())
        {
            var pointIdValueString = arrayJElement.GetValue<string>();

            var parsedPointId = ulong.TryParse(pointIdValueString, out ulong pointIdInt)
                ? PointId.Integer(pointIdInt)
                : PointId.Guid(Guid.Parse((ReadOnlySpan<char>) pointIdValueString));

            collection.Add(parsedPointId);
        }

        return collection;
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<PointId> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var pointId in value)
        {
            JsonSerializer.Serialize(writer, pointId, _serializerOptions);
        }

        writer.WriteEndArray();
    }
}
