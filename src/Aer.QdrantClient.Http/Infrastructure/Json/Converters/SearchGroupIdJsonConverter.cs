using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class SearchGroupIdJsonConverter : JsonConverter<SearchGroupId>
{
    public override SearchGroupId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var stringGroupId = reader.GetString();
                return SearchGroupId.String(stringGroupId);

            case JsonTokenType.Number:
                var intGroupId = reader.GetInt64();
                return SearchGroupId.Integer(intGroupId);

            default:
                throw new QdrantJsonValueParsingException(reader.GetString());
        }
    }

    public override void Write(Utf8JsonWriter writer, SearchGroupId value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.ObjectId, JsonSerializerConstants.DefaultSerializerOptions);
}
