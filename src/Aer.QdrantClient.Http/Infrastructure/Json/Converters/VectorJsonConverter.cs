using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class VectorJsonConverter : JsonConverter<VectorBase>
{
    public override VectorBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartArray:

                var vectorValuesJArray = JsonNode.Parse(ref reader)?.AsArray();

                return ReadSingleOrMultiVectorFromJArray(vectorValuesJArray, typeToConvert);

            case JsonTokenType.StartObject:

                // means named vectors collection

                // name - vector name
                // value can be either a simple vector
                // "vector1" : [0.0, 0.1, 0.2], "vector2" " [10, 11, 12]
                // or a sparse vector
                // "vector1" : {"indices": [6, 7], "values": [1.0, 2.0]}
                // or a multivector
                // "vector1" : [[0.0, 0.1, 0.2],[0.02, 0.12, 0.22],[0.03, 0.13, 0.23]]

                var namedVectorsJObject = JsonNode.Parse(ref reader);

                var namedVectorsObject = namedVectorsJObject
                    .Deserialize<Dictionary<string, JsonNode>>(JsonSerializerConstants.DefaultSerializerOptions);

                var vectors = new Dictionary<string, VectorBase>();

                foreach (var (vectorName, vector) in namedVectorsObject)
                {
                    switch (vector)
                    {
                        case JsonArray singleOrMultiVectorValues:

                            var readVector = ReadSingleOrMultiVectorFromJArray(singleOrMultiVectorValues, typeToConvert);

                            vectors.Add(vectorName, readVector);
                            break;

                        case JsonObject sparseVectorValues:
                            var sparseVector = sparseVectorValues
                                .Deserialize<SparseVector>(JsonSerializerConstants.DefaultSerializerOptions);

                            vectors.Add(vectorName, sparseVector);
                            break;

                        default:
                            throw new QdrantJsonParsingException(
                                $"Unable to deserialize Qdrant vector value. Unexpected vector representation : {vector.GetType()}");
                    }
                }

                return new NamedVectors()
                {
                    Vectors = vectors
                };

            default:
                throw new QdrantJsonParsingException("Unable to deserialize Qdrant vector value");
        }
    }

    public override void Write(Utf8JsonWriter writer, VectorBase value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case DenseVector v:
                JsonSerializer.Serialize(writer, v.VectorValues, JsonSerializerConstants.DefaultSerializerOptions);
                return;

            case SparseVector sv:
                writer.WriteStartObject();
                {
                    writer.WritePropertyName("indices");
                    JsonSerializer.Serialize(writer, sv.Indices, JsonSerializerConstants.DefaultSerializerOptions);
                    writer.WritePropertyName("values");
                    JsonSerializer.Serialize(writer, sv.Values, JsonSerializerConstants.DefaultSerializerOptions);
                }
                writer.WriteEndObject();
                return;

            case MultiVector mv:
                JsonSerializer.Serialize(writer, mv.Vectors, JsonSerializerConstants.DefaultSerializerOptions);
                return;

            case NamedVectors nv:
                writer.WriteStartObject();
                {
                    // named vector contains either DenseVector, SparseVector or MultiVector as value

                    foreach (var (vectorName, vector) in nv.Vectors)
                    {
                        writer.WritePropertyName(vectorName);

                        switch (vector.VectorKind)
                        {
                            case VectorKind.Dense:
                                var singleVector = vector.AsDenseVector();

                                JsonSerializer.Serialize(
                                    writer,
                                    singleVector.VectorValues,
                                    JsonSerializerConstants.DefaultSerializerOptions);

                                break;
                            case VectorKind.Sparse:
                                var sparseVector = vector.AsSparseVector();

                                JsonSerializer.Serialize(
                                    writer,
                                    sparseVector,
                                    JsonSerializerConstants.DefaultSerializerOptions);

                                break;
                            case VectorKind.Multi:

                                var multiVector = vector.AsMultiVector();

                                JsonSerializer.Serialize(
                                    writer,
                                    multiVector.Vectors,
                                    JsonSerializerConstants.DefaultSerializerOptions);

                                break;

                            case VectorKind.Named:
                                throw new QdrantJsonSerializationException(
                                    $"Can't serialize {value} vector of type {value.GetType()}. Named vector can't be a member of another named vector");

                            default:
                                throw new QdrantJsonSerializationException(
                                    $"Can't serialize {value} vector of type {value.GetType()}. Unknown vector kind {vector.VectorKind}");
                        }
                    }
                }
                writer.WriteEndObject();
                return;

            default:
                throw new QdrantJsonSerializationException($"Can't serialize {value} vector of type {value.GetType()}");
        }
    }

    private static VectorBase ReadSingleOrMultiVectorFromJArray(JsonArray vectorValuesJArray, Type typeToConvert)
    {
        // vectorValuesJArray can either contain a multivector or a single vector

        if (vectorValuesJArray is null or { Count: 0 })
        {
            throw new QdrantJsonParsingException(
                $"Unable to deserialize Qdrant vector value as {typeToConvert}. The vector value is missing");
        }

        // check first array value - if it is JArray itself - we are dealing with multivector
        // if it is a number value - we are deserializing a single vector

        var firstJArrayValueKind = vectorValuesJArray[0]!.GetValueKind();

        switch (firstJArrayValueKind)
        {
            case JsonValueKind.Number:
                var vectorValuesArray =
                    vectorValuesJArray.Deserialize<float[]>(JsonSerializerConstants.DefaultSerializerOptions);

                return new DenseVector()
                {
                    VectorValues = vectorValuesArray
                };

            case JsonValueKind.Array:
                var multiVectorValuesArray =
                    vectorValuesJArray.Deserialize<float[][]>(JsonSerializerConstants.DefaultSerializerOptions);

                return new MultiVector()
                {
                    Vectors = multiVectorValuesArray
                };

            case JsonValueKind.Undefined:
            case JsonValueKind.Object:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
            default:
                throw new InvalidOperationException($"Unsupported JSON token kind: {firstJArrayValueKind}");
        }
    }
}
