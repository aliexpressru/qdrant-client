using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class ShardSelectorJsonConverter : JsonConverter<ShardSelector>
{
    public override ShardSelector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Reading {nameof(ShardSelector)} instances is not supported");
    }

    public override void Write(Utf8JsonWriter writer, ShardSelector value, JsonSerializerOptions options)
    {
        if (value is ShardSelector.IntegerShardKeyShardSelector iss)
        {
            if (iss.ShardKeyValues is {Length: > 1})
            {
                JsonSerializer.Serialize(writer, iss.ShardKeyValues, JsonSerializerConstants.SerializerOptions);
            }
            else
            {
                // means only one shard key
                writer.WriteNumberValue(iss.ShardKeyValues[0]);
            }

            return;
        }

        if (value is ShardSelector.StringShardKeyShardSelector sss)
        {
            if (sss.ShardKeyValues is {Length: > 1})
            {
                JsonSerializer.Serialize(writer, sss.ShardKeyValues, JsonSerializerConstants.SerializerOptions);
            }
            else
            {
                // means only one shard key
                writer.WriteStringValue(sss.ShardKeyValues[0]);
            }

            return;
        }

        throw new QdrantJsonSerializationException($"Can't serialize {value} shard selector of type {value.GetType()}");
    }
}
