using System.Text.Json;
using Aer.QdrantClient.Http.Infrastructure.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

/// <summary>
/// Represents the match for field that does not any of the given values.
/// </summary>
/// <typeparam name="T">The type of the payload field.</typeparam>
internal class FieldMatchExceptCondition<T> : FilterConditionBase
{
    private readonly T[] _exceptValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldMatchExceptCondition{T}"/> class.
    /// </summary>
    /// <param name="payloadFieldName">The key to match.</param>
    /// <param name="exceptValues">The values to match except against.</param>
    public FieldMatchExceptCondition(string payloadFieldName, params T[] exceptValues)
        : base(payloadFieldName)
    {
        _exceptValues = exceptValues;
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("match");
        jsonWriter.WriteStartObject();
        {
            jsonWriter.WritePropertyName("except");
            JsonSerializer.Serialize(jsonWriter, _exceptValues, JsonSerializerConstants.SerializerOptions);
        }
        jsonWriter.WriteEndObject();
    }
}
