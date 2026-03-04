using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Requests.Public.QueryPoints.RelevanceFeedback;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal class FeedbackStrategyJsonConverter : JsonConverter<FeedbackStrategy>
{
    public override FeedbackStrategy Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotSupportedException($"Reading {nameof(FeedbackStrategy)} instances is not supported");

    public override void Write(Utf8JsonWriter writer, FeedbackStrategy value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case NaiveFeedbackStrategy nfs:

                using (writer.WriteObject())
                {
                    writer.WritePropertyName("naive");

                    JsonSerializer.Serialize(
                        writer,
                        nfs,
                        JsonSerializerConstants.DefaultSerializerOptions
                    );
                }

                break;
            default:
                throw new QdrantJsonSerializationException($"Unknown feedback strategy {value.GetType()}");
        }
    }
}
