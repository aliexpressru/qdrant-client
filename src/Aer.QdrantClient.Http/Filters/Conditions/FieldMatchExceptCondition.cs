using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class FieldMatchExceptCondition<T> : FilterConditionBase
{
    private readonly T[] _exceptValues;

    protected internal override PayloadIndexedFieldType? PayloadFieldType { get; } = GetPayloadFieldType<T>();

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
            JsonSerializer.Serialize(jsonWriter, _exceptValues, JsonSerializerConstants.DefaultSerializerOptions);
        }
        jsonWriter.WriteEndObject();
    }

    internal override void Accept(IFilterConditionVisitor visitor) => visitor.VisitFieldMatchExceptCondition(this);
}
