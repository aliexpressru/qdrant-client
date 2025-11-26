using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

/// <summary>
/// Initializes a new instance of the <see cref="FieldMatchExceptCondition{T}"/> class.
/// </summary>
/// <param name="payloadFieldName">The key to match.</param>
/// <param name="exceptValues">The values to match except against.</param>
internal sealed class FieldMatchExceptCondition<T>(string payloadFieldName, params T[] exceptValues) : FilterConditionBase(payloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType { get; } = GetPayloadFieldType<T>();

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        jsonWriter.WritePropertyName("match");
        jsonWriter.WriteStartObject();
        {
            jsonWriter.WritePropertyName("except");
            JsonSerializer.Serialize(jsonWriter, exceptValues, JsonSerializerConstants.DefaultSerializerOptions);
        }
        jsonWriter.WriteEndObject();
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldMatchExceptCondition(this);
}
