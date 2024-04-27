using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class ByteArrayJsonConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // we don't need to read byte sequences since qdrant Uint8 vectors are returned by HTTP API as floating point
        // values with fractional part 0 like 33.0
        throw new NotSupportedException("Reading byte[] instances is not supported");

        // if (reader.TokenType != JsonTokenType.StartArray)
        // {
        //     throw new QdrantJsonParsingException("Can't parse an array of byte values");
        // }
        //
        // // advance reader inside array
        // reader.Read();
        //
        // List<byte> ret = new();
        //
        // while (reader.TokenType != JsonTokenType.EndArray)
        // {
        //     if (reader.TokenType != JsonTokenType.Number)
        //     {
        //         throw new QdrantJsonParsingException("Can't parse value as a byte");
        //     }
        //
        //     var byteValue = reader.GetByte();
        //     ret.Add(byteValue);
        //
        //     reader.Read();
        // }
        //
        // return ret.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var number in value)
        {
            writer.WriteNumberValue(number);
        }

        writer.WriteEndArray();
    }
}
