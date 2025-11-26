using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldRangeDateTimeCondition(
    string payloadFieldName,
    DateTimeOffset? lessThan = null,
    DateTimeOffset? lessThanOrEqual = null,
    DateTimeOffset? greaterThan = null,
    DateTimeOffset? greaterThanOrEqual = null) : FilterConditionBase(payloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => PayloadIndexedFieldType.Datetime;

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("range");
        jsonWriter.WriteStartObject();

        if (lessThan is not null)
        {
            jsonWriter.WriteString("lt", lessThan.Value.ToString("u"));
        }

        if (lessThanOrEqual is not null)
        {
            jsonWriter.WriteString("lte", lessThanOrEqual.Value.ToString("u"));
        }

        if (greaterThan is not null)
        {
            jsonWriter.WriteString("gt", greaterThan.Value.ToString("u"));
        }

        if (greaterThanOrEqual is not null)
        {
            jsonWriter.WriteString("gte", greaterThanOrEqual.Value.ToString("u"));
        }

        jsonWriter.WriteEndObject();
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldRangeDateTimeCondition(this);
}
