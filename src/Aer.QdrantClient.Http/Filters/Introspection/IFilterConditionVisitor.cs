using Aer.QdrantClient.Http.Filters.Conditions;
using Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;

namespace Aer.QdrantClient.Http.Filters.Introspection;

/// <summary>
/// A visitor interface for filter conditions.
/// </summary>
internal interface IFilterConditionVisitor
{
    /// <summary>
    /// Visits a filter group condition.
    /// </summary>
    /// <param name="condition">The condition to visit.</param>
    public void VisitFilterGroupCondition(FilterGroupCondition condition);

    public void VisitMinimumShouldCondition(MinimumShouldCondition condition);

    public void VisitMustCondition(MustCondition condition);

    public void VisitMustNotCondition(MustNotCondition condition);

    public void VisitNestedCondition(NestedCondition condition);

    public void VisitShouldCondition(ShouldCondition condition);

    public void VisitFieldInGeoBoundingBoxCondition(FieldInGeoBoundingBoxCondition condition);

    public void VisitFieldInGeoPolygonCondition(FieldInGeoPolygonCondition condition);

    public void VisitFieldInGeoRadiusCondition(FieldInGeoRadiusCondition condition);
    public void VisitFieldIsNullCondition(FieldIsNullCondition condition);

    public void VisitFieldIsNullOrEmptyCondition(FieldIsNullOrEmptyCondition condition);

    public void VisitFieldMatchAnyCondition<T>(FieldMatchAnyCondition<T> condition);

    public void VisitFieldMatchAnyConditionFast<T>(FieldMatchAnyConditionFast<T> condition);

    public void VisitFieldMatchCondition<T>(FieldMatchCondition<T> condition);

    public void VisitFieldMatchExceptCondition<T>(FieldMatchExceptCondition<T> condition);

    public void VisitFieldRangeDateTimeCondition(FieldRangeDateTimeCondition condition);

    public void VisitFieldRangeDoubleCondition(FieldRangeDoubleCondition condition);

    public void VisitFieldRangeIntCondition(FieldRangeIntCondition condition);

    public void VisitFieldValuesCountCondition(FieldValuesCountCondition condition);

    public void VisitHasAnyIdCondition(HasAnyIdCondition condition);

    public void VisitHasNamedVectorCondition(HasNamedVectorCondition condition);
}
