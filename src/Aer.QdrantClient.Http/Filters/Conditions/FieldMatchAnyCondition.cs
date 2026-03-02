using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldMatchAnyCondition<T>(string payloadFieldName, IEnumerable<T> matchAnyValuesToMatch) : FilterConditionBase(payloadFieldName)
{
    internal readonly IEnumerable<T> _anyValuesToMatch = matchAnyValuesToMatch;

    protected internal override PayloadIndexedFieldType? PayloadFieldType { get; } = GetPayloadFieldType<T>();

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);

        using (jsonWriter.WriteObject("match"))
        {
            jsonWriter.WritePropertyName("any");

            JsonSerializer.Serialize(jsonWriter, _anyValuesToMatch, JsonSerializerConstants.DefaultSerializerOptions);
        }
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldMatchAnyCondition(this);
}
