using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class PointIdIEnumerableJsonConverter : JsonConverter<IEnumerable<PointId>>
{
    private static readonly JsonSerializerOptions _serializerOptions =
        JsonSerializerConstants.CreateSerializerOptions(new PointIdJsonConverter());

    public override IEnumerable<PointId> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return [];
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new QdrantJsonParsingException($"Can't deserialize value {reader.GetString()} as {typeof(IEnumerable<PointId>)}");
        }

        JsonNode array = JsonNode.Parse(ref reader);

        List<PointId> collection = [];

        foreach (var arrayJElement in array!.AsArray())
        {
            var valueKind = arrayJElement.GetValueKind();

            switch (valueKind)
            {
                case JsonValueKind.Number:
                {
                    var ulongValue = arrayJElement.GetValue<ulong>();
                    var pointId = PointId.Integer(ulongValue);

                    collection.Add(pointId);
                    break;
                }
                case JsonValueKind.String:
                {
                    var pointIdValueString = arrayJElement.GetValue<string>();

                    // try parse as Guid then try parse as ulong
                    var parsedPointId = Guid.TryParse(pointIdValueString, out Guid parsedIdGuid)
                        ? PointId.Guid(parsedIdGuid)
#if NETSTANDARD2_0
                        : PointId.Integer(ulong.Parse(pointIdValueString));
#else
                        : PointId.Integer(ulong.Parse((ReadOnlySpan<char>) pointIdValueString));
#endif
                    collection.Add(parsedPointId);
                    break;
                }
                default:
                    throw new QdrantJsonValueParsingException(reader.GetString());
            }
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
