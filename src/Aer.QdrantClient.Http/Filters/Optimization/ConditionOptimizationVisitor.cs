using Aer.QdrantClient.Http.Filters.Conditions;
using Aer.QdrantClient.Http.Filters.Optimization.Abstractions;

namespace Aer.QdrantClient.Http.Filters.Optimization;

internal class ConditionOptimizationVisitor : IOptimizationVisitor
{
	public void Visit(FilterConditionBase condition)
	{
		if (condition is IOptimizableCondition optimizableCondition)
		{ 
			optimizableCondition.Accept(this);
		}
	}

	public void Visit<T>(FieldMatchAnyCondition<T> condition)
	{
		return;
	}
}
