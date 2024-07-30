using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Aer.QdrantClient.Http.Infrastructure.Helpers;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// The helper class for creating order by selectors with payload selector functions.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public static class OrderBySelector<TPayload>
{
    /// <summary>
    /// Creates an instance of the <see cref="OrderBySelector"/> class
    /// with the specified payload field name and default ascending order.
    /// </summary>
    /// <param name="payloadFieldSelectorExpression">The payload field to order by selector function.</param>
    /// <param name="startFrom">The starting value for the order by.</param>
    public static OrderBySelector Asc<TProperty>(
        Expression<Func<TPayload, TProperty>> payloadFieldSelectorExpression,
        OrderByStartFrom startFrom = null)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);
        return OrderBySelector.Asc(payloadFieldName, startFrom);
    }

    /// <summary>
    /// Creates an instance of the <see cref="OrderBySelector"/> class
    /// with the specified payload field name and descending order.
    /// </summary>
    /// <param name="payloadFieldSelectorExpression">The payload field to order by selector function.</param>
    /// <param name="startFrom">The starting value for the order by.</param>
    public static OrderBySelector Desc<TProperty>(
        Expression<Func<TPayload, TProperty>> payloadFieldSelectorExpression,
        OrderByStartFrom startFrom = null)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);
        return OrderBySelector.Desc(payloadFieldName, startFrom);
    }
}
