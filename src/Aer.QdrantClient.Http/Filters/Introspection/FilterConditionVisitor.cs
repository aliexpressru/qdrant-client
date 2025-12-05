using Aer.QdrantClient.Http.Filters.Conditions;
using Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;

namespace Aer.QdrantClient.Http.Filters.Introspection;

internal abstract class FilterConditionVisitor
{
    public virtual void VisitFilterGroupCondition(FilterGroupCondition condition)
    { }

    public virtual void VisitMinimumShouldCondition(MinimumShouldCondition condition)
    { }

    public virtual void VisitMustCondition(MustCondition condition)
    { }

    public virtual void VisitMustNotCondition(MustNotCondition condition)
    { }

    public virtual void VisitNestedCondition(NestedCondition condition)
    { }

    public virtual void VisitShouldCondition(ShouldCondition condition)
    { }

    public virtual void VisitFieldInGeoBoundingBoxCondition(FieldInGeoBoundingBoxCondition condition)
    { }

    public virtual void VisitFieldInGeoPolygonCondition(FieldInGeoPolygonCondition condition)
    { }

    public virtual void VisitFieldInGeoRadiusCondition(FieldInGeoRadiusCondition condition)
    { }

    public virtual void VisitFieldIsNullCondition(FieldIsNullCondition condition)
    { }

    public virtual void VisitFieldIsNullOrEmptyCondition(FieldIsNullOrEmptyCondition condition)
    { }

    public virtual void VisitFieldMatchAnyCondition<T>(FieldMatchAnyCondition<T> condition)
    { }

    public virtual void VisitFieldMatchAnyConditionFast<T>(FieldMatchAnyConditionFast<T> condition)
    { }

    public virtual void VisitFieldMatchCondition<T>(FieldMatchCondition<T> condition)
    { }

    public virtual void VisitFieldMatchExceptCondition<T>(FieldMatchExceptCondition<T> condition)
    { }

    public virtual void VisitFieldRangeDateTimeCondition(FieldRangeDateTimeCondition condition)
    { }

    public virtual void VisitFieldRangeDoubleCondition(FieldRangeDoubleCondition condition)
    { }

    public virtual void VisitFieldRangeIntCondition(FieldRangeIntCondition condition)
    { }

    public virtual void VisitFieldValuesCountCondition(FieldValuesCountCondition condition)
    { }

    public virtual void VisitHasAnyIdCondition(HasAnyIdCondition condition)
    { }

    public virtual void VisitHasNamedVectorCondition(HasNamedVectorCondition condition)
    { }

    public virtual void VisitFieldMatchTextCondition(FieldMatchTextCondition fieldMatchTextCondition)
    { }
}
