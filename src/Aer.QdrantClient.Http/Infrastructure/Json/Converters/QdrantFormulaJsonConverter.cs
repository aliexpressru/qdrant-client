using System.Text.Json;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Formulas;

namespace Aer.QdrantClient.Http.Infrastructure.Json.Converters;

internal sealed class QdrantFormulaJsonConverter : JsonConverter<QdrantFormula>
{
    public override QdrantFormula Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Reading {nameof(QdrantFormula)} instances is not supported");
    }

    public override void Write(Utf8JsonWriter writer, QdrantFormula value, JsonSerializerOptions options)
    {
        value.WriteFormulaJson(writer);
    }
}
