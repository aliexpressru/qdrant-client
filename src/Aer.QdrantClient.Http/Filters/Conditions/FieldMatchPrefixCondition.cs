using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

/// <summary>
/// Initializes a new instance of the <see cref="FieldMatchPrefixCondition"/> class.
/// </summary>
/// <param name="payloadFieldName">The key to match.</param>
/// <param name="query">The text value to match against.</param>
internal sealed class FieldMatchPrefixCondition(
    string payloadFieldName,
    string query) : FilterConditionBase(payloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => PayloadIndexedFieldType.Keyword;

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);

        using (jsonWriter.WriteObject("match"))
        {
            jsonWriter.WritePropertyName("prefix");
            jsonWriter.WriteStringValue(query);
        }
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldMatchPrefixCondition(this);
}
