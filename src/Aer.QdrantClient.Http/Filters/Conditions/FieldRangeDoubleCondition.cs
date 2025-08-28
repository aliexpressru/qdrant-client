using System.Text.Json;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldRangeDoubleCondition : FilterConditionBase
{
    private readonly double? _gt;
    private readonly double? _gte;
    private readonly double? _lt;
    private readonly double? _lte;

    protected internal override PayloadIndexedFieldType? PayloadFieldType => PayloadIndexedFieldType.Float;

    public FieldRangeDoubleCondition(
        string payloadFieldName,
        double? lt = null,
        double? lte = null,
        double? gt = null,
        double? gte = null) : base(payloadFieldName)
    {
        _lt = lt;
        _lte = lte;
        _gt = gt;
        _gte = gte;
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("range");
        jsonWriter.WriteStartObject();

        if (_lt is not null)
        {
            jsonWriter.WriteNumber("lt", _lt.Value);
        }

        if (_lte is not null)
        {
            jsonWriter.WriteNumber("lte", _lte.Value);
        }

        if (_gt is not null)
        {
            jsonWriter.WriteNumber("gt", _gt.Value);
        }

        if (_gte is not null)
        {
            jsonWriter.WriteNumber("gte", _gte.Value);
        }

        jsonWriter.WriteEndObject();
    }
}
