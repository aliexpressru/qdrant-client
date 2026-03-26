using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class QdrantCollectionOptimizerStatusJsonConverter
    : JsonConverter<GetCollectionInfoResponse.QdrantOptimizerStatusUint>
{
    public override GetCollectionInfoResponse.QdrantOptimizerStatusUint Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            {
                var statusStringValue = reader.GetString();

                if (statusStringValue == "ok")
                {
                    return new GetCollectionInfoResponse.QdrantOptimizerStatusUint(QdrantOptimizerStatus.Ok);
                }

                // means string with some unknown content
                return new GetCollectionInfoResponse.QdrantOptimizerStatusUint(QdrantOptimizerStatus.Unknown)
                {
                    RawStatusString = statusStringValue,
                };
            }

            case JsonTokenType.StartObject:
            {
                // means complex status like this:
                //  "optimizer_status": {
                //      "error":"Some error message"
                //  },

                string errorMessage = null;

                if (
                    JsonElement.TryParseValue(ref reader, out var statusObject)
                    && statusObject.Value.TryGetProperty("error", out var errorProperty)
                )
                {
                    errorMessage = errorProperty.GetString();
                }

                if (errorMessage is not null)
                {
                    return new GetCollectionInfoResponse.QdrantOptimizerStatusUint(QdrantOptimizerStatus.Error)
                    {
                        Error = errorMessage,
                    };
                }

                return new GetCollectionInfoResponse.QdrantOptimizerStatusUint(QdrantOptimizerStatus.Unknown)
                {
                    RawStatusString = statusObject.Value.GetRawText(),
                };
            }

            default:
                throw new QdrantJsonSerializationException("Unable to deserialize Qdrant collection optimizer status value");
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        GetCollectionInfoResponse.QdrantOptimizerStatusUint value,
        JsonSerializerOptions options
    ) => JsonSerializer.Serialize(writer, value, JsonSerializerConstants.DefaultSerializerOptions);
}
