using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class ShardKeyJsonConverter : JsonConverter<ShardKey>
{
    public override ShardKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => new StringShardKey(reader.GetString()),
            JsonTokenType.Number => new IntegerShardKey(reader.GetUInt64()),
            _ => throw new QdrantJsonSerializationException($"Can't deserialize {reader.GetString()} shard key value")
        };

    public override void Write(Utf8JsonWriter writer, ShardKey value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case StringShardKey ssk:
                writer.WriteStringValue(ssk.ShardKeyValue);
                return;
            case IntegerShardKey isk:
                writer.WriteNumberValue(isk.ShardKeyValue);
                return;
            default:
                throw new QdrantJsonSerializationException($"Can't serialize shard key value of type {value.GetType()}");
        }
    }
}
