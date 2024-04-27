using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class PointIdJsonConverter : JsonConverter<PointId>
{
    public override PointId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var parsedGuid = reader.GetGuid();
                return PointId.Guid(parsedGuid);
            case JsonTokenType.Number:
                return PointId.Integer(reader.GetInt64());
            default:
                throw new QdrantJsonValueParsingException(reader.GetString());
        }
    }

    public override void Write(Utf8JsonWriter writer, PointId value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToJson(), JsonSerializerConstants.SerializerOptions);
    }
}
