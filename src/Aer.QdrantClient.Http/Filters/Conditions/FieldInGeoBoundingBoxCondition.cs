using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal class FieldInGeoBoundingBoxCondition : FilterConditionBase
{
    private readonly double _topLeftLongtitude;
    private readonly double _topLeftLatitude;
    private readonly double _bottomRightLongtitude;
    private readonly double _bottomRightLatitude;

    public FieldInGeoBoundingBoxCondition(
        string payloadFieldName,
        double topLeftLongtitude,
        double topLeftLatitude,
        double bottomRightLongtitude,
        double bottomRightLatitude) : base(payloadFieldName)
    {
        _topLeftLongtitude = topLeftLongtitude;
        _topLeftLatitude = topLeftLatitude;
        _bottomRightLongtitude = bottomRightLongtitude;
        _bottomRightLatitude = bottomRightLatitude;
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("geo_bounding_box");

        jsonWriter.WriteStartObject();
        {

            jsonWriter.WritePropertyName("bottom_right");
            jsonWriter.WriteStartObject();

            {
                jsonWriter.WriteNumber("lat", _bottomRightLatitude);
                jsonWriter.WriteNumber("lon", _bottomRightLongtitude);
            }

            jsonWriter.WriteEndObject();

            jsonWriter.WritePropertyName("top_left");
            jsonWriter.WriteStartObject();

            {
                jsonWriter.WriteNumber("lat", _topLeftLatitude);
                jsonWriter.WriteNumber("lon", _topLeftLongtitude);
            }

            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndObject();
    }
}
