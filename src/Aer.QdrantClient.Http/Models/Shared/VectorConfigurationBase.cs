using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;

namespace Aer.QdrantClient.Http.Models.Shared;

/// <summary>
/// Represents a one vector or multiple named vectors collection configuration.
/// </summary>
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public abstract class VectorConfigurationBase
{
    #region Nested classes

    /// <summary>
    /// Represents a single vector configuration.
    /// </summary>
    public sealed class SingleVectorConfiguration : VectorConfigurationBase
    {
        /// <summary>
        /// The distance metric used to build collection index.
        /// </summary>
        public required string Distance { get; init; }

        /// <summary>
        /// The distance metric used to build collection index.
        /// </summary>
        [JsonIgnore]
        public VectorDistanceMetric DistanceMetric =>
#if NETSTANDARD2_0
            (VectorDistanceMetric) Enum.Parse(typeof(VectorDistanceMetric), Distance, ignoreCase: true);
#else
            Enum.Parse<VectorDistanceMetric>(Distance, ignoreCase: true);
#endif

        /// <summary>
        /// The vector elements count - vector dimensions.
        /// </summary>
        public required ulong Size { get; init; }

        /// <summary>
        /// Custom params for HNSW index. If none - values from collection configuration are used.
        /// </summary>
        public HnswConfiguration HnswConfig { get; init; }

        /// <summary>
        /// Custom params for quantization. If none - values from collection configuration are used.
        /// </summary>
        [JsonConverter(typeof(QuantizationConfigurationJsonConverter))]
        public QuantizationConfiguration QuantizationConfig { get; set; }

        /// <summary>
        /// If <c>true</c>, vectors are served from disk, improving RAM usage at the cost of latency.
        /// </summary>
        public bool OnDisk { get; init; }

        /// <summary>
        /// Defines which datatype should be used to represent vectors in the storage.
        /// Choosing different datatypes allows to optimize memory usage and performance vs accuracy.
        /// </summary>
        public VectorDataType Datatype { get; init; }

        /// <summary>
        /// The multivector configuration.
        /// </summary>
        public MultivectorConfiguration MultivectorConfig { get; init; }

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
        /// <param name="vectorDataType">The datatype that should be used to represent vectors in the storage.</param>
        /// <param name="multivectorConfiguration">The multivector configuration.</param>
        [SetsRequiredMembers]
        public SingleVectorConfiguration(
            VectorDistanceMetric vectorDistanceMetric,
            ulong vectorSize,
            bool isServeVectorsFromDisk,
            HnswConfiguration vectorHnswConfiguration = null,
            QuantizationConfiguration vectorQuantizationConfiguration = null,
            VectorDataType vectorDataType = VectorDataType.Float32,
            MultivectorConfiguration multivectorConfiguration = null)
        {
            Distance = vectorDistanceMetric.ToString();
            Size = vectorSize;
            OnDisk = isServeVectorsFromDisk;
            HnswConfig = vectorHnswConfiguration;
            QuantizationConfig = vectorQuantizationConfiguration;
            Datatype = vectorDataType;
            MultivectorConfig = multivectorConfiguration;
        }
    }

    /// <summary>
    /// Represents a configuration for collection with named vectors.
    /// </summary>
    public sealed class NamedVectorsConfiguration : VectorConfigurationBase
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
        /// <param name="vectorDataType">The datatype that should be used to represent vectors in the storage.</param>
        [SetsRequiredMembers]
        public NamedVectorsConfiguration(
            VectorDistanceMetric vectorDistanceMetric,
            ulong vectorSize,
            bool isServeVectorsFromDisk,
            IEnumerable<string> namedVectorNames,
            HnswConfiguration vectorHnswConfiguration,
            QuantizationConfiguration vectorQuantizationConfiguration,
            VectorDataType vectorDataType = VectorDataType.Float32)
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
                        vectorQuantizationConfiguration,
                        vectorDataType
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
    /// Casts this instance to <see cref="NamedVectorsConfiguration"/> multiple named vectors configuration.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidCastException">Occurs if this instance is not <see cref="NamedVectorsConfiguration"/></exception>
    public NamedVectorsConfiguration AsMultipleVectorsConfiguration()
    {
        if (this is NamedVectorsConfiguration nvc)
        {
            return nvc;
        }

        throw new InvalidCastException(
            $"Can't cast {GetType()} to {typeof(NamedVectorsConfiguration)}. Looks like this configuration is single vector configuration");
    }

    /// <summary>
    /// Casts this instance to <see cref="SingleVectorConfiguration"/> single vector configuration.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidCastException">Occurs if this instance is not <see cref="SingleVectorConfiguration"/></exception>
    public SingleVectorConfiguration AsSingleVectorConfiguration()
    {
        if (this is SingleVectorConfiguration svc)
        {
            return svc;
        }

        throw new InvalidCastException(
            $"Can't cast {GetType()} to {typeof(SingleVectorConfiguration)}. Looks like this configuration is multiple named vectors configuration");
    }
}

