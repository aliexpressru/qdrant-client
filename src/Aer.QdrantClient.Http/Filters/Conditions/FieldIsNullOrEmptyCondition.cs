using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldIsNullOrEmptyCondition(string payloadFieldName) : FilterConditionBase(payloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => null;

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        using (jsonWriter.WriteObject("is_empty"))
        {
            WritePayloadFieldName(jsonWriter);
        }
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldIsNullOrEmptyCondition(this);
}
