namespace Aer.QdrantClient.Http.Filters.Optimization.Abstractions;

/// <summary>
/// An interface for all filter conditions that support optimization.
/// </summary>
internal interface IOptimizableCondition
{
	public void Accept(IOptimizationVisitor visitor);
}
