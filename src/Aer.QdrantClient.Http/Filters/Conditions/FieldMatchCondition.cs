using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldMatchCondition<T> : FilterConditionBase
{
    private readonly bool _isSubstringMatch;
    private readonly bool _isPhraseMatch;
    private readonly T _value;

    protected internal override PayloadIndexedFieldType? PayloadFieldType { get; } = GetPayloadFieldType<T>();

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldMatchCondition{T}"/> class.
    /// </summary>
    /// <param name="payloadFieldName">The key to match.</param>
    /// <param name="value">The value to match against.</param>
    /// <param name="isSubstringMatch">If set to <c>true</c> performs substring match on full-text indexed payload field.</param>
    /// <param name="isPhraseMatch">If set to <c>true</c> use phrase matching for search on full-text indexed payload field.</param>
    public FieldMatchCondition(string payloadFieldName, T value, bool isSubstringMatch = false, bool isPhraseMatch = false)
        : base(payloadFieldName)
    {
        _isSubstringMatch = isSubstringMatch;
        _isPhraseMatch = isPhraseMatch;
        _value = value;
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("match");
        jsonWriter.WriteStartObject();
        {
            jsonWriter.WritePropertyName(
                _isPhraseMatch
                    ? "phrase"
                    : _isSubstringMatch
                        ? "text"
                        : "value");

            JsonSerializer.Serialize(jsonWriter, _value, JsonSerializerConstants.DefaultSerializerOptions);
        }
        jsonWriter.WriteEndObject();
    }

    internal override void Accept(IFilterConditionVisitor visitor) => visitor.VisitFieldMatchCondition(this);
}
