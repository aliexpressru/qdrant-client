using Aer.QdrantClient.Http.Exceptions;
using Aer.QdrantClient.Http.Filters.Conditions;
using Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Filters.Optimization;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace Aer.QdrantClient.Http.Filters;

/// <summary>
/// Represents a qdrant filter.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public sealed class QdrantFilter
{
#if NET9_0_OR_GREATER
    [FeatureSwitchDefinition("Aer.QdrantClient.OptimizeFilterConditions")]
    internal static bool IsFilterOptimizationEnabled =>
        AppContext.TryGetSwitch("Aer.QdrantClient.OptimizeFilterConditions", out bool isEnabled)
        && isEnabled;
#endif

    private readonly List<FilterConditionBase> _conditions = [];

    /// <summary>
    /// Returns an empty filter.
    /// </summary>
    public static QdrantFilter Empty { get; } = new();

    /// <summary>
    /// Returns <c>true</c> if this filter is empty (i.e. does not have any conditions and was not
    /// created from a raw filter string), otherwise <c>false</c>.
    /// </summary>
    public bool IsEmpty => _conditions.Count == 0 && string.IsNullOrWhiteSpace(RawFilterString);

    /// <summary>
    /// Gets the raw filter string if this filter was created from a raw filter string.
    /// </summary>
    public string RawFilterString { get; private set; }

    /// <summary>
    /// Gets the payload filed names used in all of this filter conditions along with their inferred
    /// types. Is this filter was constructed with raw filter string - returns an empty collection.
    /// </summary>
    public IReadOnlyCollection<FieldNameType> GetPayloadFieldsWithTypes()
    {
        if (IsEmpty
            || !string.IsNullOrEmpty(RawFilterString))
        {
            return [];
        }

        var payloadPropertyNames = new HashSet<FieldNameType>();

        foreach (var condition in _conditions)
        {
            GetPayloadFieldNameTypesInternal(condition, payloadPropertyNames);
        }

        return payloadPropertyNames;
    }

    /// <summary>
    /// This ctor is for preventing builder from being created manually.
    /// </summary>
    private QdrantFilter()
    {
    }

    /// <summary>
    /// Creates the qdrant filter instance from a single condition.
    /// </summary>
    public static QdrantFilter Create(FilterConditionBase singleCondition)
    {
        if (singleCondition is null)
        {
            throw new ArgumentNullException(nameof(singleCondition));
        }

        QdrantFilter ret = new();

        var isConditionGroup = CheckTopLevelConditionIsGroup(singleCondition);

        ret._conditions.Add(isConditionGroup ? singleCondition : new MustCondition(singleCondition));

        return ret;
    }

    /// <summary>
    /// Creates the qdrant filter instance from filter conditions.
    /// </summary>
    public static QdrantFilter Create(params FilterConditionBase[] conditions)
    {
        if (conditions is null or { Length: 0 })
        {
            return Empty;
        }

        QdrantFilter ret = new();

        foreach (var condition in conditions)
        {
            var isConditionGroup = CheckTopLevelConditionIsGroup(condition);

            ret._conditions.Add(isConditionGroup ? condition : new MustCondition(condition));
        }

        return ret;
    }

    /// <summary>
    /// Creates a qdrant filter instance from filter conditions.
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
    /// Creates the qdrant filter instance directly from a filter string.
    /// </summary>
    public static QdrantFilter Create(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            throw new ArgumentNullException(nameof(filter));
        }

        QdrantFilter ret = new()
        {
            RawFilterString = filter
        };

        return ret;
    }

    /// <summary>
    /// Adds all filter conditions from specified filter to this filter.
    /// </summary>
    /// <param name="target">The filter to add conditions to.</param>
    /// <param name="source">The filter to add conditions from.</param>
    public static QdrantFilter operator +(QdrantFilter target, QdrantFilter source)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (source is null or { IsEmpty: true })
        {
            return target;
        }

        if (!string.IsNullOrWhiteSpace(target.RawFilterString))
        {
            throw new QdrantFilterModificationForbiddenException(target.RawFilterString);
        }

        if (!string.IsNullOrWhiteSpace(source.RawFilterString))
        {
            throw new QdrantFilterModificationForbiddenException(source.RawFilterString);
        }

        target._conditions.AddRange(source._conditions);

        return target;
    }

    /// <summary>
    /// Adds the filter condition to this filter.
    /// </summary>
    /// <param name="filter">The filter to add condition to.</param>
    /// <param name="condition">The condition to add to the filter.</param>
    public static QdrantFilter operator +(QdrantFilter filter, FilterConditionBase condition)
    {
        if (!string.IsNullOrWhiteSpace(filter?.RawFilterString))
        {
            throw new QdrantFilterModificationForbiddenException(filter.RawFilterString);
        }

        var isConditionGroup = CheckTopLevelConditionIsGroup(condition);

        if (filter is { } existingFilter)
        {
            existingFilter._conditions.Add(isConditionGroup ? condition : new MustCondition(condition));

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
    /// <param name="isIndentFilterSyntax">
    /// Determines whether the resulting filter string should be indented. Default value <c>false</c>.
    /// </param>
    public string ToString(bool isIndentFilterSyntax)
    {
        if (!string.IsNullOrWhiteSpace(RawFilterString))
        {
            return RawFilterString;
        }

        if (_conditions is null or { Count: 0 })
        {
            return string.Empty;
        }

        using var stream = new MemoryStream();

        Utf8JsonWriter jsonWriter = new(
            stream,
            new JsonWriterOptions()
            {
                Indented = isIndentFilterSyntax,
                SkipValidation = true
            });

        WriteFilterJson(jsonWriter);

        jsonWriter.Flush();

        var builtFilter = Encoding.UTF8.GetString(stream.ToArray());

        return builtFilter;
    }

    /// <inheritdoc/>
    public override string ToString() => ToString(isIndentFilterSyntax: true);

    internal static bool CheckTopLevelConditionIsGroup(FilterConditionBase condition)
        =>
            condition is MustCondition
                or MustNotCondition
                or ShouldCondition
                or FilterGroupCondition
                or MinimumShouldCondition;

    /// <summary>
    /// Write this filter as Json to output writer. For serialization purposes.
    /// </summary>
    internal void WriteFilterJson(Utf8JsonWriter jsonWriter)
    {
        if (!string.IsNullOrWhiteSpace(RawFilterString))
        {
            jsonWriter.WriteRawValue(RawFilterString);
            return;
        }

        if (_conditions.Count == 0)
        {
            jsonWriter.WriteNullValue();
            return;
        }

#if NET9_0_OR_GREATER
        Optimize(_conditions);
#endif

        jsonWriter.WriteStartObject();

        foreach (var condition in _conditions)
        {
            condition.WriteJson(jsonWriter);
        }

        jsonWriter.WriteEndObject();
    }

    /// <summary>
    /// Analyzes the existing filter conditions and attempts to rewrite them in more optimal way.
    /// </summary>
    internal static void Optimize(List<FilterConditionBase> conditionsToOptimize)
    {
#if NET9_0_OR_GREATER
        if (IsFilterOptimizationEnabled)
        {
            foreach (var condition in conditionsToOptimize)
            {
                condition.Accept(ConditionOptimizerVisitor.Instance);
            }
        }
#endif
    }

    private void GetPayloadFieldNameTypesInternal(FilterConditionBase condition, HashSet<FieldNameType> payloadFiledNameTypes)
    {
        if (IsEmpty || !string.IsNullOrEmpty(RawFilterString))
        {
            return;
        }

        if (condition is FilterGroupConditionBase filterGroupCondition)
        {
            foreach (var conditionInGroup in filterGroupCondition.Conditions)
            {
                GetPayloadFieldNameTypesInternal(conditionInGroup, payloadFiledNameTypes);
            }
        }
        else
        {
            // Means this is a leaf condition, not a group
            payloadFiledNameTypes.Add(new(condition.PayloadFieldName, condition.PayloadFieldType));
        }
    }
}
