using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a Haversine geo distance expression that calculates distance between origin and target geographic point.
/// </summary>
internal sealed class GeoDistanceExpression : ExpressionBase
{
	private readonly double _originLongitude;
	private readonly double _originLatitude;
	private readonly string _toPayloadFieldName;

	public GeoDistanceExpression(double originLongitude, double originLatitude, string toPayloadFieldName)
	{
		_originLongitude = originLongitude;
		_originLatitude = originLatitude;
		_toPayloadFieldName = toPayloadFieldName;
	}

	public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
	{
		jsonWriter.WriteStartObject();
		{
			jsonWriter.WritePropertyName("geo_distance");

			jsonWriter.WriteStartObject();
			{
				jsonWriter.WritePropertyName("origin");
				
				jsonWriter.WriteStartObject();
				{ 
					jsonWriter.WritePropertyName("lon");
					jsonWriter.WriteNumberValue(_originLongitude);
					
					jsonWriter.WritePropertyName("lat");
					jsonWriter.WriteNumberValue(_originLatitude);
				}
				jsonWriter.WriteEndObject();
				
				jsonWriter.WritePropertyName("to");
				jsonWriter.WriteStringValue(_toPayloadFieldName);
			}
			jsonWriter.WriteEndObject();
		}
		jsonWriter.WriteEndObject();
	}
}
