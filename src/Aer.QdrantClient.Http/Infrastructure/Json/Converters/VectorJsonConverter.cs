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

                // name - vector name
                // value can be ither a simple vector
                // "vector1" : [0.0, 0.1, 0.2], "vector2" " [10, 11, 12]

                // or it can be asparse vector
                // "vector1" : {"indices": [6, 7], "values": [1.0, 2.0]}

                var namedVectorsJObject = JsonNode.Parse(ref reader);

                var namedVectorsObject = namedVectorsJObject.Deserialize<Dictionary<string, JsonNode>>(
                    JsonSerializerConstants.SerializerOptions);

                var vectors = new Dictionary<string, VectorBase>();

                foreach (var (vectorName, vector) in namedVectorsObject)
                {
                    if (vector is JsonArray singleVectorValues)
                    {
                        vectors.Add(
                            vectorName,
                            new Vector()
                            {
                                VectorValues = singleVectorValues.GetValues<float>().ToArray()
                            });
                    }
                    else if(vector is JsonObject sparseVectorValues)
                    {
                        var sparseVector =
                            sparseVectorValues.Deserialize<SparseVector>(JsonSerializerConstants.SerializerOptions);

                        vectors.Add(vectorName, sparseVector);
                    }
                    else
                    {
                        throw new QdrantJsonParsingException(
                            $"Unbable to deserialize Qdrant vector value. Unexpected vector representation : {vector.GetType()}");
                    }
                }

                return new NamedVectors() {Vectors = vectors};
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

        if (value is NamedVectors nv)
        {
            writer.WriteStartObject();
            {
                // named vector contains either Vector or SparseVector as value

                foreach (var (vectorName, vector) in nv.Vectors)
                {
                    writer.WritePropertyName(vectorName);

                    if (vector.IsSparseVector)
                    {
                        var sparseVector = vector.AsSparseVector();
                        JsonSerializer.Serialize(writer, sparseVector, JsonSerializerConstants.SerializerOptions);
                    }
                    else
                    {
                        // means this vector is a non-sparse one
                        var sngleVector = vector.AsSingleVector();
                        JsonSerializer.Serialize(writer, sngleVector.VectorValues, JsonSerializerConstants.SerializerOptions);
                    }
                }
            }
            writer.WriteEndObject();

            return;
        }

        throw new QdrantJsonSerializationException($"Can't serialize {value} vector of type {value.GetType()}");
    }
}
