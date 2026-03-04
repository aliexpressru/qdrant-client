using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldRangeDoubleCondition(
    string payloadFieldName,
    double? lt = null,
    double? lte = null,
    double? gt = null,
    double? gte = null) : FilterConditionBase(payloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => PayloadIndexedFieldType.Float;

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);

        using (jsonWriter.WriteObject("range"))
        {
            if (lt is not null)
            {
                jsonWriter.WriteNumber("lt", lt.Value);
            }

            if (lte is not null)
            {
                jsonWriter.WriteNumber("lte", lte.Value);
            }

            if (gt is not null)
            {
                jsonWriter.WriteNumber("gt", gt.Value);
            }

            if (gte is not null)
            {
                jsonWriter.WriteNumber("gte", gte.Value);
            }
        }
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldRangeDoubleCondition(this);
}
