using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class QueryVectorJsonConverter : JsonConverter<QueryVector>
{
    public override QueryVector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException($"Reading {nameof(QueryVector)} instances is not supported");

    public override void Write(Utf8JsonWriter writer, QueryVector value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case QueryVector.DenseQueryVector dv:
                JsonSerializer.Serialize(writer, dv.Vector, JsonSerializerConstants.DefaultSerializerOptions);

                return;
            case QueryVector.SparseQueryVector sv:
                JsonSerializer.Serialize(writer, sv.Vector, JsonSerializerConstants.DefaultIndentedSerializerOptions);

                return;
            default:
                throw new QdrantJsonSerializationException($"Can't serialize {value} query vector of type {value.GetType()}");
        }
    }
}
