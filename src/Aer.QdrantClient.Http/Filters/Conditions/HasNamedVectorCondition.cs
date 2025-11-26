using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class HasNamedVectorCondition(string namedVectorName) : FilterConditionBase(DiscardPayloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => null;

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WritePropertyName("has_vector");
        jsonWriter.WriteStringValue(namedVectorName);
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitHasNamedVectorCondition(this);
}
