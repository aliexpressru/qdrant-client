using System.Text.Json;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldInGeoRadiusCondition : FilterConditionBase
{
    private readonly double _centerLongitude;
    private readonly double _centerLatitude;
    private readonly double _radius;

    protected internal override PayloadIndexedFieldType? PayloadFieldType => PayloadIndexedFieldType.Geo;

    public FieldInGeoRadiusCondition(
        string payloadFieldName,
        double centerLongitude,
        double centerLatitude,
        double radius) : base(payloadFieldName)
    {
        _centerLongitude = centerLongitude;
        _centerLatitude = centerLatitude;
        _radius = radius;
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("geo_radius");

        jsonWriter.WriteStartObject();
        {

            jsonWriter.WritePropertyName("center");
            jsonWriter.WriteStartObject();

            {
                jsonWriter.WriteNumber("lat", _centerLatitude);
                jsonWriter.WriteNumber("lon", _centerLongitude);
            }

            jsonWriter.WriteEndObject();

            jsonWriter.WriteNumber("radius", _radius);
        }

        jsonWriter.WriteEndObject();
    }
}
