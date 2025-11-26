using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldInGeoBoundingBoxCondition(
    string payloadFieldName,
    double topLeftLongitude,
    double topLeftLatitude,
    double bottomRightLongitude,
    double bottomRightLatitude) : FilterConditionBase(payloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => PayloadIndexedFieldType.Geo;

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("geo_bounding_box");

        jsonWriter.WriteStartObject();
        {

            jsonWriter.WritePropertyName("bottom_right");
            jsonWriter.WriteStartObject();

            {
                jsonWriter.WriteNumber("lat", bottomRightLatitude);
                jsonWriter.WriteNumber("lon", bottomRightLongitude);
            }

            jsonWriter.WriteEndObject();

            jsonWriter.WritePropertyName("top_left");
            jsonWriter.WriteStartObject();

            {
                jsonWriter.WriteNumber("lat", topLeftLatitude);
                jsonWriter.WriteNumber("lon", topLeftLongitude);
            }

            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndObject();
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldInGeoBoundingBoxCondition(this);
}
