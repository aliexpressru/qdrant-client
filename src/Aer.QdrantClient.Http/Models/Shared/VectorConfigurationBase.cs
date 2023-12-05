using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a one vector or multiple named vectors collection configuration.
/// </summary>
public abstract class VectorConfigurationBase
{
    #region Nested classes

    /// <summary>
    /// Represents a single vector configuration.
    /// </summary>
    public class SingleVectorConfiguration : VectorConfigurationBase
    {
        /// <summary>
        /// The distance metric used to build collection index.
        /// </summary>
        public required string Distance { get; init; }

        /// <summary>
        /// The vector elements count - vector dimensions.
        /// </summary>
        public required ulong Size { get; init; }

        /// <summary>
        /// Custom params for HNSW index. If none - values from collection configuration are used.
        /// </summary>
        public HnswConfiguration HnswConfig { get; set; }

        /// <summary>
        /// Custom params for quantization. If none - values from collection configuration are used.
        /// </summary>
        [JsonConverter(typeof(QuantizationConfigurationJsonConverter))]
        public QuantizationConfiguration QuantizationConfig { get; set; }

        /// <summary>
        /// If <c>true</c>, vectors are served from disk, improving RAM usage at the cost of latency.
        /// </summary>
        public bool OnDisk { get; set; }

        [JsonConstructor]
        internal SingleVectorConfiguration()
        { }

        /// <summary>
        /// The collection vector fields configuration.
        /// </summary>
        /// <param name="vectorDistanceMetric">The distance metric used to build collection index.</param>
        /// <param name="vectorSize">The vector elements count - vector dimensions.</param>
        /// <param name="isServeVectorsFromDisk">If <c>true</c>, vectors are served from disk, improving RAM usage at the cost of latency.</param>
        /// <param name="vectorHnswConfiguration">Custom params for HNSW index. If none - values from collection configuration are used.</param>
        /// <param name="vectorQuantizationConfiguration">Custom params for quantization. If none - values from collection configuration are used.</param>
        [SetsRequiredMembers]
        public SingleVectorConfiguration(
            VectorDistanceMetric vectorDistanceMetric,
            ulong vectorSize,
            bool isServeVectorsFromDisk,
            HnswConfiguration vectorHnswConfiguration = null,
            QuantizationConfiguration vectorQuantizationConfiguration = null)
        {
            Distance = vectorDistanceMetric.ToString();
            Size = vectorSize;
            OnDisk = isServeVectorsFromDisk;
            HnswConfig = vectorHnswConfiguration;
            QuantizationConfig = vectorQuantizationConfiguration;
        }
    }

    /// <summary>
    /// Represents a configuration for collection with named vectors.
    /// </summary>
    public class NamedVectorsConfiguration : VectorConfigurationBase
    {
        /// <summary>
        /// The named vectors configurations,
        /// </summary>
        public required Dictionary<string, SingleVectorConfiguration> NamedVectors { init; get; }

        [JsonConstructor]
        internal NamedVectorsConfiguration()
        { }

        /// <summary>
        /// Initializes new instance of <see cref="VectorConfigurationBase.NamedVectorsConfiguration"/> with specified different vectors configurations.
        /// </summary>
        /// <param name="namedVectorsConfiguration"></param>
        [SetsRequiredMembers]
        public NamedVectorsConfiguration(Dictionary<string, SingleVectorConfiguration> namedVectorsConfiguration)
        {
            NamedVectors = namedVectorsConfiguration;
        }

        /// <summary>
        /// Initializes new instance of <see cref="VectorConfigurationBase.NamedVectorsConfiguration"/> with specified named vectors with identical configurations.
        /// </summary>
        /// <param name="vectorDistanceMetric">The distance metric used to build collection index.</param>
        /// <param name="vectorSize">The vector elements count - vector dimensions.</param>
        /// <param name="isServeVectorsFromDisk">If <c>true</c>, vectors are served from disk, improving RAM usage at the cost of latency.</param>
        /// <param name="namedVectorNames">The named vector names. All vectors share same size and distance metric.</param>
        /// <param name="vectorHnswConfiguration">Custom params for HNSW index. If none - values from collection configuration are used.</param>
        /// <param name="vectorQuantizationConfiguration">Custom params for quantization. If none - values from collection configuration are used.</param>
        [SetsRequiredMembers]
        public NamedVectorsConfiguration(
            VectorDistanceMetric vectorDistanceMetric,
            ulong vectorSize,
            bool isServeVectorsFromDisk,
            IEnumerable<string> namedVectorNames,
            HnswConfiguration vectorHnswConfiguration,
            QuantizationConfiguration vectorQuantizationConfiguration)
        {
            Dictionary<string, SingleVectorConfiguration> namedVectors = new();

            foreach (var vectorName in namedVectorNames)
            {
                namedVectors.Add(
                    vectorName,
                    new SingleVectorConfiguration(
                        vectorDistanceMetric,
                        vectorSize,
                        isServeVectorsFromDisk,
                        vectorHnswConfiguration,
                        vectorQuantizationConfiguration
                    )
                );
            }

            NamedVectors = namedVectors;
        }
    }

    #endregion

    /// <summary>
    /// <c>true</c> if this instance describes multiple vectors configuration.
    /// <c>false</c> if this instance describes single vector configuration.
    /// </summary>
    public bool IsMultipleVectorsConfiguration => this is NamedVectorsConfiguration;

    /// <summary>
    /// Casts this instance to <see cref="NamedVectorsConfiguration"/> multiple named vectors configuraiton.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidCastException">Occures if this instance is not <see cref="NamedVectorsConfiguration"/></exception>
    public NamedVectorsConfiguration AsMultipleVectorsConfiguration()
    {
        if (this is NamedVectorsConfiguration nvc)
        {
            return nvc;
        }

        throw new InvalidCastException(
            $"Can't cast {GetType()} to {typeof(NamedVectorsConfiguration)}. Looks like this configuraiton is single vector configuration");
    }

    /// <summary>
    /// Casts this instance to <see cref="SingleVectorConfiguration"/> single vector configuration.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidCastException">Occures if this instance is not <see cref="SingleVectorConfiguration"/></exception>
    public SingleVectorConfiguration AsSingleVectorConfiguration()
    {
        if (this is SingleVectorConfiguration svc)
        {
            return svc;
        }

        throw new InvalidCastException(
            $"Can't cast {GetType()} to {typeof(SingleVectorConfiguration)}. Looks like this configuraiton is multiple named vectors configuration");
    }
}

