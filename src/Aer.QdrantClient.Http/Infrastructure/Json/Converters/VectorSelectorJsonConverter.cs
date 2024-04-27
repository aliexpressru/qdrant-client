using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class VectorSelectorJsonConverter : JsonConverter<VectorSelector>
{
    public override VectorSelector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Reading {nameof(VectorSelector)} instances is not supported");
    }

    public override void Write(Utf8JsonWriter writer, VectorSelector value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case VectorSelector.AllVectorsSelector avs:
                writer.WriteBooleanValue(avs.AreAllVectorsSelected);

                return;
            case VectorSelector.IncludeNamedVectorsSelector ivs:
                JsonSerializer.Serialize(writer, ivs.IncludedVectorNames, JsonSerializerConstants.SerializerOptions);

                return;
            default:
                throw new QdrantJsonSerializationException($"Can't serialize {value} vector selector of type {value.GetType()}");
        }
    }
}
