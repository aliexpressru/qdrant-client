using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json;

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

        using (jsonWriter.WriteObject("geo_polygon"))
        {
            using (jsonWriter.WriteObject("exterior"))
            {
                using (jsonWriter.WriteArray("points"))
                {
                    foreach (var exteriorPolygonPoint in exteriorPolygonPoints)
                    {
                        using (jsonWriter.WriteObject())
                        {
                            jsonWriter.WriteNumber("lat", exteriorPolygonPoint.Latitude);

                            jsonWriter.WriteNumber("lon", exteriorPolygonPoint.Longitude);
                        }
                    }
                }
            }

            using (jsonWriter.WriteArray("interiors"))
            {
                if (interiorPolygonsPoints is { Length: > 0 })
                {
                    foreach (var interiorPolygonPoints in interiorPolygonsPoints)
                    {
                        using (jsonWriter.WriteObject())
                        {
                            using (jsonWriter.WriteArray("points"))
                            {
                                foreach (var interiorPolygonPoint in interiorPolygonPoints)
                                {
                                    using (jsonWriter.WriteObject())
                                    {
                                        jsonWriter.WriteNumber("lat", interiorPolygonPoint.Latitude);

                                        jsonWriter.WriteNumber("lon", interiorPolygonPoint.Longitude);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldInGeoPolygonCondition(this);
}
