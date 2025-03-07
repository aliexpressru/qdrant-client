using Aer.QdrantClient.Http.Filters.Conditions;

namespace Aer.QdrantClient.Http.Filters.Optimization.Abstractions;

/// <summary>
/// A common interface for condition optimization visitors.
/// </summary>
internal interface IOptimizationVisitor
{
	public void Visit(FilterConditionBase condition);
	
	public void Visit<T>(FieldMatchAnyCondition<T> condition);
}
