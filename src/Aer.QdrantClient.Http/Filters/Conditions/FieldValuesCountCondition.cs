using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldValuesCountCondition(
    string payloadFieldName,
    int? lessThan,
    int? lessThanOrEqual,
    int? greaterThan,
    int? greaterThanOrEqual) : FilterConditionBase(payloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => null;

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);

        using (jsonWriter.WriteObject("values_count"))
        {
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
        }
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldValuesCountCondition(this);
}
