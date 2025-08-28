using System.Text.Json;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldInGeoPolygonCondition : FilterConditionBase
{
    private readonly IEnumerable<GeoPoint> _exteriorPolygonPoints;
    private readonly IEnumerable<GeoPoint>[] _interiorPolygonsPoints;

    protected internal override PayloadIndexedFieldType? PayloadFieldType => PayloadIndexedFieldType.Geo;
    
    public FieldInGeoPolygonCondition(
        string payloadFieldName,
        IEnumerable<GeoPoint> exteriorPolygonPoints,
        IEnumerable<GeoPoint>[] interiorPolygonsPoints) : base(payloadFieldName)
    {
        _exteriorPolygonPoints = exteriorPolygonPoints;
        _interiorPolygonsPoints = interiorPolygonsPoints;
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
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
                    foreach (var exteriorPolygonPoint in _exteriorPolygonPoints)
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

            if (_interiorPolygonsPoints is {Length: > 0})
            {

                foreach (var interiorPolygonPoints in _interiorPolygonsPoints)
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
}
