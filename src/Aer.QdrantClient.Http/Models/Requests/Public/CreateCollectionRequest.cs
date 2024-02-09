using System.Text.Json.Serialization;
using Aer.QdrantClient.Http.Infrastructure.Json.Converters;
using Aer.QdrantClient.Http.Models.Shared;

// ReSharper disable MemberCanBeInternal
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aer.QdrantClient.Http.Models.Requests.Public;

/// <summary>
/// The request for creating Qdrant collection.
/// </summary>
public sealed class CreateCollectionRequest
{
    #region Nested classes

    /// <summary>
    /// Represents other collection to copy data from configuration.
    /// </summary>
    public class InitFromCollection
    {
        /// <summary>
        /// Specify other collection name to copy data from.
        /// </summary>
        public string Collection { set; get; }

        /// <summary>
        /// Creates a new instance of <see cref="InitFromCollection"/> with specified collection name.
        /// </summary>
        /// <param name="sourceColelctionName">The name of the colelction to copy data from.</param>
        public static InitFromCollection ByName(string sourceColelctionName)
        {
            return new InitFromCollection()
            {
                Collection = sourceColelctionName
            };
        }
    }

    #endregion

    /// <summary>
    /// Gets the vectors configuration.
    /// </summary>
    /// <remarks>
    /// Currently only single vector mode collection creation is supported.
    /// </remarks>
    [JsonConverter(typeof(VectorConfigurationJsonConverter))]
    public VectorConfigurationBase Vectors { get; }

    /// <summary>
    /// Gets the sparse vectors configuration.
    /// </summary>
    public Dictionary<string, SparseVectorConfiguration> SparseVectors { get; set; }

    /// <summary>
    /// Number of shards in collection. Default is <c>1</c> for standalone,
    /// otherwise equal to the number of nodes. Minimum is <c>1</c>.
    /// </summary>
    public uint? ShardNumber { set; get; }

    /// <summary>
    /// The sharding method. Set to <see cref="ShardingMethod.Custom"/>
    /// to configure use cusom sharing configuration.
    /// </summary>
    /// <remarks>
    /// In this mode, the <see cref="ShardNumber"/> means the number of shards per shard key,
    /// where points will be distributed evenly.
    /// </remarks>
    public ShardingMethod? ShardingMethod { set; get; }

    /// <summary>
    /// Number of shards replicas. Default is 1 Minimum is 1.
    /// </summary>
    public uint? ReplicationFactor { get; set; }

    /// <summary>
    /// Defines how many replicas should apply the operation for us to consider it successful.
    /// Increasing this number will make the collection more resilient to inconsistencies,
    /// but will also make it fail if not enough replicas are available. Does not have any performance impact.
    /// </summary>
    public int? WriteConsistencyFactor { get; set; }

    /// <summary>
    /// If <c>true</c> - point's payload will not be stored in memory.
    /// It will be read from the disk every time it is requested.
    /// This setting saves RAM by (slightly) increasing the response time.
    /// Those payload values that are involved in filtering and are indexed - remain in RAM.
    /// </summary>
    public bool? OnDiskPayload { get; set; }

    /// <summary>
    /// Gets or sets the HNSW configuration.
    /// </summary>
    public HnswConfiguration HnswConfig { get; set; }

    /// <summary>
    /// Gets or sets the optimizers configuration.
    /// </summary>
    public OptimizersConfiguration OptimizersConfig { get; set; }

    /// <summary>
    /// Gets or sets the collection quantization configuration.
    /// </summary>
    [JsonConverter(typeof(QuantizationConfigurationJsonConverter))]
    public QuantizationConfiguration QuantizationConfig { get; set; }

    /// <summary>
    /// Specify other collection to copy data from.
    /// </summary>
    public InitFromCollection InitFrom { get; set; }

    /// <summary>Initializes a new instance of the <see cref="CreateCollectionRequest" /> class with singe vector
    /// or multiple named vectors with identical configuration.</summary>
    /// <param name="vectorDistanceMetric">The distance metric.</param>
    /// <param name="vectorSize">Size of the vector.</param>
    /// <param name="isServeVectorsFromDisk">If <c>true</c>, vectors are served from disk, improving RAM usage at the cost of latency.</param>
    /// <param name="namedVectorNames">The named vector names. All vectors share same size and distance metric.</param>
    /// <param name="vectorHnswConfiguration">Custom params for HNSW index. If none - values from collection configuration are used.</param>
    /// <param name="vectorQuantizationConfiguration">Custom params for quantization. If none - values from collection configuration are used.</param>
    public CreateCollectionRequest(
        VectorDistanceMetric vectorDistanceMetric,
        ulong vectorSize,
        bool isServeVectorsFromDisk,
        IEnumerable<string> namedVectorNames = null,
        HnswConfiguration vectorHnswConfiguration = null,
        QuantizationConfiguration vectorQuantizationConfiguration = null)
    {
        if (namedVectorNames is null)
        {
            Vectors = new VectorConfigurationBase.SingleVectorConfiguration(
                vectorDistanceMetric,
                vectorSize,
                isServeVectorsFromDisk,
                vectorHnswConfiguration,
                vectorQuantizationConfiguration);
        }
        else
        {
            Vectors = new VectorConfigurationBase.NamedVectorsConfiguration(
                vectorDistanceMetric,
                vectorSize,
                isServeVectorsFromDisk,
                namedVectorNames,
                vectorHnswConfiguration,
                vectorQuantizationConfiguration);
        }
    }

    /// <summary>Initializes a new instance of the <see cref="CreateCollectionRequest" /> class with
    /// multiple named vectors with different configurations for each named vector.</summary>
    /// <param name="namedVectorsConfiguration">The named vectors configurations.</param>
    public CreateCollectionRequest(
        Dictionary<string, VectorConfigurationBase.SingleVectorConfiguration> namedVectorsConfiguration)
    {
        Vectors = new VectorConfigurationBase.NamedVectorsConfiguration(namedVectorsConfiguration);
    }
}
