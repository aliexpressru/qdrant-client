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
            using (jsonWriter.WriteObject("geo_distance"))
            {
                using (jsonWriter.WriteObject("origin"))
                {
                    jsonWriter.WriteNumber("lon", originLongitude);
                    jsonWriter.WriteNumber("lat", originLatitude);
                }

                jsonWriter.WriteString("to", toPayloadFieldName);
            }
        }
    }
}
