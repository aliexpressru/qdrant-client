using System.Text.Json;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldInGeoBoundingBoxCondition : FilterConditionBase
{
    private readonly double _topLeftLongitude;
    private readonly double _topLeftLatitude;
    private readonly double _bottomRightLongitude;
    private readonly double _bottomRightLatitude;

    protected internal override PayloadIndexedFieldType? PayloadFieldType => PayloadIndexedFieldType.Geo;

    public FieldInGeoBoundingBoxCondition(
        string payloadFieldName,
        double topLeftLongitude,
        double topLeftLatitude,
        double bottomRightLongitude,
        double bottomRightLatitude) : base(payloadFieldName)
    {
        _topLeftLongitude = topLeftLongitude;
        _topLeftLatitude = topLeftLatitude;
        _bottomRightLongitude = bottomRightLongitude;
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
                jsonWriter.WriteNumber("lon", _bottomRightLongitude);
            }

            jsonWriter.WriteEndObject();

            jsonWriter.WritePropertyName("top_left");
            jsonWriter.WriteStartObject();

            {
                jsonWriter.WriteNumber("lat", _topLeftLatitude);
                jsonWriter.WriteNumber("lon", _topLeftLongitude);
            }

            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndObject();
    }
}
