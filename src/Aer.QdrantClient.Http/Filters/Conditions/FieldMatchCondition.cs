using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Models.Shared;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters.Conditions;

/// <summary>
/// Initializes a new instance of the <see cref="FieldMatchCondition{T}"/> class.
/// </summary>
/// <param name="payloadFieldName">The key to match.</param>
/// <param name="value">The value to match against.</param>
internal sealed class FieldMatchCondition<T>(string payloadFieldName, T value) : FilterConditionBase(payloadFieldName)
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType { get; } = GetPayloadFieldType<T>();

    internal override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        WritePayloadFieldName(jsonWriter);
        using (jsonWriter.WriteObject("match"))
        {
            jsonWriter.WritePropertyName("value");

            JsonSerializer.Serialize(jsonWriter, value, JsonSerializerConstants.DefaultSerializerOptions);
        }
    }

    internal override void Accept(FilterConditionVisitor visitor) => visitor.VisitFieldMatchCondition(this);
}
