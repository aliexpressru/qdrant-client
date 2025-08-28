using System.Text.Json;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldValuesCountCondition : FilterConditionBase
{
    private readonly int? _lessThan;
    private readonly int? _lessThanOrEqual;
    private readonly int? _greaterThan;
    private readonly int? _greaterThanOrEqual;

    protected internal override PayloadIndexedFieldType? PayloadFieldType => null; 

    public FieldValuesCountCondition(
        string payloadFieldName,
        int? lessThan,
        int? lessThanOrEqual,
        int? greaterThan,
        int? greaterThanOrEqual) : base(payloadFieldName)
    {
        _lessThan = lessThan;
        _lessThanOrEqual = lessThanOrEqual;
        _greaterThan = greaterThan;
        _greaterThanOrEqual = greaterThanOrEqual;
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("values_count");
        jsonWriter.WriteStartObject();

        if (_lessThan is not null)
        {
            jsonWriter.WriteNumber("lt", _lessThan.Value);
        }

        if (_lessThanOrEqual is not null)
        {
            jsonWriter.WriteNumber("lte", _lessThanOrEqual.Value);
        }

        if (_greaterThan is not null)
        {
            jsonWriter.WriteNumber("gt", _greaterThan.Value);
        }

        if (_greaterThanOrEqual is not null)
        {
            jsonWriter.WriteNumber("gte", _greaterThanOrEqual.Value);
        }

        jsonWriter.WriteEndObject();
    }
}
