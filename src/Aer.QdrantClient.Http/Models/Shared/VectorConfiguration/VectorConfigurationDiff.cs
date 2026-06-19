using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a one vector or multiple named vectors collection configuration diff for partial collection updates.
/// </summary>
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public abstract class VectorConfigurationDiff
{
    #region Nested classes

    /// <summary>
    /// Represents a single vector configuration diff.
    /// </summary>
    public sealed class SingleVectorConfigurationDiff : VectorConfigurationDiff
    {
        /// <summary>
        /// Custom params for HNSW index. If none - values from collection configuration are used.
        /// </summary>
        public HnswConfigurationDiff HnswConfig { get; set; }

        /// <summary>
        /// Custom params for quantization. If none - values from collection configuration are used.
        /// </summary>
        [JsonConverter(typeof(QuantizationConfigurationDiffJsonConverter))]
        public QuantizationConfigurationDiff QuantizationConfig { get; set; }

        /// <summary>
        /// If <c>true</c>, vectors are served from disk, improving RAM usage at the cost of latency.
        /// </summary>
        public bool? OnDisk { get; set; }
    }

    /// <summary>
    /// Represents a configuration diff for collection with named vectors.
    /// </summary>
    public sealed class NamedVectorsConfigurationDiff : VectorConfigurationDiff
    {
        /// <summary>
        /// The named vectors configuration diffs.
        /// </summary>
        public required Dictionary<string, SingleVectorConfigurationDiff> NamedVectors { init; get; }

        [JsonConstructor]
        internal NamedVectorsConfigurationDiff()
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="VectorConfigurationDiff.NamedVectorsConfigurationDiff"/> with specified named vectors configuration diffs.
        /// </summary>
        /// <param name="namedVectorsConfigurationDiff">The named vectors configuration diffs.</param>
        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public NamedVectorsConfigurationDiff(Dictionary<string, SingleVectorConfigurationDiff> namedVectorsConfigurationDiff)
        {
            NamedVectors = namedVectorsConfigurationDiff;
        }
    }

    #endregion

    /// <summary>
    /// <c>true</c> if this instance describes multiple vectors configuration diff.
    /// <c>false</c> if this instance describes single vector configuration diff.
    /// </summary>
    public bool IsMultipleVectorsConfiguration => this is NamedVectorsConfigurationDiff;

    /// <summary>
    /// Casts this instance to <see cref="NamedVectorsConfigurationDiff"/> multiple named vectors configuration diff.
    /// </summary>
    /// <exception cref="InvalidCastException">Occurs if this instance is not <see cref="NamedVectorsConfigurationDiff"/>.</exception>
    public NamedVectorsConfigurationDiff AsMultipleVectorsConfiguration()
    {
        if (this is NamedVectorsConfigurationDiff nvc)
        {
            return nvc;
        }

        throw new InvalidCastException(
            $"Can't cast {GetType()} to {typeof(NamedVectorsConfigurationDiff)}. Looks like this configuration is single vector configuration");
    }

    /// <summary>
    /// Casts this instance to <see cref="SingleVectorConfigurationDiff"/> single vector configuration diff.
    /// </summary>
    /// <exception cref="InvalidCastException">Occurs if this instance is not <see cref="SingleVectorConfigurationDiff"/>.</exception>
    public SingleVectorConfigurationDiff AsSingleVectorConfiguration()
    {
        if (this is SingleVectorConfigurationDiff svc)
        {
            return svc;
        }

        throw new InvalidCastException(
            $"Can't cast {GetType()} to {typeof(SingleVectorConfigurationDiff)}. Looks like this configuration is multiple named vectors configuration");
    }
}
