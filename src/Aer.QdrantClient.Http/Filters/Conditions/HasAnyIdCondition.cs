using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Models.Primitives;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal sealed class HasAnyIdCondition : FilterConditionBase
{
    private readonly IEnumerable<PointId> _pointIds;

    protected internal override PayloadIndexedFieldType? PayloadFieldType => null;

    public HasAnyIdCondition(IEnumerable<PointId> pointIds) : base(DiscardPayloadFieldName)
    {
        _pointIds = pointIds;
    }

    public HasAnyIdCondition(IEnumerable<int> integerPointIds) : base(DiscardPayloadFieldName)
    {
        _pointIds = integerPointIds.Select(PointId.Integer);
    }

    public HasAnyIdCondition(IEnumerable<Guid> guidPointIds) : base(DiscardPayloadFieldName)
    {
        _pointIds = guidPointIds.Select(PointId.Guid);
    }

    public HasAnyIdCondition(IEnumerable<string> stringPointIds) : base(DiscardPayloadFieldName)
    {
        _pointIds = stringPointIds.Select(PointId.Guid);
    }

    public override void WriteConditionJson(Utf8JsonWriter jsonWriter)
    {
        jsonWriter.WritePropertyName("has_id");
        jsonWriter.WriteStartArray();

        foreach (var pointId in _pointIds)
        {
            JsonSerializer.Serialize(jsonWriter, pointId.ObjectId, JsonSerializerConstants.DefaultSerializerOptions);
        }

        jsonWriter.WriteEndArray();
    }

    internal override void Accept(IFilterConditionVisitor visitor) => visitor.VisitHasAnyIdCondition(this);
}
