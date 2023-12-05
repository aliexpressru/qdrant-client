using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class VectorJsonConverter : JsonConverter<VectorBase>
{
    public override VectorBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartArray:
            {
                var vectorValuesJArray = JsonNode.Parse(ref reader);

                var vectorValuesArray =
                    vectorValuesJArray.Deserialize<float[]>(JsonSerializerConstants.SerializerOptions);

                return new Vector()
                {
                    VectorValues = vectorValuesArray
                };
            }

            case JsonTokenType.StartObject:
            {
                // means named vectors collection:
                // "vector1" : [0.0, 0.1, 0.2], "vector2" " [10, 11, 12]

                var namedVectorsJObject = JsonNode.Parse(ref reader);

                var namedVectorsObject = namedVectorsJObject.Deserialize<Dictionary<string, float[]>>(
                    JsonSerializerConstants.SerializerOptions);

                return new NamedVectors()
                {
                    Vectors = namedVectorsObject
                };
            }

            default:
                throw new QdrantJsonParsingException("Unbable to deserialize Qdrant vector value");
        }
    }

    public override void Write(Utf8JsonWriter writer, VectorBase value, JsonSerializerOptions options)
    {
        if (value is Vector v)
        {
            JsonSerializer.Serialize(writer, v.VectorValues, JsonSerializerConstants.SerializerOptions);

            return;
        }

        if (value is NamedVectors mv)
        {
            JsonSerializer.Serialize(writer, mv.Vectors, JsonSerializerConstants.SerializerOptions);

            return;
        }

        throw new QdrantJsonSerializationException($"Can't serialize {value} vector of type {value.GetType()}");
    }
}
