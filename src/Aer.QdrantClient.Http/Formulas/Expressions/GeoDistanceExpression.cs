using Aer.QdrantClient.Http.Infrastructure.Helpers;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a Haversine geo distance expression that calculates distance between origin and target geographic point.
/// </summary>
internal sealed class GeoDistanceExpression(double originLongitude, double originLatitude, string toPayloadFieldName) : ExpressionBase
{
    public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
    {
        using (jsonWriter.WriteObject())
        {
            jsonWriter.WritePropertyName("geo_distance");

            jsonWriter.WriteObject();
            {
                jsonWriter.WritePropertyName("origin");

                jsonWriter.WriteObject();
                {
                    jsonWriter.WritePropertyName("lon");
                    jsonWriter.WriteNumberValue(originLongitude);

                    jsonWriter.WritePropertyName("lat");
                    jsonWriter.WriteNumberValue(originLatitude);
                }

                jsonWriter.WritePropertyName("to");
                jsonWriter.WriteStringValue(toPayloadFieldName);
            }
        }
    }
}
