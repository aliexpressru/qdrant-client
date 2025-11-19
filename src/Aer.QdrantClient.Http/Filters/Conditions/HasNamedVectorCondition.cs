using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class HasNamedVectorCondition : FilterConditionBase
{
    private readonly string _namedVectorName;

    protected internal override PayloadIndexedFieldType? PayloadFieldType => null;

    public HasNamedVectorCondition(string namedVectorName) : base(DiscardPayloadFieldName)
    {
        _namedVectorName = namedVectorName;
    }

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WritePropertyName("has_vector");
        jsonWriter.WriteStringValue(_namedVectorName);
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitHasNamedVectorCondition(this);
}
