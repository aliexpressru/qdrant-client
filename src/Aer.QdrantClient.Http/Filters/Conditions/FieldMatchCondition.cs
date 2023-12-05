using System.Text.Json;
using Aer.QdrantClient.Http.Infrastructure.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

/// <summary>
/// Represents the field match .
/// </summary>
/// <typeparam name="T">The type of the payload field.</typeparam>
internal class FieldMatchCondition<T> : FilterConditionBase
{
    private readonly bool _isSubstringMatch;
    private readonly T _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldMatchCondition{T}"/> class.
    /// </summary>
    /// <param name="payloadFieldName">The key to match.</param>
    /// <param name="value">The value to match against.</param>
    /// <param name="isSubstringMatch">Is set to <c>true</c> performs substring match on full-text indexed payload field.</param>
    public FieldMatchCondition(string payloadFieldName, T value, bool isSubstringMatch = false)
        : base(payloadFieldName)
    {
        _isSubstringMatch = isSubstringMatch;
        _value = value;
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("match");
        jsonWriter.WriteStartObject();
        {
            jsonWriter.WritePropertyName(
                _isSubstringMatch
                    ? "text"
                    : "value");

            JsonSerializer.Serialize(jsonWriter, _value, JsonSerializerConstants.SerializerOptions);
        }
        jsonWriter.WriteEndObject();
    }
}
