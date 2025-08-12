using System.Diagnostics.CodeAnalysis;
using Aer.QdrantClient.Http.Filters.Conditions;
using Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;
using Aer.QdrantClient.Http.Models.Primitives;

namespace Aer.QdrantClient.Http.Filters.Builders;

/// <summary>
/// Class for building filter condition instances.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API")]
[SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API")]
public static class Q
{
    /// <summary>
    /// Checks whether all the underlying conditions were satisfied.
    /// </summary>
    public static FilterConditionBase Must(params FilterConditionBase[] conditions)
        => new MustCondition(conditions);

    /// <summary>
    /// Checks whether all the underlying conditions were satisfied.
    /// </summary>
    public static FilterConditionBase Must(IEnumerable<FilterConditionBase> conditions)
        => new MustCondition(conditions);

    /// <summary>
    /// Checks whether all the underlying conditions were NOT satisfied.
    /// </summary>
    public static FilterConditionBase MustNot(params FilterConditionBase[] conditions)
        => new MustNotCondition(conditions);

    /// <summary>
    /// Checks whether all the underlying conditions were NOT satisfied.
    /// </summary>
    public static FilterConditionBase MustNot(IEnumerable<FilterConditionBase> conditions)
        => new MustNotCondition(conditions);

    /// <summary>
    /// Checks whether any of the underlying conditions was satisfied.
    /// </summary>
    public static FilterConditionBase Should(params FilterConditionBase[] conditions)
        => new ShouldCondition(conditions);

    /// <summary>
    /// Checks whether any of the underlying conditions was satisfied.
    /// </summary>
    public static FilterConditionBase Should(IEnumerable<FilterConditionBase> conditions)
        => new ShouldCondition(conditions);

    /// <summary>
    /// Checks whether the minimum number of the underlying conditions was satisfied.
    /// </summary>
    public static FilterConditionBase MinShould(int minCount, params FilterConditionBase[] conditions)
        => new MinimumShouldCondition(minCount, conditions);

    /// <summary>
    /// Checks whether the minimum number of the underlying conditions was satisfied.
    /// </summary>
    public static FilterConditionBase MinShould(int minCount, IEnumerable<FilterConditionBase> conditions)
        => new MinimumShouldCondition(minCount, conditions);

    /// <summary>
    /// Check if payload has a field with a given value.
    /// </summary>
    /// <typeparam name="T">Type of the value to match payload value against.</typeparam>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="value">Payload field value.</param>
    public static FilterConditionBase MatchValue<T>(string payloadFieldName, T value)
        => new FieldMatchCondition<T>(payloadFieldName, value, isSubstringMatch: false);

    /// <summary>
    /// Check if payload has a field with any of the given values.
    /// </summary>
    /// <typeparam name="T">Type of the value to match payload value against.</typeparam>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="anyValues">Payload field values to match with.</param>
    public static FilterConditionBase MatchAny<T>(string payloadFieldName, IEnumerable<T> anyValues)
        => new FieldMatchAnyCondition<T>(payloadFieldName, anyValues);

    /// <summary>
    /// Check if payload has a field with any of the given values.
    /// </summary>
    /// <typeparam name="T">Type of the value to match payload value against.</typeparam>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="anyValues">Payload field values to match with.</param>
    public static FilterConditionBase MatchAny<T>(string payloadFieldName, params T[] anyValues)
        => new FieldMatchAnyCondition<T>(payloadFieldName, anyValues);

    /// <summary>
    /// Check if payload has a field with any of the given values.
    /// This filter variant is optimised so the MatchAny is substituted with
    /// a bunch of MatchValue conditions combined with Should.
    /// </summary>
    /// <typeparam name="T">Type of the value to match payload value against.</typeparam>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="anyValues">Payload field values to match with.</param>
    public static FilterConditionBase MatchAnyFast<T>(string payloadFieldName, IEnumerable<T> anyValues)
        => new FieldMatchAnyConditionFast<T>(payloadFieldName, anyValues);

    /// <summary>
    /// Check if payload has a field with any of the given values.
    /// This filter variant is optimised so the MatchAny is substituted with
    /// a bunch of MatchValue conditions combined with Should.
    /// </summary>
    /// <typeparam name="T">Type of the value to match payload value against.</typeparam>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="anyValues">Payload field values to match with.</param>
    public static FilterConditionBase MatchAnyFast<T>(string payloadFieldName, params T[] anyValues)
        => new FieldMatchAnyConditionFast<T>(payloadFieldName, anyValues);

    /// <summary>
    /// Check if payload has a field that doesn't have any of the given values.
    /// </summary>
    /// <typeparam name="T">Type of the value to match payload value against.</typeparam>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="exceptValues">Payload field values to match except against.</param>
    public static FilterConditionBase MatchExceptValue<T>(string payloadFieldName, params T[] exceptValues)
        => new FieldMatchExceptCondition<T>(payloadFieldName, exceptValues);

    /// <summary>
    /// Check if payload has a text field which contains a given substring value.
    /// </summary>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="substringValue">Payload value to substring match against.</param>
    /// <param name="isPhraseMatch">If set to <c>true</c> use phrase matching for search. Requires phrase matching to be enabled for payload field index.</param>
    /// <remarks>
    /// Exact texts that will match the condition depend on full-text index configuration. Configuration is defined during the index creation and describe at full-text index.
    /// If there is no full-text index for the field, the condition will work as exact substring match.
    /// </remarks>
    public static FilterConditionBase MatchFulltext(
        string payloadFieldName,
        string substringValue,
        bool isPhraseMatch = false)
        => 
            new FieldMatchCondition<string>(
            payloadFieldName,
            substringValue,
            isSubstringMatch: true,
            isPhraseMatch: isPhraseMatch);

    /// <summary>
    /// Check if payload field value lies in a given range.
    /// If several values are stored, at least one of them should match the condition.
    /// </summary>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="lessThan">Value that payload value must be less than.</param>
    /// <param name="lessThanOrEqual">Value that payload value must be less than or equal.</param>
    /// <param name="greaterThan">Value that payload value must be greater than.</param>
    /// <param name="greaterThanOrEqual">Value that payload value must be greater than or equal.</param>
    public static FilterConditionBase BeInRange(
        string payloadFieldName,
        int? lessThan = null,
        int? lessThanOrEqual = null,
        int? greaterThan = null,
        int? greaterThanOrEqual = null)
        =>
            new FieldRangeIntCondition(
                payloadFieldName,
                lessThan,
                lessThanOrEqual,
                greaterThan,
                greaterThanOrEqual);

    /// <summary>
    /// Check if payload field value lies in a given range.
    /// If several values are stored, at least one of them should match the condition.
    /// </summary>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="lessThan">Value that payload value must be less than.</param>
    /// <param name="lessThanOrEqual">Value that payload value must be less than or equal.</param>
    /// <param name="greaterThan">Value that payload value must be greater than.</param>
    /// <param name="greaterThanOrEqual">Value that payload value must be greater than or equal.</param>
    public static FilterConditionBase BeInRange(
        string payloadFieldName,
        double? lessThan = null,
        double? lessThanOrEqual = null,
        double? greaterThan = null,
        double? greaterThanOrEqual = null)
        =>
            new FieldRangeDoubleCondition(
                payloadFieldName,
                lessThan,
                lessThanOrEqual,
                greaterThan,
                greaterThanOrEqual);

    /// <summary>
    /// Check if payload field value lies in a given range.
    /// If several values are stored, at least one of them should match the condition.
    /// </summary>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="lessThan">Value that payload value must be less than.</param>
    /// <param name="lessThanOrEqual">Value that payload value must be less than or equal.</param>
    /// <param name="greaterThan">Value that payload value must be greater than.</param>
    /// <param name="greaterThanOrEqual">Value that payload value must be greater than or equal.</param>
    public static FilterConditionBase BeInRange(
        string payloadFieldName,
        DateTime? lessThan = null,
        DateTime? lessThanOrEqual = null,
        DateTime? greaterThan = null,
        DateTime? greaterThanOrEqual = null)
        =>
            new FieldRangeDateTimeCondition(
                payloadFieldName,
                lessThan,
                lessThanOrEqual,
                greaterThan,
                greaterThanOrEqual);

    /// <summary>
    /// Check number of values of the payload against the range of possible integer values.
    /// </summary>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="lessThan">Count that payload values count must be less than.</param>
    /// <param name="lessThanOrEqual">Count that payload values count must be less than or equal.</param>
    /// <param name="greaterThan">Count that payload values count must be greater than.</param>
    /// <param name="greaterThanOrEqual">Count that payload values count must be greater than or equal.</param>
    /// <remarks>
    /// If value stored under the <paramref name="payloadFieldName"/> payload field is not an array - it is assumed
    /// that the amount of values is equals to 1.
    /// </remarks>
    public static FilterConditionBase HaveValuesCount(
        string payloadFieldName,
        int? lessThan = null,
        int? lessThanOrEqual = null,
        int? greaterThan = null,
        int? greaterThanOrEqual = null)
        =>
            new FieldValuesCountCondition(
                payloadFieldName,
                lessThan,
                lessThanOrEqual,
                greaterThan,
                greaterThanOrEqual);

    /// <summary>
    /// Check if the specified payload field either does not exist, or has <c>null</c> or empty array <c>[]</c> value.
    /// </summary>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    public static FilterConditionBase BeNullOrEmpty(string payloadFieldName) =>
        new FieldIsNullOrEmptyCondition(payloadFieldName);

    /// <summary>
    /// Check if the specified payload field has <c>null</c> value.
    /// </summary>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    public static FilterConditionBase BeNull(string payloadFieldName)
        => new FieldIsNullCondition(payloadFieldName);

    /// <summary>
    /// Select only the points with specified ids.
    /// </summary>
    /// <param name="ids">The ids of the points to select.</param>
    public static FilterConditionBase HaveAnyId(params PointId[] ids)
        => new HasAnyIdCondition(ids);

    /// <summary>
    /// Select only the points with specified ids.
    /// </summary>
    /// <param name="ids">The ids of the points to select.</param>
    public static FilterConditionBase HaveAnyId(IEnumerable<PointId> ids)
        => new HasAnyIdCondition(ids);

    /// <summary>
    /// Select only the points with specified ids.
    /// </summary>
    /// <param name="ids">The ids of the points to select.</param>
    public static FilterConditionBase HaveAnyId(params int[] ids)
        => new HasAnyIdCondition(ids);

    /// <summary>
    /// Select only the points with specified ids.
    /// </summary>
    /// <param name="ids">The ids of the points to select.</param>
    public static FilterConditionBase HaveAnyId(IEnumerable<int> ids)
        => new HasAnyIdCondition(ids);

    /// <summary>
    /// Select only the points with specified ids.
    /// </summary>
    /// <param name="ids">The ids of the points to select.</param>
    public static FilterConditionBase HaveAnyId(params Guid[] ids)
        => new HasAnyIdCondition(ids);

    /// <summary>
    /// Select only the points with specified ids.
    /// </summary>
    /// <param name="ids">The ids of the points to select.</param>
    public static FilterConditionBase HaveAnyId(IEnumerable<Guid> ids)
        => new HasAnyIdCondition(ids);

    /// <summary>
    /// Select only the points with specified ids.
    /// </summary>
    /// <param name="ids">The ids of the points to select.</param>
    public static FilterConditionBase HaveAnyId(params string[] ids)
        => new HasAnyIdCondition(ids);

    /// <summary>
    /// Select only the points with specified ids.
    /// </summary>
    /// <param name="ids">The ids of the points to select.</param>
    public static FilterConditionBase HaveAnyId(IEnumerable<string> ids)
        => new HasAnyIdCondition(ids);

    /// <summary>
    /// Check if points geolocation lies in a given area designated by a binding box.
    /// These conditions can only be applied to payloads that match the geo-data format.
    /// </summary>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="topLeftLongitude">Area bounding box top left longitude.</param>
    /// <param name="topLeftLatitude">Area bounding box top left latitude.</param>
    /// <param name="bottomRightLongitude">Area bounding box bottom right longitude.</param>
    /// <param name="bottomRightLatitude">Area bounding box bottom right latitude.</param>
    public static FilterConditionBase BeWithinGeoBoundingBox(
        string payloadFieldName,
        double topLeftLongitude,
        double topLeftLatitude,
        double bottomRightLongitude,
        double bottomRightLatitude)
        =>
            new FieldInGeoBoundingBoxCondition(
                payloadFieldName,
                topLeftLongitude,
                topLeftLatitude,
                bottomRightLongitude,
                bottomRightLatitude);

    /// <summary>
    /// Check if geo point is within an area with a given radius.
    /// These conditions can only be applied to payloads that match the geo-data format.
    /// </summary>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="centerLongitude">Area center longitude.</param>
    /// <param name="centerLatitude">Area center latitude.</param>
    /// <param name="radius">Radius of the area in meters.</param>
    public static FilterConditionBase BeWithinGeoRadius(
        string payloadFieldName,
        double centerLongitude,
        double centerLatitude,
        double radius)
        =>
            new FieldInGeoRadiusCondition(
                payloadFieldName,
                centerLongitude,
                centerLatitude,
                radius);

    /// <summary>
    /// Check if geo point is within an area defined by the given <paramref name="exteriorPolygonPoints"/> points
    /// and is not inside an area defied by the given <paramref name="interiorPolygonsPoints"/> points.
    /// A match is considered any point location inside or on the boundaries of the given polygon’s exterior but not inside any interiors.
    /// If several location values are stored for a point, then any of them matching will include that
    /// point as a candidate in the resultset.
    /// These conditions can only be applied to payloads that match the geo-data format.
    /// </summary>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="exteriorPolygonPoints">Points that define exterior polygon to search into.</param>
    /// <param name="interiorPolygonsPoints">Points that define interior polygons to exclude from search.</param>
    public static FilterConditionBase BeWithinGeoPolygon(
        string payloadFieldName,
        IEnumerable<GeoPoint> exteriorPolygonPoints,
        params IEnumerable<GeoPoint>[] interiorPolygonsPoints)
        =>
            new FieldInGeoPolygonCondition(
                payloadFieldName,
                exteriorPolygonPoints,
                interiorPolygonsPoints);

    /// <summary>
    /// Check if specified payload field value satisfies all the nested conditions.
    /// </summary>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="conditions">Conditions that must be satisfied.</param>
    public static FilterConditionBase SatisfyNested(
        string payloadFieldName,
        params FilterConditionBase[] conditions)
        =>
            new NestedCondition(
                payloadFieldName,
                conditions);

    /// <summary>
    /// Check if specified payload field value satisfies all the nested conditions.
    /// </summary>
    /// <param name="payloadFieldName">Name of the payload field to apply this filter to.</param>
    /// <param name="conditions">Conditions that must be satisfied.</param>
    public static FilterConditionBase SatisfyNested(
        string payloadFieldName,
        IEnumerable<FilterConditionBase> conditions)
        =>
            new NestedCondition(
                payloadFieldName,
                conditions);

    /// <summary>
    /// Check if point has a specified named vector.
    /// </summary>
    /// <param name="namedVectorName">Name of the named vector to check.</param>
    public static FilterConditionBase HasNamedVector(string namedVectorName)
        => new HasNamedVectorCondition(namedVectorName);

}
