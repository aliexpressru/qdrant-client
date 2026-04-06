using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class QdrantStatusJsonConverter : JsonConverter<QdrantStatus>
{
    public override QdrantStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            {
                var statusStringValue = reader.GetString();

                if (statusStringValue == QdrantStatus.OkStatusString)
                {
                    return new QdrantStatus(QdrantOperationStatusType.Ok);
                }

                // means string with some unknown content
                return new QdrantStatus(QdrantOperationStatusType.Unknown) { RawStatusString = statusStringValue };
            }

            case JsonTokenType.StartObject:
            {
                // means complex status like this:
                //  "status": {
                //      "error":"Wrong input: Collection `test_collection` already exists!"
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
                    return new QdrantStatus(QdrantOperationStatusType.Error) { Error = errorMessage };
                }

                return new QdrantStatus(QdrantOperationStatusType.Unknown) { RawStatusString = statusObject.Value.GetRawText() };
            }

            default:
                throw new QdrantJsonParsingException("Unable to deserialize Qdrant status value");
        }
    }

    public override void Write(Utf8JsonWriter writer, QdrantStatus value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, JsonSerializerConstants.DefaultSerializerOptions);
}
