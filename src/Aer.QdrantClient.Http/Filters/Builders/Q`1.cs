using System.Linq.Expressions;
using Aer.QdrantClient.Http.Filters.Conditions;
using Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Primitives;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Filters.Builders;

/// <summary>
/// Class for buiding filter condition instances with payload field names extracted from the specified typed payload.
/// </summary>
/// <typeparam name="TPayload">The payload type.</typeparam>
public static class Q<TPayload>
{
    /// <summary>
    /// Gets payload field name from payload field selector expression.
    /// </summary>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    public static string GetPayloadFieldName<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);
        return payloadFieldName;
    }

    /// <summary>
    /// Check if point has a field with a given value.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <typeparam name="TValue">The type of the value to match payload field value against.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="value">Payload field value.</param>
    public static FilterConditionBase MatchValue<TField, TValue>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        TValue value)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);
        return new FieldMatchCondition<TValue>(payloadFieldName, value, isSubstringMatch: false);
    }

    /// <summary>
    /// Check if point has a field with any given value.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <typeparam name="TValue">The type of the value to match payload field value against.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="anyValues">Payload field values to match any.</param>
    public static FilterConditionBase MatchAny<TField, TValue>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        IEnumerable<TValue> anyValues)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);
        return new FieldMatchAnyCondition<TValue>(payloadFieldName, anyValues);
    }

    /// <summary>
    /// Check if point has a field with any given value.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <typeparam name="TValue">The type of the value to match payload field value against.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="anyValues">Payload field values to match any.</param>
    public static FilterConditionBase MatchAny<TField, TValue>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        params TValue[] anyValues)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);
        return new FieldMatchAnyCondition<TValue>(payloadFieldName, anyValues);
    }

    /// <summary>
    /// Check if point has a field with any given value.
    /// This filter variant is optimised so the MatchAny is substituted with
    /// a bunch of MatchVBalue conditions combined with Should.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <typeparam name="TValue">The type of the value to match payload field value against.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="anyValues">Payload field values to match any.</param>
    public static FilterConditionBase MatchAnyFast<TField, TValue>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        IEnumerable<TValue> anyValues)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);
        return new FieldMatchAnyConditionFast<TValue>(payloadFieldName, anyValues);
    }

    /// <summary>
    /// Check if point has a field with any given value.
    /// This filter variant is optimised so the MatchAny is substituted with
    /// a bunch of MatchVBalue conditions combined with Should.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <typeparam name="TValue">The type of the value to match payload field value against.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="anyValues">Payload field values to match any.</param>
    public static FilterConditionBase MatchAnyFast<TField, TValue>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        params TValue[] anyValues)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);
        return new FieldMatchAnyConditionFast<TValue>(payloadFieldName, anyValues);
    }

    /// <summary>
    /// Check if payload has a field that doesn't have any of the given values.
    /// </summary>
    /// <typeparam name="TField">Type of the value to match payload value against.</typeparam>
    /// <typeparam name="TValue">The type of the value to match payload field value against.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="exceptValues">Payload field values to match except against.</param>
    public static FilterConditionBase MatchExceptValue<TField, TValue>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        params TValue[] exceptValues)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);
        return new FieldMatchExceptCondition<TValue>(payloadFieldName, exceptValues);
    }

    /// <summary>
    /// Check if point has a text field which contains a given substring value.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="substringValue">Payload value to substring match against.</param>
    public static FilterConditionBase MatchSubstring<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        string substringValue)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);
        return new FieldMatchCondition<string>(payloadFieldName, substringValue, isSubstringMatch: true);
    }

    /// <summary>
    /// Check if payload field value lies in a given range.
    /// If several values are stored, at least one of them should match the condition.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="lessThan">Value that palyload value must be less than.</param>
    /// <param name="lessThanOrEqual">Value that palyload value must be less than or equal.</param>
    /// <param name="greaterThan">Value that palyload value must be greater than.</param>
    /// <param name="greaterThanOrEqual">Value that palyload value must be greater than or equal.</param>
    public static FilterConditionBase BeInRange<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        int? lessThan = null,
        int? lessThanOrEqual = null,
        int? greaterThan = null,
        int? greaterThanOrEqual = null)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return new FieldRangeIntCondition(
            payloadFieldName,
            lessThan,
            lessThanOrEqual,
            greaterThan,
            greaterThanOrEqual);
    }

    /// <summary>
    /// Check if payload field value lies in a given range.
    /// If several values are stored, at least one of them should match the condition.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="lessThan">Value that palyload value must be less than.</param>
    /// <param name="lessThanOrEqual">Value that palyload value must be less than or equal.</param>
    /// <param name="greaterThan">Value that palyload value must be greater than.</param>
    /// <param name="greaterThanOrEqual">Value that palyload value must be greater than or equal.</param>
    public static FilterConditionBase BeInRange<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        double? lessThan = null,
        double? lessThanOrEqual = null,
        double? greaterThan = null,
        double? greaterThanOrEqual = null)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return new FieldRangeDoubleCondition(
            payloadFieldName,
            lessThan,
            lessThanOrEqual,
            greaterThan,
            greaterThanOrEqual);
    }

    /// <summary>
    /// Check number of values of the payload agains the range of possible integer values.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="lessThan">Count that palyload values count must be less than.</param>
    /// <param name="lessThanOrEqual">Count that palyload values count must be less than or equal.</param>
    /// <param name="greaterThan">Count that palyload values count must be greater than.</param>
    /// <param name="greaterThanOrEqual">Count that palyload values count must be greater than or equal.</param>
    /// <remarks>
    /// If value stored under the <paramref name="payloadFieldSelectorExpression"/> is not an array - it is assumed
    /// that the amount of values is equals to 1.
    /// </remarks>
    public static FilterConditionBase HaveValuesCount<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        int? lessThan = null,
        int? lessThanOrEqual = null,
        int? greaterThan = null,
        int? greaterThanOrEqual = null)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);
        return new FieldValuesCountCondition(
            payloadFieldName,
            lessThan,
            lessThanOrEqual,
            greaterThan,
            greaterThanOrEqual);
    }

    /// <summary>
    /// Check if the specified payload field either does not exist, or has <c>null</c> or empty array <c>[]</c> value.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    public static FilterConditionBase BeNullOrEmpty<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);
        return new FieldIsNullOrEmptyCondition(payloadFieldName);
    }

    /// <summary>
    /// Check if the specified payload field has <c>null</c> value.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    public static FilterConditionBase BeNull<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);
        return new FieldIsNullCondition(payloadFieldName);
    }

    /// <summary>
    /// Check if points geo location lies in a given area designated by a binding box.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="topLeftLongtitude">Area bounding box top left longtitude.</param>
    /// <param name="topLeftLatitude">Area bounding box top left latitude.</param>
    /// <param name="bottomRightLongtitude">Area bounding box bottom right longtitude.</param>
    /// <param name="bottomRightLatitude">Area bounding box bottom right latitude.</param>
    public static FilterConditionBase BeWithinGeoBoundingBox<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        double topLeftLongtitude,
        double topLeftLatitude,
        double bottomRightLongtitude,
        double bottomRightLatitude)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return new FieldInGeoBoundingBoxCondition(
            payloadFieldName,
            topLeftLongtitude,
            topLeftLatitude,
            bottomRightLongtitude,
            bottomRightLatitude);
    }

    /// <summary>
    /// Check if geo point is within an area with a given radius.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="centerLongtitude">Area center longtitude.</param>
    /// <param name="centerLatitude">Area center latitude.</param>
    /// <param name="radius">Radius of the area in meters.</param>
    public static FilterConditionBase BeWithinGeoRadius<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        double centerLongtitude,
        double centerLatitude,
        double radius)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return new FieldInGeoRadiusCondition(
            payloadFieldName,
            centerLongtitude,
            centerLatitude,
            radius);
    }

    /// <summary>
    /// Check if geo point is within an area defined by the given <paramref name="exteriorPolygonPoints"/> points
    /// and is not inside an area defied by the given <paramref name="interiorPolygonsPoints"/> points.
    /// A match is considered any point location inside or on the boundaries of the given polygon’s exterior but not inside any interiors.
    /// If several location values are stored for a point, then any of them matching will include that
    /// point as a candidate in the resultset.
    /// These conditions can only be applied to payloads that match the geo-data format.
    /// </summary>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="exteriorPolygonPoints">Points that define exterior polygon to search into.</param>
    /// <param name="interiorPolygonsPoints">Points that define interior ploygons to exclude from search.</param>
    public static FilterConditionBase BeWithinGeoPolygon<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        IEnumerable<GeoPoint> exteriorPolygonPoints,
        params IEnumerable<GeoPoint>[] interiorPolygonsPoints)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return new FieldInGeoPolygonCondition(
            payloadFieldName,
            exteriorPolygonPoints,
            interiorPolygonsPoints);
    }

    /// <summary>
    /// Check if specified payload field value satisfies all the nested conditions.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="conditions">Conditions that must be satisfied.</param>
    public static FilterConditionBase SatisfyNested<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        params FilterConditionBase[] conditions
    )
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return new NestedCondition(
            payloadFieldName,
            conditions);
    }

    /// <summary>
    /// Check if specified payload field value satisfies all the nested conditions.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="conditions">Conditions that must be satisfied.</param>
    public static FilterConditionBase SatisfyNested<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        IEnumerable<FilterConditionBase> conditions
    )
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return new NestedCondition(
            payloadFieldName,
            conditions);
    }
}
