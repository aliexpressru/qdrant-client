using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

/// <summary>
/// Initializes a new instance of the <see cref="FieldMatchCondition{T}"/> class.
/// </summary>
/// <param name="payloadFieldName">The key to match.</param>
/// <param name="value">The value to match against.</param>
/// <param name="isSubstringMatch">If set to <c>true</c> performs substring match on full-text indexed payload field.</param>
/// <param name="isPhraseMatch">If set to <c>true</c> use phrase matching for search on full-text indexed payload field.</param>
internal sealed class FieldMatchCondition<T>(
    string payloadFieldName,
    T value,
    bool isSubstringMatch = false,
    bool isPhraseMatch = false) : FilterConditionBase(payloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType { get; } = GetPayloadFieldType<T>();

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("match");
        jsonWriter.WriteStartObject();
        {
            jsonWriter.WritePropertyName(
                isPhraseMatch
                    ? "phrase"
                    : isSubstringMatch
                        ? "text"
                        : "value");

            JsonSerializer.Serialize(jsonWriter, value, JsonSerializerConstants.DefaultSerializerOptions);
        }
        jsonWriter.WriteEndObject();
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldMatchCondition(this);
}
