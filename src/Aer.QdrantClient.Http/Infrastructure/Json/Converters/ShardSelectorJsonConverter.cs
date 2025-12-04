using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class ShardSelectorJsonConverter : JsonConverter<ShardSelector>
{
    private static readonly JsonSerializerOptions _serializerOptions =
        JsonSerializerConstants.CreateSerializerOptions(new ShardKeyJsonConverter());

    public override ShardSelector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException($"Reading {nameof(ShardSelector)} instances is not supported");

    public override void Write(Utf8JsonWriter writer, ShardSelector value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case ShardSelector.IntegerShardKeyShardSelector iss:
            {
                if (iss.FallbackShardKeyValue is not null)
                {
                    writer.WriteStartObject();
                    {
                        writer.WritePropertyName("target");
                        writer.WriteNumberValue(iss.ShardKeyValue!.Value);
                        writer.WritePropertyName("fallback");
                        JsonSerializer.Serialize(writer, iss.FallbackShardKeyValue, _serializerOptions);
                    }
                    writer.WriteEndObject();
                }
                else
                {
                    // Means no fallback, just write the target
                    if (iss.ShardKeyValue.HasValue)
                    {
                        // means only one shard key
                        writer.WriteNumberValue(iss.ShardKeyValue.Value);
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, iss.ShardKeyValues, JsonSerializerConstants.DefaultSerializerOptions);
                    }
                }

                return;
            }
            case ShardSelector.StringShardKeyShardSelector sss:
            {
                if (sss.FallbackShardKeyValue is not null)
                {
                    writer.WriteStartObject();
                    {
                        writer.WritePropertyName("target");
                        writer.WriteStringValue(sss.ShardKeyValue);
                        writer.WritePropertyName("fallback");
                        JsonSerializer.Serialize(writer, sss.FallbackShardKeyValue, _serializerOptions);
                    }
                    writer.WriteEndObject();
                }
                else
                {
                    // Means no fallback, just write the target
                    if (!string.IsNullOrEmpty(sss.ShardKeyValue))
                    {
                        // means only one shard key
                        writer.WriteStringValue(sss.ShardKeyValue);
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, sss.ShardKeyValues, JsonSerializerConstants.DefaultSerializerOptions);
                    }
                }

                return;
            }
            default:
                throw new QdrantJsonSerializationException($"Can't serialize {value} shard selector of type {value.GetType()}");
        }
    }
}
