using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json;

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

        using (jsonWriter.WriteObject("geo_radius"))
        {
            using (jsonWriter.WriteObject("center"))

            {
                jsonWriter.WriteNumber("lat", centerLatitude);
                jsonWriter.WriteNumber("lon", centerLongitude);
            }

            jsonWriter.WriteNumber("radius", radius);
        }
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldInGeoRadiusCondition(this);
}
