using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class JsonStringSnakeCaseLowerEnumConverter : JsonStringEnumConverter
{
    public JsonStringSnakeCaseLowerEnumConverter() : base(JsonNamingPolicy.SnakeCaseLower) { }
}

internal class JsonStringSnakeCaseLowerEnumConverter<T> : JsonStringEnumConverter<T>
    where T : struct, Enum
{
    public JsonStringSnakeCaseLowerEnumConverter() : base(JsonNamingPolicy.SnakeCaseLower) { }
}
