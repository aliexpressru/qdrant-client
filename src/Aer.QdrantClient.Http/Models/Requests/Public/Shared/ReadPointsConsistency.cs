using System.Diagnostics.CodeAnalysis;

namespace Aer.QdrantClient.Http.Models.Requests.Public.Shared;

/// <summary>
/// Represents the retrieve points request consistency setting.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public abstract class ReadPointsConsistency
{
    #region Nested classes

    /// <summary>
    /// Represents an integer consistency.
    /// </summary>
    internal sealed class IntegerReadConsistency : ReadPointsConsistency
    {
        /// <summary>
        /// Random requests to send and retrieve.
        /// </summary>
        public int ConsistencyValue { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="consistencyValue">Send N random requests and return points, which present on all of them.</param>
        internal IntegerReadConsistency(int consistencyValue)
        {
            ConsistencyValue = consistencyValue;
        }

        /// <inheritdoc/>
        public override string ToQueryParameterValue() => ConsistencyValue.ToString();
    }

    /// <summary>
    /// Represents a specific preset consistency.
    /// </summary>
    internal sealed class PresetReadConsistency : ReadPointsConsistency
    {
        /// <summary>
        /// The specific preset consistency type.
        /// </summary>
        public ConsistencyType Consistency { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="consistencyType">The type of the preset consistency.</param>
        internal PresetReadConsistency(ConsistencyType consistencyType)
        {
            Consistency = consistencyType;
        }

        /// <inheritdoc/>
        public override string ToQueryParameterValue() => Consistency.ToString().ToLowerInvariant();
    }

    #endregion

    /// <summary>
    /// The default consistency value.
    /// </summary>
    public static ReadPointsConsistency Default { get; } = new IntegerReadConsistency(1);

    /// <summary>
    /// Convert to get points request query parameter.
    /// </summary>
    public abstract string ToQueryParameterValue();

    /// <summary>
    /// Gets the <see cref="PresetReadConsistency"/> consistency type.
    /// </summary>
    /// <param name="consistencyType">The type of the preset consistency.</param>
    public static ReadPointsConsistency Preset(ConsistencyType consistencyType) =>
        new PresetReadConsistency(consistencyType);

    /// <summary>
    /// Gets the <see cref="IntegerReadConsistency"/> consistency type.
    /// </summary>
    /// <param name="consistencyValue">Return points that are present on specified number of nodes.</param>
    public static ReadPointsConsistency Integer(int consistencyValue) =>
        new IntegerReadConsistency(consistencyValue);
}
