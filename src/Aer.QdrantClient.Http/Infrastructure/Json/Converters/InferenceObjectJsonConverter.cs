using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Primitives.Inference;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class InferenceObjectJsonConverter : JsonConverter<InferenceObject>
{
    private static readonly JsonSerializerOptions _serializerOptions =
        JsonSerializerConstants.CreateSerializerOptions(
            new FullTextIndexStemmingAlgorithmJsonConverter()
        );

    public override InferenceObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
    throw new NotSupportedException($"Reading {typeof(InferenceObject)} instances is not supported");

    public override void Write(Utf8JsonWriter writer, InferenceObject value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case ImageInferenceObject iio:
                JsonSerializer.Serialize(
                    writer,
                    iio,
                    JsonSerializerConstants.DefaultSerializerOptions);

                break;

            case DocumentInferenceObject tio:
                // Here we need to handle BM25 options override.

                object optionsObject = tio.Bm25Options is null
                    ? tio.Options
                    : tio.Bm25Options;

                using (writer.WriteObject())
                {
                    writer.WriteString("text", tio.Text);
                    writer.WriteString("model", tio.Model);
                    if (optionsObject is not null)
                    {
                        writer.WritePropertyName("options");

                        JsonSerializer.Serialize(
                        writer,
                        optionsObject,
                        _serializerOptions);
                    }
                }

                break;

            case ObjectInferenceObject oio:
                JsonSerializer.Serialize(
                    writer,
                    oio,
                    JsonSerializerConstants.DefaultSerializerOptions);

                break;

            default:
                throw new QdrantJsonSerializationException($"Unknown inference object {value.GetType()}");
        }
    }
}
