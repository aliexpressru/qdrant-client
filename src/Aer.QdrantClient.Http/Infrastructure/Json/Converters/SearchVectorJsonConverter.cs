using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Requests.Public.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class SearchVectorJsonConverter : JsonConverter<SearchVector>
{
    public override SearchVector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Reading {nameof(SearchVector)} instances is not supported");
    }

    public override void Write(Utf8JsonWriter writer, SearchVector value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case SearchVector.DenseSearchVector uv:
                JsonSerializer.Serialize(writer, uv.Vector, JsonSerializerConstants.DefaultSerializerOptions);

                return;
            case SearchVector.NamedDenseSearchVector nv:
                JsonSerializer.Serialize(writer, nv, JsonSerializerConstants.DefaultSerializerOptions);

                return;

            case SearchVector.SparseSearchVector usv:
                JsonSerializer.Serialize(writer, usv.Vector, JsonSerializerConstants.DefaultIndentedSerializerOptions);

                return;

            case SearchVector.NamedSparseSearchVector nsv:
                JsonSerializer.Serialize(writer, nsv, JsonSerializerConstants.DefaultIndentedSerializerOptions);

                return;

            default:
                throw new QdrantJsonSerializationException($"Can't serialize {value} search vector of type {value.GetType()}");
        }
    }
}
