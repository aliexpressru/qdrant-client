using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Infrastructure.Json;

namespace Aer.QdrantClient.Http.Infrastructure.Helpers;

/// <summary>
/// Helper class for working with reflection and expressions.
/// </summary>
internal static class ReflectionHelper
{
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
            is MemberExpression {Member: PropertyInfo} expressionBody)
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
    private static void CollectPropertyNamesFromCallChain(
        MemberExpression expression,
        List<string> propertyNamesCallChain)
    {
        if (expression.Expression is not MemberExpression {Member: PropertyInfo} memberExpression)
        {
            // means that expression higher in the call chain is either null or of some other type : recursion exit condition
            var jsonObjectPropertyName = ReflectPropertyName(expression.Member);

            propertyNamesCallChain.Add(jsonObjectPropertyName);
            return;
        }

        if (memberExpression is {Member: PropertyInfo})
        {
            // means that expression higher in the call chain is another property name call

            CollectPropertyNamesFromCallChain(memberExpression, propertyNamesCallChain);

            var jsonObjectPropertyName = ReflectPropertyName(expression.Member);

            propertyNamesCallChain.Add(jsonObjectPropertyName);
        }
    }

    private static string ReflectPropertyName(MemberInfo targetMember)
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

        return JsonSerializerConstants.NamingStrategy.ConvertName(
            propertyInfo.Name
        );
    }
}
