using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class QdrantStatusJsonConverter : JsonConverter<QdrantStatus>
{
    public override QdrantStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            {
                var statusStringValue = reader.GetString();

                if (statusStringValue == "ok")
                {
                    return new QdrantStatus(QdrantOperationStatusType.Ok);
                }

                // means string with some unknown content
                return new QdrantStatus(QdrantOperationStatusType.Unknown)
                {
                    RawStatusString = statusStringValue
                };
            }

            case JsonTokenType.StartObject:
            {
                // means complex status like this:
                //  "status": {
                //      "error":"Wrong input: Collection `test_collection` already exists!"
                //  },

                var statusObject = JsonNode.Parse(ref reader);

                var errorMesage = statusObject?["error"]?.GetValue<string>();

                if (errorMesage is not null)
                {
                    return new QdrantStatus(QdrantOperationStatusType.Error)
                    {
                        Error = errorMesage
                    };
                }

                return new QdrantStatus(QdrantOperationStatusType.Unknown)
                {
                    RawStatusString = statusObject?.ToJsonString(JsonSerializerConstants.IndentedSerializerOptions)
                };
            }

            default:
                throw new QdrantJsonParsingException("Unbable to deserialize Qdrant status value");
        }
    }

    public override void Write(Utf8JsonWriter writer, QdrantStatus value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, JsonSerializerConstants.SerializerOptions);
    }
}
