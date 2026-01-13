using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class VectorEnumerableJsonConverter : JsonConverter<IEnumerable<VectorBase>>
{
    private static readonly JsonSerializerOptions _serializerOptions =
        JsonSerializerConstants.CreateSerializerOptions(new VectorJsonConverter());
    
    public override IEnumerable<VectorBase> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return [];
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new QdrantJsonParsingException(
                $"Can't deserialize value {reader.GetString()} as {typeof(IEnumerable<VectorBase>)}");
        }

        JsonNode array = JsonNode.Parse(ref reader);

        List<VectorBase> collection = [];
        
        foreach (var arrayJElement in array!.AsArray())
        {
            var vector = arrayJElement.Deserialize<VectorBase>(_serializerOptions);
            collection.Add(vector);
        }

        return collection;
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<VectorBase> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var vector in value)
        {
            JsonSerializer.Serialize(writer, vector, _serializerOptions);
        }

        writer.WriteEndArray();
    }
}
