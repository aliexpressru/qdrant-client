using Aer.QdrantClient.Http.Infrastructure.Helpers;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

/// <summary>
/// Represents a decay expression with specified decay function and parameters.
/// </summary>
internal sealed class DecayExpression(
    string decayOperator,
    ExpressionBase x,
    ExpressionBase target = null,
    double? scale = null,
    double? midpoint = null) : ExpressionBase
{
    private readonly string _decayOperator = decayOperator ?? throw new ArgumentNullException(nameof(decayOperator));
    private readonly ExpressionBase _x = x ?? throw new ArgumentNullException(nameof(x));
    private readonly ExpressionBase _target = target;

    public override void WriteExpressionJson(Utf8JsonWriter jsonWriter)
    {
        using (jsonWriter.WriteObject())
        {
            using (jsonWriter.WriteObject(_decayOperator))
            {
                jsonWriter.WritePropertyName("x");
                _x.WriteExpressionJson(jsonWriter);

                if (_target is { } target)
                {
                    jsonWriter.WritePropertyName("target");
                    target.WriteExpressionJson(jsonWriter);
                }

                if (scale.HasValue)
                {
                    jsonWriter.WriteNumber("scale", scale.Value);
                }

                if (midpoint.HasValue)
                {
                    jsonWriter.WriteNumber("midpoint", midpoint.Value);
                }
            }
        }
    }
}
