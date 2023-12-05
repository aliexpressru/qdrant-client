using System.Text.Json;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Http.Filters.Conditions;

internal class HasAnyIdCondition : FilterConditionBase
{
    private readonly IEnumerable<PointId> _pointIds;

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
            JsonSerializer.Serialize(jsonWriter, pointId.ToJson(), JsonSerializerConstants.SerializerOptions);
        }

        jsonWriter.WriteEndArray();
    }
}
