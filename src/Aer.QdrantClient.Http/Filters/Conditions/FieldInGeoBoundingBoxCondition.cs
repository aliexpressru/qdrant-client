using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json;

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

        using (jsonWriter.WriteObject("geo_bounding_box"))
        {
            using (jsonWriter.WriteObject("bottom_right"))

            {
                jsonWriter.WriteNumber("lat", bottomRightLatitude);
                jsonWriter.WriteNumber("lon", bottomRightLongitude);
            }

            using (jsonWriter.WriteObject("top_left"))

            {
                jsonWriter.WriteNumber("lat", topLeftLatitude);
                jsonWriter.WriteNumber("lon", topLeftLongitude);
            }
        }
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldInGeoBoundingBoxCondition(this);
}
