using Aer.QdrantClient.Http.Filters.Conditions;

namespace Aer.QdrantClient.Http.Filters.Introspection;

/// <summary>
/// A visitor interface for filter conditions.
/// </summary>
public interface IFilterConditionVisitor
{
    /// <summary>
    /// Visits a filter group condition.
    /// </summary>
    /// <param name="condition">The condition to visit.</param>
    public void VisitFilterGroupCondition(FilterConditionBase condition);
}
