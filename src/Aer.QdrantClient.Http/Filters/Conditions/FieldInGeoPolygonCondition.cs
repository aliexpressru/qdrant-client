using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldInGeoPolygonCondition(
    string payloadFieldName,
    IEnumerable<GeoPoint> exteriorPolygonPoints,
    IEnumerable<GeoPoint>[] interiorPolygonsPoints) : FilterConditionBase(payloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => PayloadIndexedFieldType.Geo;

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("geo_polygon");

        jsonWriter.WriteStartObject();
        {
            jsonWriter.WritePropertyName("exterior");
            jsonWriter.WriteStartObject();
            {
                jsonWriter.WritePropertyName("points");
                jsonWriter.WriteStartArray();
                {
                    foreach (var exteriorPolygonPoint in exteriorPolygonPoints)
                    {
                        jsonWriter.WriteStartObject();
                        {
                            jsonWriter.WriteNumber("lat", exteriorPolygonPoint.Latitude);
                            jsonWriter.WriteNumber("lon", exteriorPolygonPoint.Longitude);
                        }
                        jsonWriter.WriteEndObject();
                    }
                }
                jsonWriter.WriteEndArray();
            }
            jsonWriter.WriteEndObject();

            jsonWriter.WritePropertyName("interiors");

            jsonWriter.WriteStartArray();

            if (interiorPolygonsPoints is { Length: > 0 })
            {

                foreach (var interiorPolygonPoints in interiorPolygonsPoints)
                {
                    jsonWriter.WriteStartObject();
                    {
                        jsonWriter.WritePropertyName("points");
                        jsonWriter.WriteStartArray();
                        {
                            foreach (var interiorPolygonPoint in interiorPolygonPoints)
                            {
                                jsonWriter.WriteStartObject();
                                {
                                    jsonWriter.WriteNumber("lat", interiorPolygonPoint.Latitude);
                                    jsonWriter.WriteNumber("lon", interiorPolygonPoint.Longitude);
                                }
                                jsonWriter.WriteEndObject();
                            }
                        }
                        jsonWriter.WriteEndArray();
                    }
                    jsonWriter.WriteEndObject();
                }
            }

            jsonWriter.WriteEndArray();
        }

        jsonWriter.WriteEndObject();
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldInGeoPolygonCondition(this);
}
