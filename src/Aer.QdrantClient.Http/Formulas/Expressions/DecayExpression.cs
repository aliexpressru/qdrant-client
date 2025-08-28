using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a decay expression with specified decay function and parameters.
/// </summary>
internal sealed class DecayExpression : ExpressionBase
{
	private readonly string _decayOperator;
	private readonly ExpressionBase _x;
	private readonly ExpressionBase _target;
	private readonly double? _scale;
	private readonly double? _midpoint;

	public DecayExpression(
		string decayOperator,
		ExpressionBase x,
		ExpressionBase target = null,
		double? scale = null,
		double? midpoint = null)
	{
		_decayOperator = decayOperator ?? throw new ArgumentNullException(nameof(decayOperator));
		_x = x ?? throw new ArgumentNullException(nameof(x));
		
		_target = target;
		_scale = scale;
		_midpoint = midpoint;
	}

	public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
	{
		jsonWriter.WriteStartObject();
		{
			jsonWriter.WritePropertyName(_decayOperator);

			jsonWriter.WriteStartObject();
			{
				jsonWriter.WritePropertyName("x");
				_x.WriteExpressionJson(jsonWriter);

				if (_target is { } target)
				{ 
					jsonWriter.WritePropertyName("target");
					target.WriteExpressionJson(jsonWriter);
				}
				
				if (_scale.HasValue)
				{
					jsonWriter.WritePropertyName("scale");
					jsonWriter.WriteNumberValue(_scale.Value);
				}
				
				if (_midpoint.HasValue)
				{
					jsonWriter.WritePropertyName("midpoint");
					jsonWriter.WriteNumberValue(_midpoint.Value);
				}
			}
			jsonWriter.WriteEndObject();
		}
		jsonWriter.WriteEndObject();
	}
}
