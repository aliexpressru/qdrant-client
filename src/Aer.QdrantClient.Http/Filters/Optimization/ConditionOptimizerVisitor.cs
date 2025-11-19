using Aer.QdrantClient.Http.Filters.Conditions;
using Aer.QdrantClient.Http.Filters.Introspection;

namespace Aer.QdrantClient.Http.Filters.Optimization;

internal sealed class ConditionOptimizerVisitor : FilterConditionVisitor
{
    public static readonly ConditionOptimizerVisitor Instance = new ConditionOptimizerVisitor();

    public override void VisitFieldMatchAnyCondition<T>(FieldMatchAnyCondition<T> condition)
    {
#if NET9_0_OR_GREATER
        if (condition._anyValuesToMatch.TryGetNonEnumeratedCount(out var count) && count == 1)
        {
            // Optimize the condition
            condition.OptimizedCondition = new FieldMatchCondition<T>(condition.PayloadFieldName, condition._anyValuesToMatch.First());
        }
#endif
    }
}
