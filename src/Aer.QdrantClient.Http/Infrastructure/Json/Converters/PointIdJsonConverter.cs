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
                var guidId = reader.GetGuid();
                return PointId.Guid(guidId);

            case JsonTokenType.Number:
                var intId = reader.GetUInt64();
                return PointId.Integer(intId);

            default:
                throw new QdrantJsonValueParsingException(reader.GetString());
        }
    }

    public override void Write(Utf8JsonWriter writer, PointId value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ObjectId, JsonSerializerConstants.DefaultSerializerOptions);
    }
}
