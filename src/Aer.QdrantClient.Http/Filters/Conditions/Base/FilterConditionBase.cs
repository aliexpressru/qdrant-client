using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Aer.QdrantClient.Http.Filters.Conditions.GroupConditions;
using Aer.QdrantClient.Http.Filters.Introspection;
using Aer.QdrantClient.Http.Models.Shared;

namespace Aer.QdrantClient.Http.Filters.Conditions;

/// <summary>
/// A base class for all filter conditions.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public abstract class FilterConditionBase
{
    /// <summary>
    /// This value indicates that we don't care about the payload property name in the filter condition.
    /// </summary>
    protected static readonly string DiscardPayloadFieldName = string.Empty;

    /// <summary>
    /// The payload filed to apply filter to.
    /// </summary>
    protected internal readonly string PayloadFieldName;

    /// <summary>
    /// The type of the payload field corresponding to the actual filter parameter.
    /// </summary>
    protected internal abstract PayloadIndexedFieldType? PayloadFieldType
    {
        get;
    }

    /// <summary>
    /// Contains optimized condition if available. If available, this condition will be used instead of the current one.
    /// </summary>
    protected internal FilterConditionBase OptimizedCondition
    {
        get; set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterConditionBase"/> class.
    /// </summary>
    /// <param name="payloadFieldName">The payload key to apply filter to.</param>
    protected FilterConditionBase(string payloadFieldName)
    {
        PayloadFieldName = payloadFieldName;
    }

    /// <summary>
    /// Combines two conditions into one <see cref="FilterGroupCondition"/> with conditions on one level.
    /// Useful for creating many-conditional filters avoiding using <c>QdrantFilter.Create</c> factory method.
    /// Note that equal conditions like <c>must</c> or <c>should</c> wil overwrite each other and only the last one will be added.
    /// </summary>
    /// <param name="left">The left condition to combine.</param>
    /// <param name="right">The right condition to combine.</param>
    public static FilterConditionBase operator +(FilterConditionBase left, FilterConditionBase right)
        => new FilterGroupCondition(left, right);

    /// <summary>
    /// Combines two conditions into one with conditions combined using <see cref="MustCondition"/>.
    /// </summary>
    /// <param name="left">The left condition to combine.</param>
    /// <param name="right">The right condition to combine.</param>
    public static FilterConditionBase operator &(FilterConditionBase left, FilterConditionBase right)
    {
        List<FilterConditionBase> conditions = [];

        // unwarp must conditions

        if (left is MustCondition lmc)
        {
            conditions.AddRange(lmc.Conditions);
        }
        else
        {
            conditions.Add(left);
        }

        if (right is MustCondition rmc)
        {
            conditions.AddRange(rmc.Conditions);
        }
        else
        {
            conditions.Add(right);
        }

        return new MustCondition(conditions);
    }

    /// <summary>
    /// Combines two conditions into one with conditions combined using <see cref="ShouldCondition"/>.
    /// </summary>
    /// <param name="left">The left condition to combine.</param>
    /// <param name="right">The right condition to combine.</param>
    public static FilterConditionBase operator |(FilterConditionBase left, FilterConditionBase right)
    {
        List<FilterConditionBase> conditions = [];

        // unwarp "should" conditions

        if (left is ShouldCondition lsc)
        {
            conditions.AddRange(lsc.Conditions);
        }
        else
        {
            conditions.Add(left);
        }

        if (right is ShouldCondition rsc)
        {
            conditions.AddRange(rsc.Conditions);
        }
        else
        {
            conditions.Add(right);
        }

        return new ShouldCondition(conditions);
    }

    /// <summary>
    /// Wraps condition in <see cref="MustNotCondition"/>. If the condition is <see cref="MustCondition"/>
    /// or <see cref="MustNotCondition"/> it gets transformed into an opposite condition.
    /// </summary>
    /// <param name="condition">The condition to negate.</param>
    public static FilterConditionBase operator !(FilterConditionBase condition)
    {
        if (condition is MustNotCondition mnc)
        {
            return new MustCondition(mnc.Conditions);
        }

        if (condition is MustCondition mc)
        {
            return new MustNotCondition(mc.Conditions);
        }

        return new MustNotCondition(condition);
    }

    /// <summary>
    /// Write out the condition json to specified writer.
    /// </summary>
    /// <param name="jsonWriter">The json writer to write filter json to.</param>
    internal abstract void WriteConditionJson(Utf8JsonWriter jsonWriter);

    /// <summary>
    /// Writes the condition json to specified writer, using optimized condition if available.
    /// </summary>
    /// <param name="jsonWriter">The json writer to write filter json to.</param>
    public void WriteJson(Utf8JsonWriter jsonWriter)
    {
        if (OptimizedCondition != null)
        {
            OptimizedCondition.WriteConditionJson(jsonWriter);
        }
        else
        {
            WriteConditionJson(jsonWriter);
        }
    }

    /// <summary>
    /// Writes the payload field to resulting filter json.
    /// </summary>
    /// <param name="jsonWriter">The json writer to write filter json to.</param>
    protected void WritePayloadFieldName(Utf8JsonWriter jsonWriter)
    {
        if (PayloadFieldName == DiscardPayloadFieldName)
        {
            return;
        }

        jsonWriter.WriteString("key", PayloadFieldName);
    }

    /// <summary>
    /// Gets the <see cref="PayloadIndexedFieldType"/> for the specified parameter type.
    /// </summary>
    /// <typeparam name="T">Type of the parameter to get <see cref="PayloadIndexedFieldType"/> for.</typeparam>
    protected static PayloadIndexedFieldType? GetPayloadFieldType<T>()
    {
        var type = typeof(T);
        var typeCode = Type.GetTypeCode(type);

        PayloadIndexedFieldType? payloadFieldType = typeCode switch
        {
            TypeCode.String => PayloadIndexedFieldType.Keyword,
            TypeCode.Int32 => PayloadIndexedFieldType.Integer,
            TypeCode.Int64 => PayloadIndexedFieldType.Integer,
            TypeCode.Single => PayloadIndexedFieldType.Float,
            TypeCode.Double => PayloadIndexedFieldType.Float,
            TypeCode.Boolean => PayloadIndexedFieldType.Keyword,

            // Guid will have an Object type code so we need to check for it explicitly.
            TypeCode.Object when type == typeof(Guid) => PayloadIndexedFieldType.Uuid,

            TypeCode.Char => PayloadIndexedFieldType.Keyword,
            TypeCode.SByte => PayloadIndexedFieldType.Integer,
            TypeCode.Byte => PayloadIndexedFieldType.Integer,
            TypeCode.Int16 => PayloadIndexedFieldType.Integer,
            TypeCode.UInt16 => PayloadIndexedFieldType.Integer,
            TypeCode.UInt32 => PayloadIndexedFieldType.Integer,
            TypeCode.UInt64 => PayloadIndexedFieldType.Integer,
            TypeCode.Decimal => PayloadIndexedFieldType.Float,
            TypeCode.DateTime => PayloadIndexedFieldType.Datetime,
            TypeCode.Object => null,
            TypeCode.DBNull => null,
            TypeCode.Empty => null,
            _ => throw new ArgumentOutOfRangeException($"Unknown type code '{typeCode}' for type {typeof(T).FullName}")
        };

        return payloadFieldType;
    }

    /// <summary>
    /// Accepts the specified visitor.
    /// </summary>
    /// <param name="visitor">The visitor to accept.</param>
    internal abstract void Accept(FilterConditionVisitor visitor);
}
