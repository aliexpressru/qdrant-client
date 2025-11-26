using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldRangeIntCondition(
    string payloadFieldName,
    int? lessThan = null,
    int? lessThanOrEqual = null,
    int? greaterThan = null,
    int? greaterThanOrEqual = null) : FilterConditionBase(payloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => PayloadIndexedFieldType.Integer;

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("range");
        jsonWriter.WriteStartObject();

        if (lessThan is not null)
        {
            jsonWriter.WriteNumber("lt", lessThan.Value);
        }

        if (lessThanOrEqual is not null)
        {
            jsonWriter.WriteNumber("lte", lessThanOrEqual.Value);
        }

        if (greaterThan is not null)
        {
            jsonWriter.WriteNumber("gt", greaterThan.Value);
        }

        if (greaterThanOrEqual is not null)
        {
            jsonWriter.WriteNumber("gte", greaterThanOrEqual.Value);
        }

        jsonWriter.WriteEndObject();
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldRangeIntCondition(this);
}
