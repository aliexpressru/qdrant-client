using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Aer.QdrantClient.Http.Filters.Conditions;
using Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Aer.QdrantClient.Http.Models.Primitives;
using static Aer.QdrantClient.Http.Filters.Conditions.FieldMatchTextCondition;

namespace Aer.QdrantClient.Http.Filters.Builders;

/// <summary>
/// Class for building filter condition instances with payload field names extracted from the specified typed payload.
/// </summary>
/// <typeparam name="TPayload">The payload type.</typeparam>
[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API")]
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
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
        return new FieldMatchCondition<TValue>(payloadFieldName, value);
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
    /// a bunch of MatchValue conditions combined with Should.
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
    /// a bunch of MatchValue conditions combined with Should.
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
    /// <param name="isPhraseMatch">If set to <c>true</c> use phrase matching for search. Requires phrase matching to be enabled for payload field index.</param>
    /// <remarks>
    /// Exact texts that will match the condition depend on full-text index configuration. Configuration is defined during the index creation and describe at full-text index.
    /// If there is no full-text index for the field, the condition will work as exact substring match.
    /// </remarks>
    [Obsolete($"Use {nameof(MatchText)} or {nameof(MatchTextPhrase)} or {nameof(MatchTextAny)} instead.")]
    public static FilterConditionBase MatchFulltext<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        string substringValue,
        bool isPhraseMatch = false)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return isPhraseMatch
            ? new FieldMatchTextCondition(payloadFieldName, substringValue, TextMatchType.Phrase)
            : new FieldMatchTextCondition(payloadFieldName, substringValue, TextMatchType.Default);
    }

    /// <summary>
    /// Check if payload has a text field which contains a given substring value.
    /// </summary>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="query">Value to substring match against.</param>
    /// <remarks>
    /// Exact texts that will match the condition depend on full-text index configuration. Configuration is defined during the index creation and describe at full-text index.
    /// If there is no full-text index for the field, the condition will work as exact substring match.
    /// </remarks>
    public static FilterConditionBase MatchText(Expression<Func<TPayload, string>> payloadFieldSelectorExpression, string query)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return new FieldMatchTextCondition(
            payloadFieldName,
            query,
            TextMatchType.Default);
    }

    /// <summary>
    /// Check if payload has a text field which contains a given substring phrase.
    /// </summary>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="query">Value to substring match against.</param>
    /// <remarks>
    /// Exact texts that will match the condition depend on full-text index configuration. Configuration is defined during the index creation and describe at full-text index.
    /// If there is no full-text index for the field, the condition will work as exact substring match.
    /// </remarks>
    public static FilterConditionBase MatchTextPhrase(Expression<Func<TPayload, string>> payloadFieldSelectorExpression, string query)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return new FieldMatchTextCondition(
            payloadFieldName,
            query,
            TextMatchType.Phrase);
    }

    /// <summary>
    /// Check if payload has a text field which contains a given any of the substring query terms.
    /// </summary>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="query">Value to substring match against.</param>
    /// <remarks>
    /// Exact texts that will match the condition depend on full-text index configuration. Configuration is defined during the index creation and describe at full-text index.
    /// If there is no full-text index for the field, the condition will work as exact substring match.
    /// </remarks>
    public static FilterConditionBase MatchTextAny(Expression<Func<TPayload, string>> payloadFieldSelectorExpression, string query)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return new FieldMatchTextCondition(
            payloadFieldName,
            query,
            TextMatchType.Any);
    }

    /// <summary>
    /// Check if payload field value lies in a given range.
    /// If several values are stored, at least one of them should match the condition.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="lessThan">Value that payload value must be less than.</param>
    /// <param name="lessThanOrEqual">Value that payload value must be less than or equal.</param>
    /// <param name="greaterThan">Value that payload value must be greater than.</param>
    /// <param name="greaterThanOrEqual">Value that payload value must be greater than or equal.</param>
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
    /// <param name="lessThan">Value that payload value must be less than.</param>
    /// <param name="lessThanOrEqual">Value that payload value must be less than or equal.</param>
    /// <param name="greaterThan">Value that payload value must be greater than.</param>
    /// <param name="greaterThanOrEqual">Value that payload value must be greater than or equal.</param>
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
    /// Check if payload field value lies in a given range.
    /// If several values are stored, at least one of them should match the condition.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="lessThan">Value that payload value must be less than.</param>
    /// <param name="lessThanOrEqual">Value that payload value must be less than or equal.</param>
    /// <param name="greaterThan">Value that payload value must be greater than.</param>
    /// <param name="greaterThanOrEqual">Value that payload value must be greater than or equal.</param>
    public static FilterConditionBase BeInRange<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        DateTime? lessThan = null,
        DateTime? lessThanOrEqual = null,
        DateTime? greaterThan = null,
        DateTime? greaterThanOrEqual = null)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return new FieldRangeDateTimeCondition(
            payloadFieldName,
            lessThan,
            lessThanOrEqual,
            greaterThan,
            greaterThanOrEqual);
    }

    /// <summary>
    /// Check number of values of the payload against the range of possible integer values.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="lessThan">Count that payload values count must be less than.</param>
    /// <param name="lessThanOrEqual">Count that payload values count must be less than or equal.</param>
    /// <param name="greaterThan">Count that payload values count must be greater than.</param>
    /// <param name="greaterThanOrEqual">Count that payload values count must be greater than or equal.</param>
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
    /// Check if points geolocation lies in a given area designated by a binding box.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="topLeftLongitude">Area bounding box top left longitude.</param>
    /// <param name="topLeftLatitude">Area bounding box top left latitude.</param>
    /// <param name="bottomRightLongitude">Area bounding box bottom right longitude.</param>
    /// <param name="bottomRightLatitude">Area bounding box bottom right latitude.</param>
    public static FilterConditionBase BeWithinGeoBoundingBox<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        double topLeftLongitude,
        double topLeftLatitude,
        double bottomRightLongitude,
        double bottomRightLatitude)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return new FieldInGeoBoundingBoxCondition(
            payloadFieldName,
            topLeftLongitude,
            topLeftLatitude,
            bottomRightLongitude,
            bottomRightLatitude);
    }

    /// <summary>
    /// Check if geo point is within an area with a given radius.
    /// </summary>
    /// <typeparam name="TField">The type of the payload field.</typeparam>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="centerLongitude">Area center longitude.</param>
    /// <param name="centerLatitude">Area center latitude.</param>
    /// <param name="radius">Radius of the area in meters.</param>
    public static FilterConditionBase BeWithinGeoRadius<TField>(
        Expression<Func<TPayload, TField>> payloadFieldSelectorExpression,
        double centerLongitude,
        double centerLatitude,
        double radius)
    {
        var payloadFieldName = ReflectionHelper.GetPayloadFieldName(payloadFieldSelectorExpression);

        return new FieldInGeoRadiusCondition(
            payloadFieldName,
            centerLongitude,
            centerLatitude,
            radius);
    }

    /// <summary>
    /// Check if geo point is within an area defined by the given <paramref name="exteriorPolygonPoints"/> points
    /// and is not inside an area defied by the given <paramref name="interiorPolygonsPoints"/> points.
    /// A match is considered any point location inside or on the boundaries of the given polygon’s exterior but not inside any interiors.
    /// If several location values are stored for a point, then any of them matching will include that
    /// point as a candidate in the result set.
    /// These conditions can only be applied to payloads that match the geo-data format.
    /// </summary>
    /// <param name="payloadFieldSelectorExpression">The payload field selector expression.</param>
    /// <param name="exteriorPolygonPoints">Points that define exterior polygon to search into.</param>
    /// <param name="interiorPolygonsPoints">Points that define interior polygons to exclude from search.</param>
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
