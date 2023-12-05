using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Models.Responses;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class QdrantCollectionOptimizerStatusJsonConverter : JsonConverter<GetCollectionInfoResponse.QdrantOptimizerStatusUint>
{
    public override GetCollectionInfoResponse.QdrantOptimizerStatusUint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            {
                var statusStringValue = reader.GetString();

                if (statusStringValue == "ok")
                {
                    return new GetCollectionInfoResponse.QdrantOptimizerStatusUint(
                        QdrantOptimizerStatus.Ok);
                }

                // means string with some unknown content
                return new GetCollectionInfoResponse.QdrantOptimizerStatusUint(
                    QdrantOptimizerStatus.Unknown)
                {
                    RawStatusString = statusStringValue
                };
            }

            case JsonTokenType.StartObject:
            {
                // means complex status like this:
                //  "optimizer_status": {
                //      "error":"Some error message"
                //  },

                var statusObject = JsonNode.Parse(ref reader);

                var errorMesage = statusObject?["error"]?.GetValue<string>();

                if (errorMesage is not null)
                {
                    return new GetCollectionInfoResponse.QdrantOptimizerStatusUint(
                        QdrantOptimizerStatus.Error)
                    {
                        Error = errorMesage
                    };
                }

                return new GetCollectionInfoResponse.QdrantOptimizerStatusUint(
                    QdrantOptimizerStatus.Unknown)
                {
                    RawStatusString = statusObject?.ToJsonString(JsonSerializerConstants.IndentedSerializerOptions)
                };
            }

            default:
                throw new QdrantJsonSerializationException("Unbable to deserialize Qdrant collection optimizer status value");
        }
    }

    public override void Write(Utf8JsonWriter writer, GetCollectionInfoResponse.QdrantOptimizerStatusUint value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, JsonSerializerConstants.SerializerOptions);
    }
}
