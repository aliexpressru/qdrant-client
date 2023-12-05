using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal class FieldInGeoRadiusCondition : FilterConditionBase
{
    private readonly double _centerLongtitude;
    private readonly double _centerLatitude;
    private readonly double _radius;

    public FieldInGeoRadiusCondition(
        string payloadFieldName,
        double centerLongtitude,
        double centerLatitude,
        double radius) : base(payloadFieldName)
    {
        _centerLongtitude = centerLongtitude;
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
                jsonWriter.WriteNumber("lon", _centerLongtitude);
            }

            jsonWriter.WriteEndObject();

            jsonWriter.WriteNumber("radius", _radius);
        }

        jsonWriter.WriteEndObject();
    }
}
