using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Shared;

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

                return new FloatVector()
                {
                    Values = vectorValuesArray
                };
            }

            case JsonTokenType.StartObject:
            {
                // means named vectors collection:

                // name - vector name
                // value can be either a simple vector
                // "vector1" : [0.0, 0.1, 0.2], "vector2" " [10, 11, 12]

                // or it can be a sparse vector
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
                            new FloatVector()
                            {
                                Values = singleVectorValues.GetValues<float>().ToArray()
                            });
                    }
                    else if(vector is JsonObject sparseVectorValues)
                    {
                        var sparseVector =
                            sparseVectorValues.Deserialize<SparseFloatVector>(JsonSerializerConstants.SerializerOptions);

                        vectors.Add(vectorName, sparseVector);
                    }
                    else
                    {
                        throw new QdrantJsonParsingException(
                            $"Unable to deserialize Qdrant vector value. Unexpected vector representation : {vector.GetType()}");
                    }
                }

                return new NamedVectors() {Vectors = vectors};
            }

            default:
                throw new QdrantJsonParsingException("Unable to deserialize Qdrant vector value");
        }
    }

    public override void Write(Utf8JsonWriter writer, VectorBase value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case FloatVector fv:
                JsonSerializer.Serialize(writer, fv.Values, JsonSerializerConstants.SerializerOptions);

                return;
            case ByteVector bv:
                JsonSerializer.Serialize(writer, bv.Values, JsonSerializerConstants.SerializerOptions);

                return;
            case NamedVectors nv:
            {
                writer.WriteStartObject();
                {
                    // named vector contains either Vector or SparseVector as value

                    foreach (var (vectorName, vector) in nv.Vectors)
                    {
                        writer.WritePropertyName(vectorName);

                        if (vector.IsSparseVector)
                        {
                            switch (vector.DataType)
                            {
                                case VectorDataType.Float32:
                                {
                                    var sparseVector = vector.AsSparseFloatVector();
                                    JsonSerializer.Serialize(writer, sparseVector, JsonSerializerConstants.SerializerOptions);

                                    break;
                                }
                                case VectorDataType.Uint8:
                                {
                                    var sparseVector = vector.AsSparseByteVector();
                                    JsonSerializer.Serialize(writer, sparseVector, JsonSerializerConstants.SerializerOptions);

                                    break;
                                }
                                default:
                                    throw new QdrantJsonSerializationException($"Can't serialize vector with data type of {vector.DataType}");
                            }
                        }
                        else
                        {
                            // means this vector is a non-sparse one

                            switch (vector.DataType)
                            {
                                case VectorDataType.Float32:
                                {
                                    var singleVector = vector.AsFloatVector();

                                    JsonSerializer.Serialize(
                                        writer,
                                        singleVector.Values,
                                        JsonSerializerConstants.SerializerOptions);

                                    break;
                                }
                                case VectorDataType.Uint8:
                                {
                                    var singleVector = vector.AsByteVector();

                                    JsonSerializer.Serialize(
                                        writer,
                                        singleVector.Values,
                                        JsonSerializerConstants.SerializerOptions);

                                    break;
                                }
                                default:
                                    throw new QdrantJsonSerializationException(
                                        $"Can't serialize vector with data type of {vector.DataType}");
                            }
                        }
                    }
                }
                writer.WriteEndObject();

                return;
            }
            default:
                throw new QdrantJsonSerializationException($"Can't serialize {value} vector of type {value.GetType()}");
        }
    }
}
