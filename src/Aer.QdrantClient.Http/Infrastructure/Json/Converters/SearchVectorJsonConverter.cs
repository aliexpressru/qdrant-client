using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class SearchVectorJsonConverter : JsonConverter<SearchVector>
{
    public override SearchVector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Reading {nameof(SearchVector)} instances is not supported");
    }

    public override void Write(Utf8JsonWriter writer, SearchVector value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case SearchVector.UnnamedSearchVector uv:
                JsonSerializer.Serialize(writer, uv.Vector, JsonSerializerConstants.SerializerOptions);

                return;
            case SearchVector.NamedSearchVector nv:
                JsonSerializer.Serialize(writer, nv, JsonSerializerConstants.SerializerOptions);

                return;
            default:
                throw new QdrantJsonSerializationException($"Can't serialize {value} vector of type {value.GetType()}");
        }
    }
}
