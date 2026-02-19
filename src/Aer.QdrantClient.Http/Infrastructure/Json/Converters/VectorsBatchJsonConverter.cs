using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Helpers.NetstandardPolyfill;
using Aer.QdrantClient.Http.Models.Primitives.Vectors;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class VectorsBatchJsonConverter : JsonConverter<UpsertPointsBatch.VectorsBatch>
{
    private static readonly JsonSerializerOptions _serializerOptions =
         JsonSerializerConstants.CreateSerializerOptions(new VectorJsonConverter());

    public override UpsertPointsBatch.VectorsBatch Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Reading vectors' batch is not supported");
    }

    /*
     Example of the request with different vector types
     {
       "batch": {
         "ids": [1, 2],
         "vectors": {
           "dense-vector": [
             [0.1, 0.2, 0.3],
             [0.4, 0.5, 0.6]
           ],
           "sparse-vector": [
             {
               "indices": [10, 20],
               "values": [0.9, 0.1]
             },
             {
               "indices": [15, 25],
               "values": [0.8, 0.2]
             }
           ],
           "multi-vector": [
             [
               [0.1, 0.2], [0.3, 0.4]
             ],
             [
               [0.5, 0.6], [0.7, 0.8]
             ]
           ]
         },
         "payloads": [
           { "test": "a" },
           { "test": "b" }
         ]
       }
     }
   */
    public override void Write(Utf8JsonWriter writer, UpsertPointsBatch.VectorsBatch value, JsonSerializerOptions options)
    {
        if (HasVectors(value.Vectors))
        {
            // Means that the request contains only unnamed dense/multi vectors

            writer.WriteStartArray();

            foreach (var vector in value.Vectors)
            {
                JsonSerializer.Serialize(writer, vector, _serializerOptions);
            }

            writer.WriteEndArray();
        }
        else if (value.NamedVectors is {Count: > 0})
        {
            writer.WriteStartObject();

            // Vectors of each type are grouped by their names in the batch
            foreach (var vectorBatch in value.NamedVectors)
            {
                writer.WritePropertyName(vectorBatch.Key);

                writer.WriteStartArray();

                foreach (var vector in vectorBatch.Value)
                {
                    JsonSerializer.Serialize(writer, vector, _serializerOptions);
                }

                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
        else
        {
            throw new QdrantJsonSerializationException($"Can't serialize vectors' batch since unnamed and named vectors are empty");
        }
    }

    private bool HasVectors(IEnumerable<VectorBase> vectors)
    {
        if (vectors is null)
        {
            return false;
        }

        if (vectors.TryGetNonEnumeratedCount(out var vectorsCount))
        {
            return vectorsCount > 0;
        }

        using var enumerator = vectors.GetEnumerator();

        return enumerator.MoveNext();
    }
}
