using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Infrastructure.Json;
using Polly;

namespace Aer.QdrantClient.Http.Infrastructure.Helpers;

/// <summary>
/// Helper class for working with reflection and expressions.
/// </summary>
internal static class ReflectionHelper
{
    private static readonly Type _collectionType = typeof(ICollection);

    private class MethodCallVisitor : ExpressionVisitor
    {
        public List<MethodCallExpression> MethodCalls { get; } = [];

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            MethodCalls.Add(node);
            return base.VisitMethodCall(node);
        }
    }

    /// <summary>
    /// Checks whether the retry strategy is configured in the resilience pipeline builder action.
    /// </summary>
    /// <param name="actionToInspect">The resilience configuration action to inspect.</param>
    public static bool CheckRetryStrategyConfigured(
        Expression<Action<ResiliencePipelineBuilder<HttpResponseMessage>>> actionToInspect)
    {
        var visitor = new MethodCallVisitor();
        visitor.Visit(actionToInspect);

        foreach (var methodCall in visitor.MethodCalls)
        {
            if (methodCall.Method.Name.Contains(
                nameof(RetryResiliencePipelineBuilderExtensions.AddRetry)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the JSON property name for the property specified in selector expression.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload to get property from.</typeparam>
    /// <typeparam name="TProperty">The type of the property to get.</typeparam>
    /// <param name="payloadMemberSelectorExpression">The property selector expression.</param>
    public static string GetPayloadFieldName<TPayload, TProperty>(
        Expression<Func<TPayload, TProperty>> payloadMemberSelectorExpression)
    {
        if (payloadMemberSelectorExpression.Body
            is MemberExpression { Member: PropertyInfo } expressionBody)
        {
            var namesCallChain = new List<string>();

            CollectPropertyNamesFromCallChain(expressionBody, namesCallChain);

            if (namesCallChain.Count == 1)
            {
                return namesCallChain[0];
            }

            var compoundJsonObjectPropertyName = string.Join(".", namesCallChain);

            return compoundJsonObjectPropertyName;
        }

        throw new QdrantInvalidPayloadFieldSelectorException(payloadMemberSelectorExpression.ToString());
    }

    /// <summary>
    /// Collects the property names from the expression call chain.
    /// </summary>
    /// <param name="expression">The expression to collect property names from.</param>
    /// <param name="propertyNamesCallChain">The output names in calling order from first to last.</param>
    /// <param name="isIndexer">
    /// If set to <c>true</c> means that the <paramref name="expression"/> is an array indexer.
    /// If it comes last in the call chain then it should have a `[]` added to the name.
    /// </param>
    private static void CollectPropertyNamesFromCallChain(
        MemberExpression expression,
        List<string> propertyNamesCallChain,
        bool isIndexer = false)
    {
        switch (expression.Expression)
        {
            case MemberExpression { Member: PropertyInfo } memberExpression:
            {
                // means that expression higher in the call chain is another property name call

                CollectPropertyNamesFromCallChain(memberExpression, propertyNamesCallChain);

                var jsonObjectPropertyName = ReflectMemberName(expression.Member);

                propertyNamesCallChain.Add(jsonObjectPropertyName);

                break;
            }

            case BinaryExpression { NodeType: ExpressionType.ArrayIndex } arrayCallExpression:
            {
                // means that expression higher in the call chain is another property name call with indexer access

                CollectPropertyNamesFromCallChain((MemberExpression)arrayCallExpression.Left, propertyNamesCallChain, isIndexer: true);

                var jsonObjectPropertyName = ReflectMemberName(expression.Member, isIndexer);

                propertyNamesCallChain.Add(jsonObjectPropertyName);

                break;
            }

            default:
            {
                // means that expression higher in the call chain is either null or of some other type : recursion exit condition
                var jsonObjectPropertyName = ReflectMemberName(expression.Member, shouldAddArrayBrackets: isIndexer);

                propertyNamesCallChain.Add(jsonObjectPropertyName);
                return;
            }
        }
    }

    private static string ReflectMemberName(MemberInfo targetMember, bool shouldAddArrayBrackets = false)
    {
        if (targetMember is not PropertyInfo propertyInfo)
        {
            throw new InvalidOperationException("Trying to get property name from non-property member");
        }

        var customPropertyJsonNameAttribute = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();

        if (customPropertyJsonNameAttribute is not null
            && !string.IsNullOrEmpty(customPropertyJsonNameAttribute.Name))
        {
            // means that JsonPropertyAttribute is set and its PropertyName is set
            return JsonSerializerConstants.NamingStrategy.ConvertName(
                customPropertyJsonNameAttribute.Name
            );
        }

        var reflectedJsonName = JsonSerializerConstants.NamingStrategy.ConvertName(
            propertyInfo.Name
        );

        if (shouldAddArrayBrackets &&
            (propertyInfo.PropertyType.IsArray
                || _collectionType.IsAssignableFrom(propertyInfo.PropertyType)))
        {
            // means type is either an array or a collection - (array in json), we need to add [] to the property name
            reflectedJsonName += "[]";
        }

        return reflectedJsonName;
    }
}
