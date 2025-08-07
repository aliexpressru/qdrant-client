using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

/// <summary>
/// Represents a base class for filter conditions that have other conditions inside them.
/// </summary>
internal abstract class FilterGroupConditionBase : FilterConditionBase
{
    protected internal override PayloadIndexedFieldType? PayloadFieldType => null;

    protected internal readonly List<FilterConditionBase> Conditions = [];
    
    protected FilterGroupConditionBase(string payloadFieldName) : base(payloadFieldName)
    { }
}
