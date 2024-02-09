using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class ShardKeyJsonConverter : JsonConverter<ShardKey>
{
    public override ShardKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new StringShardKey(reader.GetString());
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            return new IntegerShardKey(reader.GetUInt64());
        }

        throw new QdrantJsonSerializationException($"Can't deserialize {reader.GetString()} shard key value");
    }

    public override void Write(Utf8JsonWriter writer, ShardKey value, JsonSerializerOptions options)
    {
        if (value is StringShardKey ssk)
        {
            writer.WriteStringValue(ssk.ShardKeyValue);
            return;
        }

        if (value is IntegerShardKey isk)
        {
            writer.WriteNumberValue(isk.ShardKeyValue);
            return;
        }

        throw new QdrantJsonSerializationException($"Can't serialize shard key value of type {value.GetType()}");
    }
}
