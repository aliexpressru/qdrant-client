using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldInGeoRadiusCondition(
    string payloadFieldName,
    double centerLongitude,
    double centerLatitude,
    double radius) : FilterConditionBase(payloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => PayloadIndexedFieldType.Geo;

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("geo_radius");

        jsonWriter.WriteStartObject();
        {

            jsonWriter.WritePropertyName("center");
            jsonWriter.WriteStartObject();

            {
                jsonWriter.WriteNumber("lat", centerLatitude);
                jsonWriter.WriteNumber("lon", centerLongitude);
            }

            jsonWriter.WriteEndObject();

            jsonWriter.WriteNumber("radius", radius);
        }

        jsonWriter.WriteEndObject();
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldInGeoRadiusCondition(this);
}
