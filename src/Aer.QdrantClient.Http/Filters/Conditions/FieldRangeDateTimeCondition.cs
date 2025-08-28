using System.Text.Json;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldRangeDateTimeCondition : FilterConditionBase
{
    private readonly DateTimeOffset? _lessThan;
    private readonly DateTimeOffset? _lessThanOrEqual;
    private readonly DateTimeOffset? _greaterThan;
    private readonly DateTimeOffset? _greaterThanOrEqual;

    protected internal override PayloadIndexedFieldType? PayloadFieldType => PayloadIndexedFieldType.Datetime;

    public FieldRangeDateTimeCondition(
        string payloadFieldName,
        DateTimeOffset? lessThan = null,
        DateTimeOffset? lessThanOrEqual = null,
        DateTimeOffset? greaterThan = null,
        DateTimeOffset? greaterThanOrEqual = null) : base(payloadFieldName)
    {
        _lessThan = lessThan;
        _lessThanOrEqual = lessThanOrEqual;
        _greaterThan = greaterThan;
        _greaterThanOrEqual = greaterThanOrEqual;
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("range");
        jsonWriter.WriteStartObject();

        if (_lessThan is not null)
        {
            jsonWriter.WriteString("lt", _lessThan.Value.ToString("u"));
        }

        if (_lessThanOrEqual is not null)
        {
            jsonWriter.WriteString("lte", _lessThanOrEqual.Value.ToString("u"));
        }

        if (_greaterThan is not null)
        {
            jsonWriter.WriteString("gt", _greaterThan.Value.ToString("u"));
        }

        if (_greaterThanOrEqual is not null)
        {
            jsonWriter.WriteString("gte", _greaterThanOrEqual.Value.ToString("u"));
        }

        jsonWriter.WriteEndObject();
    }
}
