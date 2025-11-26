using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Filters;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class QdrantFilterJsonConverter : JsonConverter<QdrantFilter>
{
    public override QdrantFilter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException($"Reading {nameof(QdrantFilter)} instances is not supported");

    public override void Write(Utf8JsonWriter writer, QdrantFilter value, JsonSerializerOptions options) => value.WriteFilterJson(writer);
}
