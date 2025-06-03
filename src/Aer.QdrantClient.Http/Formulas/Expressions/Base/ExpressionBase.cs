using System.Text.Json;

namespace Aer.QdrantClient.Http.Formulas.Expressions;

public abstract class ExpressionBase
{
	/// <summary>
	/// Write out the expression json to specified writer.
	/// </summary>
	/// <param name="jsonWriter">The json writer to write expression json to.</param>
	public abstract void WriteExpressionJson(Utf8JsonWriter jsonWriter);
}
