using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json;
using static Aer.QdrantClient.Http.Filters.Conditions.FieldMatchTextCondition;

namespace Aer.QdrantClient.Http.Filters.Conditions;

/// <summary>
/// Initializes a new instance of the <see cref="FieldMatchTextCondition"/> class.
/// </summary>
/// <param name="payloadFieldName">The key to match.</param>
/// <param name="query">The text value to match against.</param>
/// <param name="matchType">The type of teh text match.</param>
internal sealed class FieldMatchTextCondition(
    string payloadFieldName,
    string query,
    TextMatchType matchType = TextMatchType.Default) : FilterConditionBase(payloadFieldName)
{
    internal enum TextMatchType
    {
        // All terms must be present in the field (default).
        Default,

        // Specific phrase must be present in the field.
        Phrase,

        // Any term can be present in the field.
        Any
    }

    protected internal override PayloadIndexedFieldType? PayloadFieldType { get; } = PayloadIndexedFieldType.Keyword;

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("match");
        jsonWriter.WriteStartObject();
        {
            jsonWriter.WritePropertyName(
                matchType switch
                {
                    TextMatchType.Default => "text",
                    TextMatchType.Phrase => "phrase",
                    TextMatchType.Any => "text_any",
                    _ => throw new InvalidOperationException($"Match type {matchType} is not supported."),
                }
            );
            jsonWriter.WriteStringValue(query);
        }
        jsonWriter.WriteEndObject();
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldMatchTextCondition(this);
}
