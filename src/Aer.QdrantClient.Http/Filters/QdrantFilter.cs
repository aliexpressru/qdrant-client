using System.Text;
using System.Text.Json;

using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Filters.Conditions;
using Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Filters;

/// <summary>
/// Represents a qdrant filter.
/// </summary>
public class QdrantFilter
{
    private readonly List<FilterConditionBase> _conditions = [];

    /// <summary>
    /// Returns an empty filter.
    /// </summary>
    public static QdrantFilter Empty { get; } = new();

    /// <summary>
    /// This ctor is for preventing builder from being created manually.
    /// </summary>
    protected QdrantFilter()
    { }

    /// <summary>
    /// Creates the qdrant filter instance from a single condition.
    /// </summary>
    public static QdrantFilter Create(FilterConditionBase singleCondition)
    {
        ArgumentNullException.ThrowIfNull(singleCondition);

        QdrantFilter ret = new();

        CheckTopLevelConditionValid(singleCondition);

        ret._conditions.Add(singleCondition);

        return ret;
    }

    /// <summary>
    /// Creates the qdrant filter instance from filter conditions.
    /// </summary>
    public static QdrantFilter Create(params FilterConditionBase[] conditions)
    {
        if (conditions is null or {Length: 0})
        {
            return Empty;
        }

        QdrantFilter ret = new();

        foreach (var condition in conditions)
        {
            CheckTopLevelConditionValid(condition);

            ret._conditions.Add(condition);
        }

        return ret;
    }

    /// <summary>
    /// Creates a qdrant filter from filter conditions.
    /// </summary>
    /// <param name="conditions">The filter conditions to create filter from.</param>
    public static QdrantFilter Create(List<FilterConditionBase> conditions)
    {
        if (conditions is null or { Count: 0 })
        {
            throw new ArgumentNullException(nameof(conditions));
        }

        var firstCondition = conditions[0];

        var filter = Create(firstCondition);

        if (conditions.Count == 1)
        {
            return filter;
        }

        // here conditions have length > 1

        foreach (var nextCondition in conditions.Skip(1))
        {
            filter += nextCondition;
        }

        return filter;
    }

    /// <summary>
    /// Adds the filter condition to this filter.
    /// </summary>
    /// <param name="filter">The filter to add condition to.</param>
    /// <param name="condition">The condition to add to the filter.</param>
    public static QdrantFilter operator +(QdrantFilter filter, FilterConditionBase condition)
    {
        CheckTopLevelConditionValid(condition);

        if (filter is { } existingFilter)
        {
            existingFilter._conditions.Add(condition);

            return filter;
        }

        return Create(condition);
    }

    /// <summary>
    /// Creates a qdrant filter from a single filter condition.
    /// </summary>
    /// <param name="singleCondition">The filter condition to create filter from.</param>
    public static implicit operator QdrantFilter(FilterConditionBase singleCondition)
        => Create(singleCondition);

    /// <summary>
    /// Creates a qdrant filter from filter conditions.
    /// </summary>
    /// <param name="conditions">The filter conditions to create filter from.</param>
    public static implicit operator QdrantFilter(FilterConditionBase[] conditions)
        => Create(conditions);

    /// <summary>
    /// Creates a qdrant filter from filter conditions.
    /// </summary>
    /// <param name="conditions">The filter conditions to create filter from.</param>
    public static implicit operator QdrantFilter(List<FilterConditionBase> conditions)
        => Create(conditions);

    /// <summary>
    /// Build qdrant filter string representation. For debug and testing purposes.
    /// </summary>
    /// <param name="isIndentFilterSyntax">Determines whether the resulting filter string should be indented. Default value <c>false</c>.</param>
    public string ToString(bool isIndentFilterSyntax)
    {
        if (_conditions is null or { Count: 0 })
        {
            return string.Empty;
        }

        using var stream = new MemoryStream();

        Utf8JsonWriter jsonWriter = new Utf8JsonWriter(
            stream,
            new JsonWriterOptions()
            {
                Indented = isIndentFilterSyntax,
                SkipValidation = true
            });

        WriteFilterJson(jsonWriter);

        jsonWriter.Flush();

        var builtFilter = Encoding.UTF8.GetString(stream.ToArray())
            .ReplaceLineEndings();

        return builtFilter;
    }

    /// <inheritdoc/>
    public override string ToString() => ToString(isIndentFilterSyntax: true);

    internal static void CheckTopLevelConditionValid(FilterConditionBase condition)
    {
        if (condition is not MustCondition
            && condition is not MustNotCondition
            && condition is not ShouldCondition
            && condition is not FilterGroupCondition)
        {
            throw new QdrantInvalidFilterException(
                $"Top-level filter condition must be one of [{nameof(MustCondition)}, {nameof(MustNotCondition)}, {nameof(ShouldCondition)}, {nameof(FilterGroupCondition)}], but found {condition.GetType().Name}");
        }
    }

    /// <summary>
    /// Write this filter as Json to output writer. For serialization purposes.
    /// </summary>
    internal void WriteFilterJson(Utf8JsonWriter jsonWriter)
    {
        if (_conditions.Count == 0)
        {
            jsonWriter.WriteNullValue();
            return;
        }

        jsonWriter.WriteStartObject();

        foreach (var condition in _conditions)
        {
            condition.WriteConditionJson(jsonWriter);
        }

        jsonWriter.WriteEndObject();
    }
}
