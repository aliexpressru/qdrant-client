using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class ShardSelectorJsonConverter : JsonConverter<ShardSelector>
{
    public override ShardSelector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException($"Reading {nameof(ShardSelector)} instances is not supported");

    public override void Write(Utf8JsonWriter writer, ShardSelector value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case ShardSelector.IntegerShardKeyShardSelector iss:
            {
                if (iss.ShardKeyValues is { Length: > 1 })
                {
                    JsonSerializer.Serialize(writer, iss.ShardKeyValues, JsonSerializerConstants.DefaultSerializerOptions);
                }
                else
                {
                    // means only one shard key
                    writer.WriteNumberValue(iss.ShardKeyValues[0]);
                }

                return;
            }
            case ShardSelector.StringShardKeyShardSelector sss:
            {
                if (sss.ShardKeyValues is { Length: > 1 })
                {
                    JsonSerializer.Serialize(writer, sss.ShardKeyValues, JsonSerializerConstants.DefaultSerializerOptions);
                }
                else
                {
                    // means only one shard key
                    writer.WriteStringValue(sss.ShardKeyValues[0]);
                }

                return;
            }
            default:
                throw new QdrantJsonSerializationException($"Can't serialize {value} shard selector of type {value.GetType()}");
        }
    }
}
